import React from "react";
import type { BacktestRunDetailView } from "../../../models";

interface Props {
  run: BacktestRunDetailView;
}

const MetricCard: React.FC<{
  label: string;
  value: string;
  color?: string;
  sub?: string;
}> = ({ label, value, color, sub }) => (
  <div className="rounded-lg border border-gray-200 bg-white p-4 dark:border-gray-700 dark:bg-gray-800">
    <p className="text-xs text-gray-500 dark:text-gray-400">{label}</p>
    <p
      className={`mt-1 text-xl font-bold ${
        color ?? "text-gray-900 dark:text-white"
      }`}
    >
      {value}
    </p>
    {sub && <p className="mt-0.5 text-[10px] text-gray-400">{sub}</p>}
  </div>
);

const fmtMoney = (v: number): string =>
  v.toLocaleString("fa-IR", { maximumFractionDigits: 0 });

const fmtPct = (v: number): string => `${v.toFixed(2)}%`;

const returnColor = (v: number): string =>
  v > 0
    ? "text-emerald-600 dark:text-emerald-400"
    : v < 0
      ? "text-red-600 dark:text-red-400"
      : "text-gray-900 dark:text-white";

export const MetricsCards: React.FC<Props> = ({ run }) => {
  const totalPnl = run.finalEquity - run.initialCapital;

  return (
    <div className="grid grid-cols-2 gap-3 md:grid-cols-3 lg:grid-cols-6">
      <MetricCard
        label="بازده کل"
        value={fmtPct(run.totalReturnPct)}
        color={returnColor(run.totalReturnPct)}
        sub={`${fmtMoney(totalPnl)} ریال`}
      />
      <MetricCard
        label="حداکثر افت"
        value={fmtPct(run.maxDrawdownPct)}
        color="text-red-600 dark:text-red-400"
      />
      <MetricCard
        label="شارپ"
        value={run.sharpeRatio.toFixed(2)}
        color={
          run.sharpeRatio >= 1
            ? "text-emerald-600"
            : run.sharpeRatio >= 0
              ? "text-gray-900 dark:text-white"
              : "text-red-600"
        }
      />
      <MetricCard
        label="نرخ برد"
        value={fmtPct(run.winRate)}
        sub={`${run.winningTrades} از ${run.totalTrades}`}
      />
      <MetricCard
        label="Profit Factor"
        value={run.profitFactor.toFixed(2)}
        color={
          run.profitFactor >= 1.5
            ? "text-emerald-600"
            : run.profitFactor >= 1
              ? "text-amber-500"
              : "text-red-600"
        }
      />
      <MetricCard
        label="سرمایه‌ی نهایی"
        value={fmtMoney(run.finalEquity)}
        sub={`از ${fmtMoney(run.initialCapital)}`}
      />
    </div>
  );
};