import { useEffect, useMemo, useState } from "react";
import {
  useSymbolsStore,
  useAccountsStore,
  useLogStore,
  useLoginStore,
} from "../../stores";
import {
  useServerClock,
  useShotgunScheduler,
} from "../../hooks";
import {
  AccountPicker,
  PriceSuggestions,
  LogBox,
  Countdown,
} from "../../components";
import { TIME_PRESETS } from "../../constants";
import { parseOrderJson } from "../../utils/parseOrderJson";
import type { Symbol } from "../../models";

export function SingleOrderPage() {
  const symbols = useSymbolsStore((s) => s.symbols);
  const accounts = useAccountsStore((s) => s.accounts);
  const getTokenStatus = useLoginStore((s) => s.isBusy);

  const [selectedAccountId, setSelectedAccountId] = useState<string | null>(null);
  const [order, setOrder] = useState<Symbol | null>(null);
  const [timeStr, setTimeStr] = useState(TIME_PRESETS[1] ?? "08:45:00.000");

  const appendLog = useLogStore((s) => s.append);

  /* auto-select اولین حساب و نماد */
  useEffect(() => {
    if (!selectedAccountId && accounts.length > 0)
      setSelectedAccountId(accounts[0].id);
  }, [accounts, selectedAccountId]);

  useEffect(() => {
    if (!order && symbols.length > 0) setOrder(symbols[0]);
    if (
      order &&
      !symbols.find((s) => s.symbolIsin === order.symbolIsin) &&
      symbols.length > 0
    ) {
      setOrder(symbols[0]);
    }
  }, [symbols, order]);

  const currentAccount = useMemo(
    () => accounts.find((a) => a.id === selectedAccountId) ?? null,
    [accounts, selectedAccountId],
  );
  const token = currentAccount?.token ?? "";

  const clock = useServerClock(token);
  const scheduler = useShotgunScheduler({ clock, onLog: appendLog });

  if (!order) {
    return (
      <div className="rounded-xl bg-slate-900 p-6 text-center">
        <div className="mb-2 text-slate-300">هنوز نمادی تعریف نشده</div>
        <div className="text-xs text-slate-500">
          برو به تب «اطلاعات پایه» و یه JSON اضافه کن
        </div>
      </div>
    );
  }
const fireNow = async () => {
  if (!currentAccount || !order) return;
  const fireAt = new Date(Date.now() + 2500);
  const hms = `${String(fireAt.getHours()).padStart(2, "0")}:${String(fireAt.getMinutes()).padStart(2, "0")}:${String(fireAt.getSeconds()).padStart(2, "0")}.000`;
  scheduler.arm([{
    accountId: currentAccount.id,
    accountName: currentAccount.name,
    token,
    symbol: order,
    price: Number(order.price),
    quantity: Number(order.quantity),
    time: hms,
  }]);
};
  const setField = <K extends keyof Symbol>(k: K, v: Symbol[K]) =>
    setOrder((o) => (o ? { ...o, [k]: v } : o));

  const total = Number(order.price || 0) * Number(order.quantity || 0) * (1 + order.commission);

  const handleArm = () => {
    if (!currentAccount) return appendLog("حساب انتخاب نشده", "err");
    if (!token.trim()) return appendLog("توکن حساب خالیه", "err");

    scheduler.arm([
      {
        accountId: currentAccount.id,
        accountName: currentAccount.name,
        token,
        symbol: order,
        price: Number(order.price),
        quantity: Number(order.quantity),
        time: timeStr,
      },
    ]);
  };

  return (
    <div className="space-y-4 rounded-xl border border-slate-800 bg-slate-900 p-4">
      <h1 className="text-lg font-bold text-slate-200">
        سرخطی تک‌سفارش — {order.symbolName} ({order.symbolIsin})
      </h1>

      <AccountPicker
        value={selectedAccountId}
        onChange={setSelectedAccountId}
        disabled={scheduler.status !== "idle"}
      />

      {/* Sync clock */}
      <div className="flex flex-wrap items-center gap-3">
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700 disabled:opacity-50"
          disabled={scheduler.status !== "idle" || clock.info.busy}
          onClick={async () => {
            appendLog("⏰ درخواست سینک دستی...", "info");
            const r = await clock.sync(5);
            if (r === null)
              appendLog(
                `سینک انجام نشد — ${clock.info.lastError || "دلیل نامشخص"}`,
                "err",
              );
          }}
        >
          {clock.info.busy ? "⏳ در حال سینک..." : "⏰ سینک سریع (۵ نمونه)"}
        </button>
        <span
          className={`text-[11px] ${
            clock.info.lastError ? "text-red-400" : "text-slate-500"
          }`}
        >
          {clock.info.busy && "⏳ در حال سینک... "}
          {clock.info.lastError
            ? `⚠️ ${clock.info.lastError}`
            : `diff: ${clock.info.diff ?? "—"}ms | samples: ${clock.info.samples} | RTT: ${clock.info.lastRtt ?? "—"}ms`}
        </span>
      </div>

      {/* Symbol picker */}
      <div>
        <label className="mb-1 block text-xs text-slate-400">نماد</label>
        <select
          className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
          value={order.symbolIsin}
          onChange={(e) => {
            const s = symbols.find((x) => x.symbolIsin === e.target.value);
            if (s) setOrder({ ...s });
          }}
          disabled={scheduler.status !== "idle"}
        >
          {symbols.map((s) => (
            <option key={s.symbolIsin} value={s.symbolIsin}>
              {s.symbolName} — {s.symbolIsin}
            </option>
          ))}
        </select>
      </div>

      {/* Price + Quantity */}
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="mb-1 block text-xs text-slate-400">قیمت (ریال)</label>
          <input
            type="number"
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            value={order.price}
            onChange={(e) => setField("price", Number(e.target.value))}
            disabled={scheduler.status !== "idle"}
          />
          <PriceSuggestions
            isin={order.symbolIsin}
            token={token}
            side={order.side}
            onPick={(p) => setField("price", p)}
            disabled={scheduler.status !== "idle"}
          />
        </div>
        <div>
          <label className="mb-1 block text-xs text-slate-400">حجم</label>
          <input
            type="number"
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            value={order.quantity}
            onChange={(e) => setField("quantity", Number(e.target.value))}
            disabled={scheduler.status !== "idle"}
          />
        </div>
      </div>

      {/* Time */}
      <div>
        <label className="mb-1 block text-xs text-slate-400">
          زمان ارسال (HH:MM:SS.mmm)
        </label>
        <input
          type="text"
          className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-sm"
          value={timeStr}
          onChange={(e) => setTimeStr(e.target.value)}
          disabled={scheduler.status !== "idle"}
        />
        <div className="mt-2 flex flex-wrap gap-2">
          {TIME_PRESETS.map((t) => (
            <button
              key={t}
              type="button"
              className="rounded-lg border border-slate-700 bg-slate-800 px-2 py-1 font-mono text-[11px] text-blue-300 hover:bg-slate-700 disabled:opacity-50"
              onClick={() => setTimeStr(t)}
              disabled={scheduler.status !== "idle"}
            >
              {t}
            </button>
          ))}
        </div>
      </div>

      <div className="rounded-lg bg-slate-950 px-3 py-2 text-center text-xs text-slate-400">
        مجموع تقریبی:{" "}
        <b className="text-slate-200">{Math.round(total).toLocaleString("fa-IR")}</b>{" "}
        ریال (کارمزد {(order.commission * 100).toFixed(4)}%)
      </div>

      <Countdown remaining={scheduler.remaining} nextLabel={scheduler.nextLabel} />

      <div className="flex gap-2">
        <button
          type="button"
          className="flex-1 rounded-lg bg-green-600 py-2.5 text-sm font-bold text-slate-950 hover:bg-green-500 disabled:opacity-50"
          onClick={handleArm}
          disabled={scheduler.status !== "idle"}
        >
          {scheduler.status === "armed" ? "در انتظار..." : "مسلح کن و سر وقت بفرست"}
        </button>
        <button
          type="button"
          className="flex-1 rounded-lg bg-slate-800 py-2.5 text-sm font-semibold text-blue-300 hover:bg-slate-700 disabled:opacity-50"
          disabled={scheduler.status !== "idle"}
          onClick={fireNow}
        >
          الان بفرست (تست)
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