import React, { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { backtestApi } from "../../api/backtestApi";
import type {
  BacktestRunSummaryView,
  BacktestDailyEquityView,
} from "../../models";
import { EquityCurveChart } from "./components/EquityCurveChart";
import { BacktestStatusBadge } from "./components/BacktestStatusBadge";

const COLORS = ["#10b981", "#3b82f6", "#f59e0b", "#ef4444", "#8b5cf6"];

const fmtMoney = (v: number): string =>
  v.toLocaleString("fa-IR", { maximumFractionDigits: 0 });

const fmtPct = (v: number): string => `${v.toFixed(2)}%`;

/* ─── Metric Row Helper ─── */
interface MetricRowProps {
  label: string;
  values: (string | number)[];
  bestIndex?: number | null;
  highIsBetter?: boolean;
  compareNumeric?: number[];
  format?: (v: number) => string;
}

const MetricRow: React.FC<MetricRowProps> = ({
  label,
  values,
  bestIndex,
  format,
}) => (
  <tr className="border-b border-gray-200 dark:border-gray-700">
    <td className="p-2 font-medium text-gray-600 dark:text-gray-400">
      {label}
    </td>
    {values.map((v, idx) => (
      <td
        key={idx}
        className={`p-2 text-center font-mono ${
          bestIndex === idx
            ? "bg-emerald-50 text-emerald-700 font-bold dark:bg-emerald-900/20 dark:text-emerald-300"
            : ""
        }`}
      >
        {typeof v === "number" && format ? format(v) : v}
      </td>
    ))}
  </tr>
);

const findBestIndex = (
  values: number[],
  highIsBetter: boolean,
): number | null => {
  if (values.length === 0) return null;
  let best = 0;
  for (let i = 1; i < values.length; i++) {
    if (highIsBetter ? values[i] > values[best] : values[i] < values[best]) {
      best = i;
    }
  }
  return best;
};

export const BacktestComparePage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const idsParam = searchParams.get("ids") ?? "";
  const ids = useMemo(
    () => idsParam.split(",").filter(Boolean),
    [idsParam],
  );

  const [runs, setRuns] = useState<BacktestRunSummaryView[]>([]);
  const [equities, setEquities] = useState<
    Record<string, BacktestDailyEquityView[]>
  >({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (ids.length === 0) {
      setError("هیچ بکتستی انتخاب نشده. به لیست برگردید و چند بکتست انتخاب کنید.");
      setLoading(false);
      return;
    }

    (async () => {
      setLoading(true);
      setError(null);
      try {
        const summaries = await backtestApi.Compare(ids);
        setRuns(summaries);

        const equityResults = await Promise.all(
          ids.map((id) => backtestApi.GetEquityCurve(id)),
        );

        const equityMap: Record<string, BacktestDailyEquityView[]> = {};
        ids.forEach((id, idx) => {
          equityMap[id] = equityResults[idx];
        });
        setEquities(equityMap);
      } catch (e) {
        setError(e instanceof Error ? e.message : "خطا در دریافت داده");
      } finally {
        setLoading(false);
      }
    })();
  }, [idsParam]);

  /* ─── Best-index helpers ─── */
  const bestReturnIdx = useMemo(
    () => findBestIndex(runs.map((r) => r.totalReturnPct), true),
    [runs],
  );
  const bestDrawdownIdx = useMemo(
    () => findBestIndex(runs.map((r) => r.maxDrawdownPct), false),
    [runs],
  );
  const bestSharpeIdx = useMemo(
    () => findBestIndex(runs.map((r) => r.sharpeRatio), true),
    [runs],
  );
  const bestWinRateIdx = useMemo(
    () => findBestIndex(runs.map((r) => r.winRate), true),
    [runs],
  );
  const bestTradesIdx = useMemo(
    () => findBestIndex(runs.map((r) => r.totalTrades), true),
    [runs],
  );

  /* ─── Equity Series ─── */
  const equitySeries = runs.map((r, idx) => ({
    name: r.name,
    data: equities[r.id] ?? [],
    color: COLORS[idx % COLORS.length],
  }));

  if (loading) {
    return (
      <div className="p-6 text-center text-sm text-gray-500">
        در حال بارگذاری...
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-3 p-6">
        <div className="rounded-lg bg-red-50 p-4 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
          ⚠️ {error}
        </div>
        <Link
          to="/trader/backtests"
          className="inline-block text-blue-600 hover:underline"
        >
          ← بازگشت به لیست
        </Link>
      </div>
    );
  }

  if (runs.length === 0) {
    return (
      <div className="space-y-3 p-6">
        <div className="rounded-lg bg-amber-50 p-4 text-sm text-amber-700 dark:bg-amber-900/30 dark:text-amber-400">
          بکتستی برای نمایش پیدا نشد.
        </div>
        <Link
          to="/trader/backtests"
          className="inline-block text-blue-600 hover:underline"
        >
          ← بازگشت به لیست
        </Link>
      </div>
    );
  }

  return (
    <div className="space-y-4 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <Link
            to="/trader/backtests"
            className="text-xs text-blue-600 hover:underline"
          >
            ← بکتست‌ها
          </Link>
          <h1 className="mt-1 text-xl font-bold text-gray-900 dark:text-white">
            مقایسه‌ی {runs.length} بکتست
          </h1>
        </div>
      </div>

      {/* Comparison Table */}
      <div className="overflow-x-auto rounded-lg border border-gray-200 bg-white dark:border-gray-700 dark:bg-gray-800">
        <table className="w-full text-sm text-gray-700 dark:text-gray-300">
          <thead className="border-b border-gray-200 bg-gray-50 text-xs dark:border-gray-700 dark:bg-gray-700">
            <tr>
              <th className="p-2 text-right">معیار</th>
              {runs.map((r, idx) => (
                <th key={r.id} className="p-2 text-center">
                  <div className="flex flex-col items-center gap-1">
                    <span
                      className="inline-block h-2 w-2 rounded-full"
                      style={{ backgroundColor: COLORS[idx % COLORS.length] }}
                    />
                    <Link
                      to={`/trader/backtests/${r.id}`}
                      className="font-bold text-blue-600 hover:underline dark:text-blue-400"
                    >
                      {r.name}
                    </Link>
                    <BacktestStatusBadge status={r.status} />
                  </div>
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            <MetricRow
              label="استراتژی"
              values={runs.map((r) => r.strategyKey)}
            />
            <MetricRow
              label="بازده کل"
              values={runs.map((r) => r.totalReturnPct)}
              bestIndex={bestReturnIdx}
              format={fmtPct}
            />
            <MetricRow
              label="حداکثر افت"
              values={runs.map((r) => r.maxDrawdownPct)}
              bestIndex={bestDrawdownIdx}
              format={(v) => fmtPct(-Math.abs(v))}
            />
            <MetricRow
              label="شارپ"
              values={runs.map((r) => r.sharpeRatio)}
              bestIndex={bestSharpeIdx}
              format={(v) => v.toFixed(2)}
            />
            <MetricRow
              label="نرخ برد"
              values={runs.map((r) => r.winRate)}
              bestIndex={bestWinRateIdx}
              format={fmtPct}
            />
            <MetricRow
              label="تعداد ترید"
              values={runs.map((r) => r.totalTrades)}
              bestIndex={bestTradesIdx}
              format={(v) => String(v)}
            />
            <MetricRow
              label="برد / باخت"
              values={runs.map(
                (r) => `${r.winningTrades} / ${r.losingTrades}`,
              )}
            />
            <MetricRow
              label="سرمایه اولیه"
              values={runs.map((r) => r.initialCapital)}
              format={fmtMoney}
            />
            <MetricRow
              label="سرمایه نهایی"
              values={runs.map((r) => r.finalEquity)}
              format={fmtMoney}
            />
            <MetricRow
              label="سود خالص"
              values={runs.map((r) => r.finalEquity - r.initialCapital)}
              format={fmtMoney}
            />
          </tbody>
        </table>
      </div>

      <div className="rounded-lg border border-blue-200 bg-blue-50 p-3 text-xs text-blue-700 dark:border-blue-800 dark:bg-blue-900/20 dark:text-blue-300">
        🏆 ستون سبز = بهترین مقدار در هر ردیف. کلیک روی نام بکتست برای مشاهده‌ی
        جزئیات.
      </div>

      {/* Equity Curve Comparison */}
      <EquityCurveChart series={equitySeries} />
    </div>
  );
};