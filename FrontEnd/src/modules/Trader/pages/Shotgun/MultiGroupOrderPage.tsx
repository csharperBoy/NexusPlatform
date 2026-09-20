import { useEffect, useMemo, useState } from "react";
import {
  useSymbolsStore,
  useAccountsStore,
  useLogStore,
} from "../../stores";
import { useServerClock, useShotgunScheduler } from "../../hooks";
import { PriceSuggestions, LogBox, Countdown } from "../../components";
import { TIME_PRESETS } from "../../constants";
import { uid } from "../../utils";
import type { Symbol } from "../../models";

interface DraftOrder {
  id: string;
  isin: string;
  price: string;
  quantity: string;
  time: string;
}

interface MultiAccount {
  id: string;
  savedAccountId: string | null;
  bulkTime: string;
  orders: DraftOrder[];
}

const makeDraft = (sym: Symbol, overrides: Partial<DraftOrder> = {}): DraftOrder => ({
  id: uid(),
  isin: sym.symbolIsin,
  price: String(sym.price),
  quantity: String(sym.quantity),
  time: TIME_PRESETS[1] ?? "08:45:00.000",
  ...overrides,
});

const makeAccount = (sym: Symbol, savedAccountId: string | null): MultiAccount => ({
  id: uid(),
  savedAccountId,
  bulkTime: TIME_PRESETS[1] ?? "08:45:00.000",
  orders: [makeDraft(sym)],
});

