import React, { useEffect, useMemo, useState } from "react";
import { signalApi } from "../../api/signalApi";
import { tradingRunApi } from "../../api/tradingRunApi";
import type { SignalView, TradingRunInfoView } from "../../models";
import {
  SignalAction,
  SIGNAL_ACTION_LABELS,
  SIGNAL_ACTION_COLORS,
} from "../../models";

const ActionBadge: React.FC<{ action: number }> = ({ action }) => {
  const a = action as SignalAction;
  return (
    <span
      className={`inline-flex rounded-full px-2 py-1 text-xs font-medium whitespace-nowrap ${
        SIGNAL_ACTION_COLORS[a] ?? "bg-slate-700 text-slate-300"
      }`}
    >
      {SIGNAL_ACTION_LABELS[a] ?? "نامشخص"}
    </span>
  );
};

const StrengthBar: React.FC<{ value: number }> = ({ value }) => {
  const pct = Math.round(value * 100);
  const color =
    value >= 0.7
      ? "bg-emerald-500"
      : value >= 0.4
        ? "bg-amber-500"
        : "bg-slate-500";
  return (
    <div className="flex items-center gap-2">
      <div className="h-2 w-16 overflow-hidden rounded bg-gray-200 dark:bg-gray-700">
        <div className={`h-full ${color}`} style={{ width: `${pct}%` }} />
      </div>
      <span className="font-mono text-xs">{value.toFixed(2)}</span>
    </div>
  );
};

export const SignalsPage: React.FC = () => {
  const [runs, setRuns] = useState<TradingRunInfoView[]>([]);
  const [selectedRunId, setSelectedRunId] = useState<string>("");
  const [take, setTake] = useState<number>(100);

  const [signals, setSignals] = useState<SignalView[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /* ─── Load runs ─── */
  useEffect(() => {
    (async () => {
      try {
        const list = await tradingRunApi.GetList();
        setRuns(list);
      } catch {
        // Ignore
      }
    })();
  }, []);

  /* ─── Load signals on run change ─── */
  useEffect(() => {
    if (!selectedRunId) {
      setSignals([]);
      return;
    }
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await signalApi.GetRecentByRun(selectedRunId, take);
        setSignals(data);
      } catch (e) {
        setError(
          e instanceof Error ? e.message : "خطا در دریافت سیگنال‌ها",
        );
        setSignals([]);
      } finally {
        setLoading(false);
      }
    })();
  }, [selectedRunId, take]);

  const summary = useMemo(() => {
    const buys = signals.filter((s) => s.action === SignalAction.Buy).length;
    const sells = signals.filter((s) => s.action === SignalAction.Sell).length;
    const consumed = signals.filter((s) => s.consumedAt != null).length;
    return { buys, sells, consumed };
  }, [signals]);

  return (
    <div className="space-y-4 p-6">
      {/* Header */}
      <div>
        <h1 className="text-xl font-bold text-gray-900 dark:text-white">
          سیگنال‌های تولیدشده
        </h1>
        <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
          {signals.length} سیگنال · {summary.buys} خرید · {summary.sells} فروش ·{" "}
          {summary.consumed} مصرف‌شده
        </p>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-2">
        <select
          value={selectedRunId}
          onChange={(e) => setSelectedRunId(e.target.value)}
          className="min-w-[240px] rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        >
          <option value="">انتخاب پلن...</option>
          {runs.map((r) => (
            <option key={r.id} value={r.id}>
              {r.name}
            </option>
          ))}
        </select>
        <select
          value={take}
          onChange={(e) => setTake(Number(e.target.value))}
          className="rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        >
          <option value={50}>۵۰ مورد آخر</option>
          <option value={100}>۱۰۰ مورد آخر</option>
          <option value={200}>۲۰۰ مورد آخر</option>
          <option value={500}>۵۰۰ مورد آخر</option>
        </select>
      </div>

      {/* Error */}
      {error && (
        <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
          ⚠️ {error}
        </div>
      )}

      {/* Table */}
      <div className="overflow-x-auto rounded-lg border border-gray-200 shadow-sm dark:border-gray-700">
        <table className="w-full text-right text-sm text-gray-700 dark:text-gray-300">
          <thead className="bg-gray-50 text-xs text-gray-700 dark:bg-gray-700 dark:text-gray-300">
            <tr>
              <th className="p-3">نماد</th>
              <th className="p-3">اکشن</th>
              <th className="p-3">قدرت</th>
              <th className="p-3">استراتژی</th>
              <th className="p-3">زمان تولید</th>
              <th className="p-3">وضعیت مصرف</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {!selectedRunId ? (
              <tr>
                <td colSpan={6} className="p-6 text-center text-sm text-gray-500">
                  برای دیدن سیگنال‌ها، یک پلن انتخاب کنید.
                </td>
              </tr>
            ) : loading ? (
              <tr>
                <td colSpan={6} className="p-6 text-center">
                  در حال بارگذاری...
                </td>
              </tr>
            ) : signals.length === 0 ? (
              <tr>
                <td colSpan={6} className="p-6 text-center text-sm text-gray-500">
                  سیگنالی یافت نشد.
                </td>
              </tr>
            ) : (
              signals.map((s) => (
                <tr
                  key={s.id}
                  className="hover:bg-gray-50 dark:hover:bg-gray-800/50"
                >
                  <td className="p-3 font-mono text-xs">{s.isin}</td>
                  <td className="p-3">
                    <ActionBadge action={s.action} />
                  </td>
                  <td className="p-3">
                    <StrengthBar value={s.strength} />
                  </td>
                  <td className="p-3 font-mono text-xs">{s.strategyKey}</td>
                  <td className="p-3 text-xs">
                    {new Date(s.producedAt).toLocaleString("fa-IR")}
                  </td>
                  <td className="p-3 text-xs">
                    {s.consumedAt ? (
                      <span className="text-emerald-600 dark:text-emerald-400">
                        ✓ مصرف‌شده
                      </span>
                    ) : (
                      <span className="text-amber-600 dark:text-amber-400">
                        ⏳ در انتظار
                      </span>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};