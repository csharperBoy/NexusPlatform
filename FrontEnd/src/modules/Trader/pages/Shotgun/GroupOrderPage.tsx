import { useEffect, useMemo, useState } from "react";
import {
  useSymbolsStore,
  useAccountsStore,
  useLogStore,
  useLoginStore,
} from "../../stores";
import { useServerClock, useShotgunScheduler } from "../../hooks";
import {
  AccountPicker,
  PriceSuggestions,
  LogBox,
  Countdown,
} from "../../components";
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

const makeDraft = (sym: Symbol, overrides: Partial<DraftOrder> = {}): DraftOrder => ({
  id: uid(),
  isin: sym.symbolIsin,
  price: String(sym.price),
  quantity: String(sym.quantity),
  time: TIME_PRESETS[1] ?? "08:45:00.000",
  ...overrides,
});

export function GroupOrderPage() {
  const symbols = useSymbolsStore((s) => s.symbols);
  const getSymbol = useSymbolsStore((s) => s.getSymbol);
  const accounts = useAccountsStore((s) => s.accounts);

  const [selectedAccountId, setSelectedAccountId] = useState<string | null>(null);
  const [orders, setOrders] = useState<DraftOrder[]>([]);
  const [bulkTime, setBulkTime] = useState(TIME_PRESETS[1] ?? "08:45:00.000");

  const appendLog = useLogStore((s) => s.append);

  useEffect(() => {
    if (!selectedAccountId && accounts.length > 0)
      setSelectedAccountId(accounts[0].id);
  }, [accounts, selectedAccountId]);

  useEffect(() => {
    if (symbols.length === 0) return;
    setOrders((prev) => {
      if (prev.length === 0) return [makeDraft(symbols[0])];
      let changed = false;
      const next = prev.map((o) => {
        if (symbols.find((s) => s.symbolIsin === o.isin)) return o;
        changed = true;
        const s = symbols[0];
        return {
          ...o,
          isin: s.symbolIsin,
          price: String(s.price),
          quantity: String(s.quantity),
        };
      });
      return changed ? next : prev;
    });
  }, [symbols]);

  const currentAccount = useMemo(
    () => accounts.find((a) => a.id === selectedAccountId) ?? null,
    [accounts, selectedAccountId],
  );
  const token = currentAccount?.token ?? "";

  const clock = useServerClock(token);
  const scheduler = useShotgunScheduler({ clock, onLog: appendLog });

  if (symbols.length === 0) {
    return (
      <div className="rounded-xl bg-slate-900 p-6 text-center">
        <div className="mb-2 text-slate-300">هنوز نمادی تعریف نشده</div>
        <div className="text-xs text-slate-500">برو به «اطلاعات پایه»</div>
      </div>
    );
  }

  const updateOrder = (id: string, patch: Partial<DraftOrder>) =>
    setOrders((os) => os.map((o) => (o.id === id ? { ...o, ...patch } : o)));
  const addOrder = () => setOrders((os) => [...os, makeDraft(symbols[0])]);
  const removeOrder = (id: string) =>
    setOrders((os) => (os.length > 1 ? os.filter((o) => o.id !== id) : os));
  const applyTimeToAll = () => {
    setOrders((os) => os.map((o) => ({ ...o, time: bulkTime })));
    appendLog(`⏱ زمان "${bulkTime}" روی ${orders.length} سفارش اعمال شد`, "info");
  };

  const grandTotal = orders.reduce((sum, o) => {
    const sym = getSymbol(o.isin);
    if (!sym) return sum;
    const p = Number(o.price) || 0;
    const q = Number(o.quantity) || 0;
    return sum + p * q * (1 + sym.commission);
  }, 0);

  const handleArm = () => {
    if (!currentAccount) return appendLog("حساب انتخاب نشده", "err");
    if (!token.trim()) return appendLog("توکن حساب خالیه", "err");

    const tasks = orders.map((o) => {
      const sym = getSymbol(o.isin);
      if (!sym) throw new Error(`نماد ${o.isin} پیدا نشد`);
      return {
        accountId: currentAccount.id,
        accountName: currentAccount.name,
        token,
        symbol: sym,
        price: Number(o.price),
        quantity: Number(o.quantity),
        time: o.time,
      };
    });

    scheduler.arm(tasks);
  };

  return (
    <div className="space-y-4 rounded-xl border border-slate-800 bg-slate-900 p-4">
      <h1 className="text-lg font-bold text-slate-200">سرخطی چندسفارش</h1>

      <AccountPicker
        value={selectedAccountId}
        onChange={setSelectedAccountId}
        disabled={scheduler.status !== "idle"}
      />

      <div className="flex flex-wrap items-center gap-3">
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700 disabled:opacity-50"
          disabled={scheduler.status !== "idle" || clock.info.busy}
          onClick={async () => {
            appendLog("⏰ درخواست سینک دستی...", "info");
            await clock.sync(5);
          }}
        >
          {clock.info.busy ? "⏳ در حال سینک..." : "⏰ سینک سریع"}
        </button>
        <span className="text-[11px] text-slate-500">
          diff: {clock.info.diff ?? "—"}ms | samples: {clock.info.samples}
        </span>
      </div>

      {/* Bulk time */}
      <div className="rounded-lg border border-slate-700 bg-slate-950 p-3">
        <label className="mb-1 block text-xs text-slate-400">زمان گروهی</label>
        <div className="flex gap-2">
          <input
            className="flex-1 rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 font-mono text-sm"
            value={bulkTime}
            onChange={(e) => setBulkTime(e.target.value)}
            disabled={scheduler.status !== "idle"}
          />
          <button
            type="button"
            className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
            onClick={applyTimeToAll}
            disabled={scheduler.status !== "idle"}
          >
            اعمال به همه
          </button>
        </div>
        <div className="mt-2 flex flex-wrap gap-2">
          {TIME_PRESETS.map((t) => (
            <button
              key={t}
              type="button"
              className="rounded-lg border border-slate-700 bg-slate-800 px-2 py-1 font-mono text-[11px] text-blue-300 hover:bg-slate-700"
              onClick={() => setBulkTime(t)}
              disabled={scheduler.status !== "idle"}
            >
              {t}
            </button>
          ))}
        </div>
      </div>

      {/* Orders head */}
      <div className="flex items-center justify-between text-xs text-slate-400">
        <span>سفارش‌ها ({orders.length})</span>
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-blue-300 hover:bg-slate-700"
          onClick={addOrder}
          disabled={scheduler.status !== "idle"}
        >
          + افزودن سفارش
        </button>
      </div>

      {/* Orders */}
      {orders.map((o, idx) => {
        const sym = getSymbol(o.isin);
        const rowTotal =
          (Number(o.price) || 0) * (Number(o.quantity) || 0) * (1 + (sym?.commission ?? 0));
        return (
          <div
            key={o.id}
            className="space-y-2 rounded-xl border border-slate-700 bg-slate-950 p-3"
          >
            <div className="flex items-center justify-between text-xs">
              <span className="text-slate-500">#{idx + 1}</span>
              <span className="font-semibold text-blue-300">
                {sym?.symbolName ?? "—"}
              </span>
              <button
                type="button"
                className="rounded border border-red-900 px-2 py-0.5 text-red-400 hover:bg-red-950 disabled:opacity-30"
                onClick={() => removeOrder(o.id)}
                disabled={scheduler.status !== "idle" || orders.length === 1}
              >
                ✕
              </button>
            </div>

            <select
              className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 text-sm"
              value={o.isin}
              onChange={(e) => {
                const s = symbols.find((x) => x.symbolIsin === e.target.value);
                if (!s) return;
                updateOrder(o.id, {
                  isin: s.symbolIsin,
                  price: String(s.price),
                  quantity: String(s.quantity),
                });
              }}
              disabled={scheduler.status !== "idle"}
            >
              {symbols.map((s) => (
                <option key={s.symbolIsin} value={s.symbolIsin}>
                  {s.symbolName} — {s.symbolIsin}
                </option>
              ))}
            </select>

            <div className="grid grid-cols-3 gap-2">
              <div>
                <label className="mb-1 block text-xs text-slate-400">قیمت</label>
                <input
                  type="number"
                  className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 text-sm"
                  value={o.price}
                  onChange={(e) => updateOrder(o.id, { price: e.target.value })}
                  disabled={scheduler.status !== "idle"}
                />
              </div>
              <div>
                <label className="mb-1 block text-xs text-slate-400">حجم</label>
                <input
                  type="number"
                  className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 text-sm"
                  value={o.quantity}
                  onChange={(e) => updateOrder(o.id, { quantity: e.target.value })}
                  disabled={scheduler.status !== "idle"}
                />
              </div>
              <div>
                <label className="mb-1 block text-xs text-slate-400">زمان</label>
                <input
                  type="text"
                  className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 font-mono text-xs"
                  value={o.time}
                  onChange={(e) => updateOrder(o.id, { time: e.target.value })}
                  disabled={scheduler.status !== "idle"}
                />
              </div>
            </div>

            {sym && (
              <PriceSuggestions
                isin={sym.symbolIsin}
                token={token}
                side={sym.side}
                onPick={(p) => updateOrder(o.id, { price: String(p) })}
                disabled={scheduler.status !== "idle"}
              />
            )}

            <div className="text-left text-[11px] text-slate-500">
              مجموع: <b className="text-slate-300">{Math.round(rowTotal).toLocaleString("fa-IR")}</b> ریال
            </div>
          </div>
        );
      })}

      <div className="rounded-lg bg-slate-950 px-3 py-2 text-center text-xs text-slate-400">
        مجموع کل:{" "}
        <b className="text-slate-200">{Math.round(grandTotal).toLocaleString("fa-IR")}</b>{" "}
        ریال
      </div>

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