export function MultiGroupOrderPage() {
  const symbols = useSymbolsStore((s) => s.symbols);
  const getSymbol = useSymbolsStore((s) => s.getSymbol);
  const savedAccounts = useAccountsStore((s) => s.accounts);
  const getSavedAccount = useAccountsStore((s) => s.getAccount);

  const [accounts, setAccounts] = useState<MultiAccount[]>([]);
  const appendLog = useLogStore((s) => s.append);

  /* ─── seed / fix multi-accounts ─── */
  useEffect(() => {
    if (savedAccounts.length === 0 || symbols.length === 0) return;
    setAccounts((prev) => {
      let changed = false;
      const next = prev.map((acc) => {
        if (acc.savedAccountId) {
          const still = savedAccounts.find((s) => s.id === acc.savedAccountId);
          if (still) return acc;
        }
        changed = true;
        return { ...acc, savedAccountId: savedAccounts[0].id };
      });
      if (prev.length === 0) {
        changed = true;
        next.push(makeAccount(symbols[0], savedAccounts[0].id));
      }
      return changed ? next : prev;
    });
  }, [savedAccounts, symbols]);

  const tokenFor = (acc: MultiAccount) =>
    getSavedAccount(acc.savedAccountId)?.token ?? "";
  const nameFor = (acc: MultiAccount) =>
    getSavedAccount(acc.savedAccountId)?.name ?? "—";

  const firstToken = useMemo(() => {
    for (const a of accounts) {
      const t = tokenFor(a);
      if (t.trim()) return t;
    }
    return "";
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [accounts, savedAccounts]);

  const clock = useServerClock(firstToken);
  const scheduler = useShotgunScheduler({ clock, onLog: appendLog });

  /* ─── CRUD ─── */
  const updateAccount = (id: string, patch: Partial<MultiAccount>) =>
    setAccounts((list) => list.map((a) => (a.id === id ? { ...a, ...patch } : a)));
  const addAccount = () =>
    setAccounts((list) => [
      ...list,
      makeAccount(symbols[0], savedAccounts[list.length]?.id ?? savedAccounts[0]?.id ?? null),
    ]);
  const removeAccount = (id: string) =>
    setAccounts((list) => (list.length > 1 ? list.filter((a) => a.id !== id) : list));

  const addOrderTo = (accId: string) =>
    setAccounts((list) =>
      list.map((a) =>
        a.id === accId ? { ...a, orders: [...a.orders, makeDraft(symbols[0])] } : a,
      ),
    );
  const removeOrderFrom = (accId: string, oid: string) =>
    setAccounts((list) =>
      list.map((a) =>
        a.id === accId && a.orders.length > 1
          ? { ...a, orders: a.orders.filter((o) => o.id !== oid) }
          : a,
      ),
    );
  const updateOrderIn = (accId: string, oid: string, patch: Partial<DraftOrder>) =>
    setAccounts((list) =>
      list.map((a) =>
        a.id === accId
          ? { ...a, orders: a.orders.map((o) => (o.id === oid ? { ...o, ...patch } : o)) }
          : a,
      ),
    );
  const applyBulkTime = (accId: string) => {
    const acc = accounts.find((a) => a.id === accId);
    if (!acc) return;
    updateAccount(accId, {
      orders: acc.orders.map((o) => ({ ...o, time: acc.bulkTime })),
    });
    appendLog(
      `⏱ [${nameFor(acc)}]: زمان ${acc.bulkTime} روی ${acc.orders.length} سفارش اعمال شد`,
      "info",
    );
  };
  const copyOrdersToAll = (sourceId: string) => {
    const src = accounts.find((a) => a.id === sourceId);
    if (!src) return;
    const template = src.orders.map((o) => ({
      isin: o.isin,
      price: o.price,
      quantity: o.quantity,
      time: o.time,
    }));
    setAccounts((list) =>
      list.map((a) => {
        if (a.id === sourceId) return a;
        return { ...a, orders: template.map((t) => ({ ...t, id: uid() })) };
      }),
    );
    appendLog(
      `📋 سفارش‌های "${nameFor(src)}" روی ${accounts.length - 1} حساب دیگر کپی شد`,
      "info",
    );
  };

  if (symbols.length === 0) {
    return (
      <div className="rounded-xl bg-slate-900 p-6 text-center">
        <div className="mb-2 text-slate-300">هنوز نمادی تعریف نشده</div>
      </div>
    );
  }
  if (savedAccounts.length === 0) {
    return (
      <div className="rounded-xl bg-slate-900 p-6 text-center">
        <div className="mb-2 text-slate-300">حسابی تعریف نشده</div>
        <div className="text-xs text-slate-500">برو به «اطلاعات پایه»</div>
      </div>
    );
  }

  const handleArm = () => {
    const tasks = [];
    for (const acc of accounts) {
      const tk = tokenFor(acc);
      if (!tk.trim()) {
        appendLog(`توکن [${nameFor(acc)}] خالیه — برو به اطلاعات پایه`, "err");
        return;
      }
      for (const o of acc.orders) {
        const sym = getSymbol(o.isin);
        if (!sym) continue;
        tasks.push({
          accountId: acc.id,
          accountName: nameFor(acc),
          token: tk,
          symbol: sym,
          price: Number(o.price),
          quantity: Number(o.quantity),
          time: o.time,
        });
      }
    }
    scheduler.arm(tasks);
  };

  const totalOrders = accounts.reduce((s, a) => s + a.orders.length, 0);

  return (
    <div className="space-y-4 rounded-xl border border-slate-800 bg-slate-900 p-4">
      <h1 className="text-lg font-bold text-slate-200">سرخطی چندحساب</h1>

      <div className="flex items-center justify-between text-xs text-slate-400">
        <span>
          {accounts.length} حساب — {totalOrders} سفارش
        </span>
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-blue-300 hover:bg-slate-700"
          onClick={addAccount}
          disabled={scheduler.status !== "idle"}
        >
          + افزودن حساب
        </button>
      </div>

      {/* Sync */}
      <div className="flex items-center gap-3">
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700 disabled:opacity-50"
          disabled={scheduler.status !== "idle" || clock.info.busy}
          onClick={() => clock.sync(5)}
        >
          {clock.info.busy ? "⏳ در حال سینک..." : "⏰ سینک سریع"}
        </button>
        <span className="text-[11px] text-slate-500">
          diff: {clock.info.diff ?? "—"}ms | samples: {clock.info.samples}
        </span>
      </div>

      {/* Accounts */}
      {accounts.map((acc, idx) => {
        const token = tokenFor(acc);
        return (
          <div
            key={acc.id}
            className="space-y-3 rounded-xl border border-blue-900/60 bg-slate-950 p-3"
          >
            <div className="flex items-center justify-between text-xs">
              <span className="text-slate-500">#{idx + 1}</span>
              <span className="font-semibold text-blue-300">{nameFor(acc)}</span>
              <button
                type="button"
                className="rounded border border-red-900 px-2 py-0.5 text-red-400 hover:bg-red-950 disabled:opacity-30"
                onClick={() => removeAccount(acc.id)}
                disabled={scheduler.status !== "idle" || accounts.length === 1}
              >
                ✕
              </button>
            </div>

            <select
              className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 text-sm"
              value={acc.savedAccountId ?? ""}
              onChange={(e) => updateAccount(acc.id, { savedAccountId: e.target.value })}
              disabled={scheduler.status !== "idle"}
            >
              {savedAccounts.map((sa) => (
                <option key={sa.id} value={sa.id}>
                  {sa.name}
                  {sa.token?.trim() ? "" : " (بدون توکن)"}
                </option>
              ))}
            </select>
            <div className={`text-xs ${token.trim() ? "text-green-400" : "text-red-400"}`}>
              {token.trim() ? "✅ توکن از اطلاعات پایه لود شد" : "⚠️ توکن تنظیم نشده"}
            </div>

            <div className="flex gap-2">
              <input
                className="flex-1 rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 font-mono text-xs"
                value={acc.bulkTime}
                onChange={(e) => updateAccount(acc.id, { bulkTime: e.target.value })}
                disabled={scheduler.status !== "idle"}
              />
              <button
                type="button"
                className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
                onClick={() => applyBulkTime(acc.id)}
                disabled={scheduler.status !== "idle"}
              >
                اعمال زمان
              </button>
              <button
                type="button"
                className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700 disabled:opacity-50"
                onClick={() => copyOrdersToAll(acc.id)}
                disabled={scheduler.status !== "idle" || accounts.length < 2}
              >
                کپی به بقیه
              </button>
            </div>

            {/* Orders */}
            {acc.orders.map((o, oidx) => {
              const sym = getSymbol(o.isin);
              return (
                <div
                  key={o.id}
                  className="space-y-2 rounded-lg border border-slate-700 bg-slate-900 p-2"
                >
                  <div className="flex items-center justify-between text-xs">
                    <span className="text-slate-500">سفارش {oidx + 1}</span>
                    <span className="font-semibold text-blue-300">
                      {sym?.symbolName ?? "—"}
                    </span>
                    <button
                      type="button"
                      className="rounded border border-red-900 px-1.5 py-0.5 text-red-400 hover:bg-red-950 disabled:opacity-30"
                      onClick={() => removeOrderFrom(acc.id, o.id)}
                      disabled={scheduler.status !== "idle" || acc.orders.length === 1}
                    >
                      ✕
                    </button>
                  </div>

                  <select
                    className="w-full rounded-lg border border-slate-700 bg-slate-950 px-2 py-1 text-xs"
                    value={o.isin}
                    onChange={(e) => {
                      const s = symbols.find((x) => x.symbolIsin === e.target.value);
                      if (!s) return;
                      updateOrderIn(acc.id, o.id, {
                        isin: s.symbolIsin,
                        price: String(s.price),
                        quantity: String(s.quantity),
                      });
                    }}
                    disabled={scheduler.status !== "idle"}
                  >
                    {symbols.map((s) => (
                      <option key={s.symbolIsin} value={s.symbolIsin}>
                        {s.symbolName}
                      </option>
                    ))}
                  </select>

                  <div className="grid grid-cols-3 gap-2">
                    <input
                      type="number"
                      className="rounded-lg border border-slate-700 bg-slate-950 px-2 py-1 text-xs"
                      value={o.price}
                      onChange={(e) => updateOrderIn(acc.id, o.id, { price: e.target.value })}
                      disabled={scheduler.status !== "idle"}
                      placeholder="قیمت"
                    />
                    <input
                      type="number"
                      className="rounded-lg border border-slate-700 bg-slate-950 px-2 py-1 text-xs"
                      value={o.quantity}
                      onChange={(e) => updateOrderIn(acc.id, o.id, { quantity: e.target.value })}
                      disabled={scheduler.status !== "idle"}
                      placeholder="حجم"
                    />
                    <input
                      type="text"
                      className="rounded-lg border border-slate-700 bg-slate-950 px-2 py-1 font-mono text-[11px]"
                      value={o.time}
                      onChange={(e) => updateOrderIn(acc.id, o.id, { time: e.target.value })}
                      disabled={scheduler.status !== "idle"}
                      placeholder="زمان"
                    />
                  </div>

                  {sym && (
                    <PriceSuggestions
                      isin={sym.symbolIsin}
                      token={token}
                      side={sym.side}
                      onPick={(p) => updateOrderIn(acc.id, o.id, { price: String(p) })}
                      disabled={scheduler.status !== "idle"}
                    />
                  )}
                </div>
              );
            })}

            <button
              type="button"
              className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
              onClick={() => addOrderTo(acc.id)}
              disabled={scheduler.status !== "idle"}
            >
              + افزودن سفارش به این حساب
            </button>
          </div>
        );
      })}

      <Countdown remaining={scheduler.remaining} nextLabel={scheduler.nextLabel} />

      <div className="flex gap-2">
        <button
          type="button"
          className="flex-1 rounded-lg bg-green-600 py-2.5 text-sm font-bold text-slate-950 hover:bg-green-500 disabled:opacity-50"
          onClick={handleArm}
          disabled={scheduler.status !== "idle"}
        >
          {scheduler.status === "armed" ? "در انتظار..." : "مسلح کن و همه رو شلیک کن"}
        </button>
        {scheduler.status !== "idle" && (
          <button
            type="button"
            className="rounded-lg bg-red-800 px-4 py-2.5 text-sm font-bold text-white hover:bg-red-700"
            onClick={scheduler.cancel}
          >
            لغو
          </button>
        )}
      </div>

      <LogBox />
    </div>
  );
}