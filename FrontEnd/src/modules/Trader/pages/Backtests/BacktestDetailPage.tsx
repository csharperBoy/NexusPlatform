import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { backtestApi } from "../../api/backtestApi";
import type {
  BacktestRunDetailView,
  BacktestTradeView,
  BacktestDailyEquityView,
} from "../../models";
import { BacktestStatus, BACKTEST_STATUS_LABELS } from "../../models";
import { BacktestStatusBadge } from "./components/BacktestStatusBadge";
import { MetricsCards } from "./components/MetricsCards";
import { EquityCurveChart } from "./components/EquityCurveChart";
import { TradesTable } from "./components/TradesTable";
import { BacktestLogsTab } from "./components/BacktestLogsTab";
export const BacktestDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [run, setRun] = useState<BacktestRunDetailView | null>(null);
  const [trades, setTrades] = useState<BacktestTradeView[]>([]);
  const [equity, setEquity] = useState<BacktestDailyEquityView[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
const [activeTab, setActiveTab] = useState<"summary" | "logs">("summary");
  const load = async () => {
    if (!id) return;
    setLoading(true);
    setError(null);
    try {
      const detail = await backtestApi.GetById(id);
      setRun(detail);

      if (detail.status === BacktestStatus.Completed) {
        const [t, e] = await Promise.all([
          backtestApi.GetTrades(id),
          backtestApi.GetEquityCurve(id),
        ]);
        setTrades(t);
        setEquity(e);
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در دریافت");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void load();
  }, [id]);

  // Poll if running
  useEffect(() => {
    if (!run) return;
    if (
      run.status !== BacktestStatus.Pending &&
      run.status !== BacktestStatus.Running
    )
      return;

    const t = setInterval(() => {
      void load();
    }, 5000);

    return () => clearInterval(t);
  }, [run?.status, id]);

  if (loading && !run) {
    return (
      <div className="p-6 text-center text-sm text-gray-500">
        در حال بارگذاری...
      </div>
    );
  }

  if (error || !run) {
    return (
      <div className="p-6">
        <div className="rounded-lg bg-red-50 p-4 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
          ⚠️ {error ?? "بکتست پیدا نشد"}
        </div>
        <Link
          to="/trader/backtests"
          className="mt-4 inline-block text-blue-600 hover:underline"
        >
          ← بازگشت
        </Link>
      </div>
    );
  }

  return (
    <div className="space-y-4 p-6">
      <div className="flex items-center justify-between">
        <div>
          <Link
            to="/trader/backtests"
            className="text-xs text-blue-600 hover:underline"
          >
            ← بکتست‌ها
          </Link>
          <h1 className="mt-1 text-xl font-bold text-gray-900 dark:text-white">
            {run.name}
          </h1>
          <p className="mt-1 text-xs text-gray-500">
            {run.strategyKey} · {run.dateFrom.slice(0, 10)} →{" "}
            {run.dateTo.slice(0, 10)} ·{" "}
            {run.symbols.map((s) => s.isin).join(", ")}
          </p>
        </div>
        <div className="text-right">
          <BacktestStatusBadge status={run.status} />
          {run.durationMs && (
            <p className="mt-1 text-[10px] text-gray-500">
              {(run.durationMs / 1000).toFixed(1)}s
            </p>
          )}
        </div>
      </div>

      {run.errorMessage && (
        <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
          ⚠️ {run.errorMessage}
        </div>
      )}
{/* Tab Bar */}
<div className="flex gap-1 border-b border-gray-200 dark:border-gray-700">
  <button
    type="button"
    onClick={() => setActiveTab("summary")}
    className={`border-b-2 px-4 py-2 text-sm font-medium transition-colors ${
      activeTab === "summary"
        ? "border-blue-500 text-blue-600 dark:text-blue-400"
        : "border-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400"
    }`}
  >
    خلاصه
  </button>
  <button
    type="button"
    onClick={() => setActiveTab("logs")}
    className={`border-b-2 px-4 py-2 text-sm font-medium transition-colors ${
      activeTab === "logs"
        ? "border-blue-500 text-blue-600 dark:text-blue-400"
        : "border-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400"
    }`}
  >
    لاگ‌ها
  </button>
</div>
{activeTab === "summary" && (
  <>
    {run.status === BacktestStatus.Completed && (
      <>
        <MetricsCards run={run} />
        <EquityCurveChart
          series={[
            {
              name: run.name,
              data: equity,
              color: "#10b981",
            },
          ]}
        />
        <TradesTable trades={trades} />
      </>
    )}

    {run.status === BacktestStatus.Pending && (
      <div className="rounded-lg border border-blue-700 bg-blue-900/20 p-4 text-sm text-blue-300">
        ⏳ در انتظار اجرا...
      </div>
    )}

    {run.status === BacktestStatus.Running && (
      <div className="rounded-lg border border-amber-700 bg-amber-900/20 p-4 text-sm text-amber-300">
        ⚡ در حال اجرا...
      </div>
    )}
  </>
)}

{activeTab === "logs" && <BacktestLogsTab backtestRunId={run.id} />}
      

      

    </div>
  );
};