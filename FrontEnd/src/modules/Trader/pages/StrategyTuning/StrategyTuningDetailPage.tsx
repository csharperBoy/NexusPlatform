// src/modules/Trader/pages/StrategyTuning/StrategyTuningDetailPage.tsx
import React, { useState } from "react";
import { Link, useParams } from "react-router-dom";
import { useStrategyTuningDetail } from "../../hooks/useStrategyTuning";
import { TUNING_STATUS_LABELS, TUNING_SCALE_LABELS } from "../../models";
import type { StrategyTuningResultView } from "../../models";

const fmtPct = (v: number | null) => v == null ? "—" : `${v.toFixed(2)}%`;
const colorPct = (v: number | null) =>
  v == null ? "" : v > 0 ? "text-emerald-600" : v < 0 ? "text-red-600" : "";

const ChampionCard: React.FC<{ label: string; result: StrategyTuningResultView | null }> = ({ label, result }) => (
  <div className="rounded-lg border border-gray-200 bg-white p-4 dark:border-gray-700 dark:bg-gray-800">
    <p className="text-xs text-gray-500">🏆 {label}</p>
    {result ? (
      <>
        <p className={`mt-1 text-2xl font-bold ${colorPct(result.avgReturnCompoundYearly)}`}>
          {fmtPct(result.avgReturnCompoundYearly)} - ({fmtPct(result.avgReturnCompoundValidYearly)})
        </p>        
        <p className="mt-1 text-[10px] text-gray-500">
          Trades: {result.totalTradesYearly ?? "—"} · Valid: {result.validWindowsYearly ?? "—"}
        </p>
        <details className="mt-2">
          <summary className="cursor-pointer text-[10px] text-blue-600">Config</summary>
          <pre dir="ltr" className="mt-1 max-h-48 overflow-auto rounded bg-gray-900 p-2 text-[10px] text-gray-100">
            {JSON.stringify(JSON.parse(result.configJson), null, 2)}
          </pre>
        </details>
      </>
    ) : <p className="mt-2 text-xs text-gray-400">—</p>}
  </div>
);

export const StrategyTuningDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { run, loading, error, refresh } = useStrategyTuningDetail(id);

  if (loading && !run) return <div className="p-6 text-sm text-gray-500">بارگذاری...</div>;
  if (error || !run) return <div className="p-6 text-red-600">⚠️ {error ?? "پیدا نشد"}</div>;

  const champYearly = run.topResults.find(r => r.isTop10InYearly) ?? null;
  const champSemi = run.topResults.find(r => r.isTop10InSemi) ?? null;
  const champQuarterly = run.topResults.find(r => r.isTop10InQuarterly) ?? null;
  const champMonthly = run.topResults.find(r => r.isTop10InMonthly) ?? null;
  const statusLabel = TUNING_STATUS_LABELS[run.status as keyof typeof TUNING_STATUS_LABELS] ?? String(run.status);

  return (
    <div className="space-y-4 p-6">
      <div className="flex justify-between">
        <div>
          <Link to="/trader/tuning" className="text-xs text-blue-600">← بهینه‌سازی‌ها</Link>
          <h1 className="mt-1 text-xl font-bold dark:text-white">{run.name}</h1>
          <p className="mt-1 text-xs text-gray-500">
            {run.strategyKey} · {run.isin} · {run.dateFrom.slice(0,10)} → {run.dateTo.slice(0,10)}
          </p>
        </div>
        <div className="text-right">
          <p className="text-sm font-medium">{statusLabel}</p>
          <p className="mt-1 text-xs text-gray-500">
            {run.completedEvaluations}/{run.totalEvaluations}
          </p>
          <button onClick={refresh} className="mt-2 text-xs text-blue-600">🔄</button>
        </div>
      </div>
{run.selectedParameters && run.selectedParameters.length > 0 && (
  <div className="rounded border border-gray-200 p-2 text-xs dark:border-gray-700">
    <span className="text-gray-500">پارامترهای تنظیم‌شده: </span>
    <span className="font-mono">{run.selectedParameters.join(", ")}</span>
  </div>
)}
      {run.errorMessage && (
        <div className="rounded bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30">⚠️ {run.errorMessage}</div>
      )}

      <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
        <ChampionCard label="سالانه" result={champYearly} />
        <ChampionCard label="۶ ماهه" result={champSemi} />
        <ChampionCard label="۳ ماهه" result={champQuarterly} />
        <ChampionCard label="۱ ماهه" result={champMonthly} />
      </div>

      <div className="rounded-lg border border-gray-200 dark:border-gray-700">
        <div className="border-b border-gray-200 p-3 dark:border-gray-700">
          <h3 className="text-sm font-bold">Top 10 هر مقیاس</h3>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-right text-xs">
            <thead className="bg-gray-50 dark:bg-gray-700">
              <tr>
                <th className="p-2">#</th>
                <th className="p-2">سالانه</th>
                <th className="p-2">۶ ماهه</th>
                <th className="p-2">۳ ماهه</th>
                <th className="p-2">۱ ماهه</th>
                <th className="p-2">Trades(Y)</th>
                <th className="p-2">Badges</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
              {run.topResults.map((r, i) => (
                <tr key={r.id} className="hover:bg-gray-50 dark:hover:bg-gray-700/30">
                  <td className="p-2">{i + 1}</td>
                  <td className={`p-2 font-mono ${colorPct(r.avgReturnCompoundYearly)}`}>
                    {fmtPct(r.avgReturnCompoundYearly)} - ({fmtPct(r.avgReturnCompoundValidYearly)})
                  </td>
                  <td className={`p-2 font-mono ${colorPct(r.avgReturnCompoundSemi)}`}>
                    {fmtPct(r.avgReturnCompoundSemi)}
                  </td>
                  <td className={`p-2 font-mono ${colorPct(r.avgReturnCompoundQuarterly)}`}>
                    {fmtPct(r.avgReturnCompoundQuarterly)}
                  </td>
                  <td className={`p-2 font-mono ${colorPct(r.avgReturnCompoundMonthly)}`}>
                    {fmtPct(r.avgReturnCompoundMonthly)}
                  </td>
                  <td className="p-2">{r.totalTradesYearly ?? "—"}</td>
                  <td className="p-2 space-x-1">
                    {r.isTop10InYearly && <span className="rounded bg-blue-100 px-1 text-[10px] dark:bg-blue-900/40">Y</span>}
                    {r.isTop10InSemi && <span className="rounded bg-purple-100 px-1 text-[10px] dark:bg-purple-900/40">6M</span>}
                    {r.isTop10InQuarterly && <span className="rounded bg-amber-100 px-1 text-[10px] dark:bg-amber-900/40">3M</span>}
                    {r.isTop10InMonthly && <span className="rounded bg-emerald-100 px-1 text-[10px] dark:bg-emerald-900/40">1M</span>}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};