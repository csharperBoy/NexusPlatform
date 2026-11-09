import React from "react";
import { useRiskDashboard } from "../../hooks/useRiskDashboard";
import { KillSwitchCard } from "./components/KillSwitchCard";
import { CircuitBreakerCard } from "./components/CircuitBreakerCard";

export const RiskDashboardPage: React.FC = () => {
  const {
    killSwitch,
    activeCircuits,
    accounts,
    runs,
    loading,
    busyKey,
    error,
    lastRefreshAt,
    refresh,
    activateGlobalKillSwitch,
    deactivateGlobalKillSwitch,
    activateAccountKillSwitch,
    deactivateAccountKillSwitch,
    resetCircuit,
    resetAllCircuits,
  } = useRiskDashboard();

  const lastRefreshText = lastRefreshAt
    ? lastRefreshAt.toLocaleTimeString("fa-IR")
    : "—";

  return (
    <div className="space-y-4 p-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-900 dark:text-white">
            مدیریت ریسک
          </h1>
          <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
            وضعیت لحظه‌ای Risk Guards + اقدامات اضطراری
            {lastRefreshAt && (
              <span className="mr-2">· آخرین بروزرسانی: {lastRefreshText}</span>
            )}
          </p>
        </div>
        <button
          type="button"
          onClick={refresh}
          disabled={loading}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 disabled:opacity-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
        >
          🔄 بروزرسانی
        </button>
      </div>

      {/* Error */}
      {error && (
        <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
          ⚠️ {error}
        </div>
      )}

      {/* Loading (only on first load) */}
      {loading && !killSwitch && (
        <div className="rounded-lg border border-gray-200 p-6 text-center text-sm text-gray-500 dark:border-gray-700">
          در حال بارگذاری...
        </div>
      )}

      {/* Cards */}
      {!loading || killSwitch ? (
        <>
          <KillSwitchCard
            status={killSwitch}
            accounts={accounts}
            busyKey={busyKey}
            onActivateGlobal={activateGlobalKillSwitch}
            onDeactivateGlobal={deactivateGlobalKillSwitch}
            onActivateAccount={activateAccountKillSwitch}
            onDeactivateAccount={deactivateAccountKillSwitch}
          />

          <CircuitBreakerCard
            activeCircuits={activeCircuits}
            accounts={accounts}
            runs={runs}
            busyKey={busyKey}
            onReset={resetCircuit}
            onResetAll={resetAllCircuits}
          />
        </>
      ) : null}

      {/* Footnote */}
      <div className="rounded-lg border border-slate-200 bg-slate-50 p-3 text-[11px] text-slate-600 dark:border-slate-700 dark:bg-slate-800/40 dark:text-slate-400">
        ℹ️ این صفحه فقط <strong>وضعیت Runtime</strong> را نشان می‌دهد.
        تنظیمات Risk (MaxLossPercent، ConsecutiveLosses، RateLimits، …) در
        <code className="mx-1 rounded bg-slate-200 px-1 font-mono text-[10px] dark:bg-slate-700">
          appsettings.json → Trader:Risk
        </code>
        است و نیاز به Restart دارد. برای Per-Run Emergency Stop، از صفحه‌ی
        «پلن‌های معاملاتی» → دکمه‌ی «توقف» استفاده کن.
      </div>
    </div>
  );
};