import React, { useState } from "react";
import { useHistoricalDataCollection } from "../../hooks/useHistoricalDataCollection";
import { CollectionStatusBadge } from "./CollectionStatusBadge";
import { CreateCollectionModal } from "./CreateCollectionModal";
import {
  CollectionJobStatus,
  TIMEFRAME_LABEL_MAP,
} from "../../models";

const fmtNum = (v: number): string => v.toLocaleString("fa-IR");

const fmtDate = (s: string | null): string => {
  if (!s) return "—";
  const d = new Date(s);
  return d.toLocaleString("fa-IR", {
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
};

const fmtDuration = (ms: number | null): string => {
  if (!ms) return "—";
  const s = ms / 1000;
  if (s < 60) return `${s.toFixed(0)}s`;
  if (s < 3600) return `${(s / 60).toFixed(1)}m`;
  return `${(s / 3600).toFixed(1)}h`;
};

export const HistoricalDataCollectionPage: React.FC = () => {
  const { jobs, loading, error, refresh, handleCancel } =
    useHistoricalDataCollection();
  const [createOpen, setCreateOpen] = useState(false);

  return (
    <div className="space-y-4 p-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-900 dark:text-white">
            جمع‌آوری داده‌ی تاریخی
          </h1>
          <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
            {jobs.length} Job ثبت‌شده
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
            ➕ Job جدید
          </button>
        </div>
      </div>

      <div className="rounded-lg border border-blue-200 bg-blue-50 p-3 text-xs text-blue-700 dark:border-blue-800 dark:bg-blue-900/20 dark:text-blue-300">
        ℹ️ داده‌های جمع‌آوری‌شده در جدول <code>Candles</code> ذخیره می‌شوند و
        برای Backtest استفاده می‌شوند. برای Backtest دقیق، M1 و M5 توصیه
        می‌شود.
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
              <th className="p-3">پیشرفت</th>
              <th className="p-3 text-center">کندل‌ها</th>
              <th className="p-3">شروع</th>
              <th className="p-3">مدت</th>
              <th className="p-3 text-center">عملیات</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {loading && jobs.length === 0 ? (
              <tr>
                <td colSpan={7} className="p-6 text-center">
                  در حال بارگذاری...
                </td>
              </tr>
            ) : jobs.length === 0 ? (
              <tr>
                <td colSpan={7} className="p-6 text-center text-gray-500">
                  هیچ Jobی وجود ندارد. یک Job جدید بسازید.
                </td>
              </tr>
            ) : (
              jobs.map((j) => (
                <tr
                  key={j.id}
                  className="hover:bg-gray-50 dark:hover:bg-gray-700/30"
                >
                  <td className="p-3 font-medium">{j.name}</td>
                  <td className="p-3">
                    <CollectionStatusBadge status={j.status} />
                  </td>
                  <td className="p-3">
                    <div className="flex flex-col gap-1">
                      <div className="h-1.5 w-32 overflow-hidden rounded bg-gray-200 dark:bg-gray-700">
                        <div
                          className={`h-full transition-all ${
                            j.status === CollectionJobStatus.Failed
                              ? "bg-red-500"
                              : j.status === CollectionJobStatus.Completed
                                ? "bg-emerald-500"
                                : "bg-blue-500"
                          }`}
                          style={{ width: `${j.progressPercent}%` }}
                        />
                      </div>
                      <span className="text-[10px] text-gray-500">
                        {j.progressPercent.toFixed(1)}% ·{" "}
                        {j.completedCombinations}/{j.totalCombinations}
                      </span>
                      {j.currentIsin && (
                        <span className="text-[10px] text-gray-400">
                          {j.currentIsin}
                          {j.currentTimeFrame
                            ? ` · ${TIMEFRAME_LABEL_MAP[j.currentTimeFrame] ?? "?"}`
                            : ""}
                        </span>
                      )}
                    </div>
                  </td>
                  <td className="p-3 text-center font-mono text-xs">
                    {fmtNum(j.totalCandlesCollected)}
                  </td>
                  <td className="p-3 text-xs">{fmtDate(j.startedAt)}</td>
                  <td className="p-3 text-xs">{fmtDuration(j.durationMs)}</td>
                  <td className="p-3">
                    <div className="flex justify-end">
                      {(j.status === CollectionJobStatus.Pending ||
                        j.status === CollectionJobStatus.Running) && (
                        <button
                          type="button"
                          onClick={() => {
                            if (confirm(`لغو Job «${j.name}»؟`)) {
                              void handleCancel(j.id);
                            }
                          }}
                          className="rounded bg-red-600 px-2 py-1 text-xs text-white hover:bg-red-700"
                        >
                          لغو
                        </button>
                      )}
                      {j.errorMessage && (
                        <span
                          className="cursor-help text-red-500"
                          title={j.errorMessage}
                        >
                          ⚠️
                        </span>
                      )}
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <CreateCollectionModal
        isOpen={createOpen}
        onClose={() => setCreateOpen(false)}
        onCreated={() => refresh()}
      />
    </div>
  );
};