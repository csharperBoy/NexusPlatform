import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useBacktestManagement } from "../../hooks/useBacktestManagement";
import { BacktestStatusBadge } from "./components/BacktestStatusBadge";
import { BacktestCreateModal } from "./BacktestCreateModal";
import { BacktestStatus } from "../../models";

const fmtMoney = (v: number): string =>
  v.toLocaleString("fa-IR", { maximumFractionDigits: 0 });

const fmtPct = (v: number): string => `${v.toFixed(2)}%`;

export const BacktestsListPage: React.FC = () => {
  const { runs, loading, error, refresh, handleCancel } =
    useBacktestManagement();
  const [createOpen, setCreateOpen] = useState(false);

  // Poll running backtests
  useEffect(() => {
    const hasRunning = runs.some(
      (r) =>
        r.status === BacktestStatus.Pending ||
        r.status === BacktestStatus.Running,
    );
    if (!hasRunning) return;

    const t = setInterval(() => {
      void refresh();
    }, 5000);

    return () => clearInterval(t);
  }, [runs, refresh]);

  return (
    <div className="space-y-4 p-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-900 dark:text-white">
            بکتست‌ها
          </h1>
          <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
            {runs.length} بکتست
          </p>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            onClick={refresh}
            disabled={loading}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 disabled:opacity-50 dark:border-gray-600 dark:text-gray-300"
          >
            🔄 بروزرسانی
          </button>
          <button
            type="button"
            onClick={() => setCreateOpen(true)}
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
          >
            ➕ بکتست جدید
          </button>
        </div>
      </div>

      {error && (
        <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
          ⚠️ {error}
        </div>
      )}

      <div className="overflow-x-auto rounded-lg border border-gray-200 shadow-sm dark:border-gray-700">
        <table className="w-full text-right text-sm text-gray-700 dark:text-gray-300">
          <thead className="bg-gray-50 text-xs text-gray-700 dark:bg-gray-700 dark:text-gray-300">
            <tr>
              <th className="p-3">نام</th>
              <th className="p-3">وضعیت</th>
              <th className="p-3">استراتژی</th>
              <th className="p-3">بازه</th>
              <th className="p-3">بازده</th>
              <th className="p-3">حداکثر افت</th>
              <th className="p-3">شارپ</th>
              <th className="p-3">ترید</th>
              <th className="p-3 text-center">عملیات</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {loading && runs.length === 0 ? (
              <tr>
                <td colSpan={9} className="p-6 text-center">
                  در حال بارگذاری...
                </td>
              </tr>
            ) : runs.length === 0 ? (
              <tr>
                <td colSpan={9} className="p-6 text-center text-gray-500">
                  هیچ بکتستی وجود ندارد.
                </td>
              </tr>
            ) : (
              runs.map((r) => (
                <tr
                  key={r.id}
                  className="hover:bg-gray-50 dark:hover:bg-gray-700/30"
                >
                  <td className="p-3 font-medium">{r.name}</td>
                  <td className="p-3">
                    <BacktestStatusBadge status={r.status} />
                  </td>
                  <td className="p-3 font-mono text-xs">{r.strategyKey}</td>
                  <td className="p-3 text-xs">
                    {r.dateFrom.slice(0, 10)} → {r.dateTo.slice(0, 10)}
                  </td>
                  <td
                    className={`p-3 text-xs font-mono ${
                      r.totalReturnPct > 0
                        ? "text-emerald-600"
                        : r.totalReturnPct < 0
                          ? "text-red-600"
                          : ""
                    }`}
                  >
                    {fmtPct(r.totalReturnPct)}
                  </td>
                  <td className="p-3 text-xs font-mono text-red-600">
                    {fmtPct(-Math.abs(r.maxDrawdownPct))}
                  </td>
                  <td className="p-3 text-xs font-mono">
                    {r.sharpeRatio.toFixed(2)}
                  </td>
                  <td className="p-3 text-xs">
                    {r.totalTrades} ({r.winningTrades}W/{r.losingTrades}L)
                  </td>
                  <td className="p-3">
                    <div className="flex justify-end gap-1">
                      <Link
                        to={`/trader/backtests/${r.id}`}
                        className="rounded bg-blue-600 px-2 py-1 text-xs text-white hover:bg-blue-700"
                      >
                        جزئیات
                      </Link>
                      {(r.status === BacktestStatus.Pending ||
                        r.status === BacktestStatus.Running) && (
                        <button
                          type="button"
                          onClick={() => handleCancel(r.id)}
                          className="rounded bg-red-600 px-2 py-1 text-xs text-white hover:bg-red-700"
                        >
                          لغو
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <BacktestCreateModal
        isOpen={createOpen}
        onClose={() => setCreateOpen(false)}
        onCreated={() => refresh()}
      />
    </div>
  );
};