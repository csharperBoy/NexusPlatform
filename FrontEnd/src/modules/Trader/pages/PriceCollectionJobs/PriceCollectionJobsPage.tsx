import React, { useEffect, useMemo, useState } from "react";
import { usePriceCollectionJobManagement } from "../../hooks/usePriceCollectionJobManagement";
import { priceSourceApi } from "../../api/priceCollectionApi";
import type {
  CreatePriceCollectionJobCommand,
  PriceSourceView,
  PriceCollectionJobView,
} from "../../models";
import { formatRecurrencePattern } from "../../models";
import { JobStateBadge } from "./components/JobStateBadge";
import { JobModeBadge } from "./components/JobModeBadge";
import { JobActions } from "./components/JobActions";
import { CreateJobModal } from "./components/CreateJobModal";

export const PriceCollectionJobsPage: React.FC = () => {
  const {
    jobs,
    loading,
    busyId,
    error,
    refresh,
    handleCreate,
    handleStart,
    handleStop,
  } = usePriceCollectionJobManagement();

  const [sources, setSources] = useState<PriceSourceView[]>([]);
  const [createOpen, setCreateOpen] = useState(false);
  const [creating, setCreating] = useState(false);

  useEffect(() => {
    (async () => {
      try {
        const data = await priceSourceApi.GetList();
        setSources(data.filter((s) => s.isActive));
      } catch {
        setSources([]);
      }
    })();
  }, []);

  const activeSources = useMemo(
    () => sources.filter((s) => s.isActive),
    [sources],
  );

  const submitCreate = async (cmd: CreatePriceCollectionJobCommand) => {
    setCreating(true);
    try {
      await handleCreate(cmd);
      setCreateOpen(false);
    } finally {
      setCreating(false);
    }
  };

  const formatDate = (iso: string | null): string => {
    if (!iso) return "—";
    return new Date(iso).toLocaleString("fa-IR");
  };

  return (
    <div className="space-y-4 p-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-900 dark:text-white">
            Jobهای جمع‌آوری قیمت
          </h1>
          <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
            {jobs.length} Job · {jobs.filter((j) => j.state === 1).length} در حال اجرا
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={refresh}
            disabled={loading}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 disabled:opacity-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
          >
            🔄 بروزرسانی
          </button>
          <button
            type="button"
            onClick={() => setCreateOpen(true)}
            disabled={activeSources.length === 0}
            title={
              activeSources.length === 0
                ? "ابتدا یک منبع فعال بسازید"
                : ""
            }
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
          >
            ➕ Job جدید
          </button>
        </div>
      </div>

      {/* Info */}
      <div className="rounded-lg border border-blue-700 bg-blue-900/20 p-3 text-xs text-blue-200">
        ℹ️ <strong>Backfill</strong>: یک‌بار اجرا از ابتدا تا max موجود.{" "}
        <strong>Recurring</strong>: در هر tick فقط رکوردهای جدیدتر از max را Sync می‌کند.
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
              <th className="p-3">منبع</th>
              <th className="p-3">حالت</th>
              <th className="p-3">وضعیت</th>
              <th className="p-3">الگو</th>
              <th className="p-3">آخرین اجرا</th>
              <th className="p-3 text-center">کل درج</th>
              <th className="p-3">شروع/توقف</th>
              <th className="p-3 text-center">عملیات</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {loading ? (
              <tr>
                <td colSpan={8} className="p-6 text-center">
                  در حال بارگذاری...
                </td>
              </tr>
            ) : jobs.length === 0 ? (
              <tr>
                <td colSpan={8} className="p-6 text-center">
                  هیچ Jobی ایجاد نشده.
                </td>
              </tr>
            ) : (
              jobs.map((job) => (
                <tr
                  key={job.id}
                  className="hover:bg-gray-50 dark:hover:bg-gray-800/50"
                >
                  <td className="p-3">
                    <div className="font-medium">{job.priceSourceName}</div>
                    <div className="font-mono text-[10px] text-gray-500">
                      {job.priceSourceCode}
                    </div>
                  </td>
                  <td className="p-3">
                    <JobModeBadge mode={job.mode} />
                  </td>
                  <td className="p-3">
                    <JobStateBadge state={job.state} />
                    {job.lastError && (
                      <div
                        className="mt-1 max-w-[200px] truncate text-[10px] text-red-400"
                        title={job.lastError}
                      >
                        {job.lastError}
                      </div>
                    )}
                  </td>
                  <td className="p-3 text-xs">
                    {formatRecurrencePattern(job.recurrencePatternJson)}
                  </td>
                  <td className="p-3 text-xs">
                    {job.lastRunAt ? (
                      <>
                        <div>{formatDate(job.lastRunAt)}</div>
                        <div className="text-[10px] text-gray-500">
                          {job.lastRunInserted} درج / {job.lastRunSkipped} skip
                        </div>
                      </>
                    ) : (
                      "—"
                    )}
                  </td>
                  <td className="p-3 text-center font-mono text-xs">
                    {job.totalInserted.toLocaleString("en-US")}
                  </td>
                  <td className="p-3 text-xs">
                    {job.startedAt && (
                      <div className="text-emerald-600 dark:text-emerald-400">
                        ▶ {formatDate(job.startedAt)}
                      </div>
                    )}
                    {job.stoppedAt && (
                      <div className="text-amber-600 dark:text-amber-400">
                        ⏹ {formatDate(job.stoppedAt)}
                      </div>
                    )}
                    {!job.startedAt && !job.stoppedAt && "—"}
                  </td>
                  <td className="p-3">
                    <JobActions
                      job={job}
                      busy={busyId === job.id}
                      onStart={() => handleStart(job.id)}
                      onStop={() => handleStop(job.id)}
                    />
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Create Modal */}
      <CreateJobModal
        isOpen={createOpen}
        sources={activeSources}
        onClose={() => setCreateOpen(false)}
        onSubmit={submitCreate}
        saving={creating}
      />
    </div>
  );
};