import React, { useState } from "react";
import type { BacktestTradeView } from "../../../models";
import { EXIT_REASON_LABELS, BacktestExitReason } from "../../../models";

interface Props {
  trades: BacktestTradeView[];
}

const fmtMoney = (v: number): string =>
  v.toLocaleString("fa-IR", { maximumFractionDigits: 0 });

const parseUtcDate = (s: string): Date => {
  const hasTz = /[Zz]|[+-]\d{2}:?\d{2}$/.test(s);
  return new Date(hasTz ? s : s + "Z");
};

const fmtDate = (s: string): string =>
  parseUtcDate(s).toLocaleString("fa-IR", {
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });

const fmtDuration = (sec: number): string => {
  if (sec < 3600) return `${Math.round(sec / 60)}m`;
  if (sec < 86400) return `${(sec / 3600).toFixed(1)}h`;
  return `${(sec / 86400).toFixed(1)}d`;
};

const ExitBadge: React.FC<{ reason: number }> = ({ reason }) => {
  const r = reason as BacktestExitReason;
  const color =
    r === BacktestExitReason.TakeProfit
      ? "bg-emerald-900/40 text-emerald-300"
      : r === BacktestExitReason.StopLoss
        ? "bg-red-900/40 text-red-300"
        : r === BacktestExitReason.Signal
          ? "bg-blue-900/40 text-blue-300"
          : "bg-slate-700 text-slate-300";

  return (
    <span
      className={`inline-flex rounded-full px-2 py-0.5 text-[10px] font-medium whitespace-nowrap ${color}`}
    >
      {EXIT_REASON_LABELS[r] ?? "—"}
    </span>
  );
};

export const TradesTable: React.FC<Props> = ({ trades }) => {
  const [sortBy, setSortBy] = useState<"entry" | "netPnl">("entry");

  const sorted = [...trades].sort((a, b) => {
    if (sortBy === "netPnl") return b.netPnL - a.netPnL;
    return a.entryTime.localeCompare(b.entryTime);
  });

  if (trades.length === 0) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-8 text-center text-sm text-gray-500 dark:border-gray-700 dark:bg-gray-800">
        هیچ تریدی در این بکتست ثبت نشده.
      </div>
    );
  }

  return (
    <div className="rounded-lg border border-gray-200 bg-white dark:border-gray-700 dark:bg-gray-800">
      <div className="flex items-center justify-between border-b border-gray-200 p-3 dark:border-gray-700">
        <h3 className="text-sm font-bold text-gray-900 dark:text-white">
          تریدها ({trades.length})
        </h3>
        <div className="flex gap-1">
          <button
            type="button"
            onClick={() => setSortBy("entry")}
            className={`rounded px-2 py-1 text-xs ${
              sortBy === "entry"
                ? "bg-blue-600 text-white"
                : "text-gray-600 dark:text-gray-400"
            }`}
          >
            زمانی
          </button>
          <button
            type="button"
            onClick={() => setSortBy("netPnl")}
            className={`rounded px-2 py-1 text-xs ${
              sortBy === "netPnl"
                ? "bg-blue-600 text-white"
                : "text-gray-600 dark:text-gray-400"
            }`}
          >
            سود
          </button>
        </div>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full text-right text-xs">
          <thead className="bg-gray-50 text-gray-700 dark:bg-gray-700 dark:text-gray-300">
            <tr>
              <th className="p-2">نماد</th>
              <th className="p-2">ورود</th>
              <th className="p-2">خروج</th>
              <th className="p-2 text-center">تعداد</th>
              <th className="p-2">میانگین ورود</th>
              <th className="p-2">قیمت خروج</th>
              <th className="p-2">دلیل خروج</th>
              <th className="p-2">سود خالص</th>
              <th className="p-2 text-center">%</th>
              <th className="p-2 text-center">مدت</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {sorted.map((t) => (
              <tr
                key={t.id}
                className="hover:bg-gray-50 dark:hover:bg-gray-700/30"
              >
                <td className="p-2 font-mono">{t.isin}</td>
                <td className="p-2">{fmtDate(t.entryTime)}</td>
                <td className="p-2">{fmtDate(t.exitTime)}</td>
                <td className="p-2 text-center">{fmtMoney(t.totalEntryQuantity)}</td>
                <td className="p-2">{fmtMoney(t.weightedAvgEntryPrice)}</td>
                <td className="p-2">{fmtMoney(t.exitPrice)}</td>
                <td className="p-2">
                  <ExitBadge reason={t.exitReason} />
                </td>
                <td
                  className={`p-2 font-mono ${
                    t.netPnL > 0 ? "text-emerald-600" : "text-red-600"
                  }`}
                >
                  {fmtMoney(t.netPnL)}
                </td>
                <td
                  className={`p-2 text-center ${
                    t.netPnLPct > 0 ? "text-emerald-600" : "text-red-600"
                  }`}
                >
                  {t.netPnLPct.toFixed(2)}%
                </td>
                <td className="p-2 text-center text-gray-500">
                  {fmtDuration(t.holdingDurationSeconds)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};