import React, { useMemo, useState } from "react";
import { useHistoricalDataCollectionManagement } from "../../hooks/useHistoricalDataCollectionManagement";
import type {
  CreateHistoricalBackfillCommand,
  CreateHistoricalRecurringCommand,
  HistoricalDataCollectionJobSummaryView,
} from "../../models";
import {
  HistoricalCollectionMode,
  HistoricalJobState,
  TIMEFRAME_LABEL_MAP,
  formatRecurrencePattern,   // ← re-exported via index? see delta — using direct import below
} from "../../models";
import { formatRecurrencePattern as fmtPattern } from "../../models/PriceCollection";
import { JobStateBadge } from "./components/JobStateBadge";
import { JobModeBadge } from "./components/JobModeBadge";
import { JobActions } from "./components/JobActions";
import { CreateJobModal } from "./CreateJobModal";

export const HistoricalDataCollectionPage: React.FC = () => {
  const {
    jobs,
    loading,
    busyId,
    error,
    refresh,
    handleCreateBackfill,
    handleCreateRecurring,
    handleStart,
    handleStop,
  } = useHistoricalDataCollectionManagement();

  const [modalOpen, setModalOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [search, setSearch] = useState("");
  const [stateFilter, setStateFilter] = useState<string>("");

  const filtered = useMemo(() => {
    return jobs.filter((j) => {
      if (search.trim()) {
        const q = search.toLowerCase();
        if (!j.name.toLowerCase().includes(q)) return false;
      }
      if (stateFilter !== "" && j.state !== Number(stateFilter)) {
        return false;
      }
      return true;
    });
  }, [jobs, search, stateFilter]);

  const runningCount = jobs.filter(
    (j) => j.state === HistoricalJobState.Running,
  ).length;

  const submitBackfill = async (cmd: CreateHistoricalBackfillCommand) => {
    setSaving(true);
    try {
      await handleCreateBackfill(cmd);
      setModalOpen(false);
    } finally {
      setSaving(false);
    }
  };

  const submitRecurring = async (cmd: CreateHistoricalRecurringCommand) => {
    setSaving(true);
    try {
      await handleCreateRecurring(cmd);
      setModalOpen(false);
    } finally {
      setSaving(false);
    }
  };

  const formatDate = (iso: string | null): string => {
    if (!iso) return "—";
    return new Date(iso).toLocaleString("fa-IR");
  };

  const formatDuration = (ms: number | null): string => {
    if (!ms || ms <= 0) return "—";
    const s = Math.floor(ms / 1000);
    const m = Math.floor(s / 60);
    const h = Math.floor(m / 60);
    if (h > 0) return `${h}h ${m % 60}m`;
    if (m > 0) return `${m}m ${s % 60}s`;
    return `${s}s`;
  };

  return (
    <div className="space-y-4 p-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-900 dark:text-white">
            جمع‌آوری داده‌ی تاریخی
          </h1>
          <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
            {jobs.length} Job · {runningCount} در حال اجرا
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
            onClick={() => setModalOpen(true)}
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
          >
            ➕ Job جدید
          </button>
        </div>
      </div>

      {/* Info */}
      <div className="rounded-lg border border-blue-700 bg-blue-900/20 p-3 text-xs text-blue-200">
        ℹ️ <strong>Backfill</strong>: یک‌بار، از تاریخ شروع تا پایان.
        <strong className="mr-2">Recurring</strong>: هر tick از آخرین کندل موجود
        برای هر (نماد، تایم‌فریم) تا الان fetch می‌کند.
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-2">
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="جستجوی نام..."
          className="flex-1 min-w-[200px] rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        />
        <select
          value={stateFilter}
          onChange={(e) => setStateFilter(e.target.value)}
          className="rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        >
          <option value="">همه‌ی وضعیت‌ها</option>
          <option value="0">ایجاد شده</option>
          <option value="1">در حال اجرا</option>
          <option value="2">متوقف</option>
          <option value="3">تکمیل شده</option>
          <option value="4">خطا</option>
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
              <th className="p-3">نام</th>
              <th className="p-3">حالت</th>
              <th className="p-3">وضعیت</th>
              <th className="p-3">پیشرفت</th>
              <th className="p-3">آخرین اجرا</th>
              <th className="p-3 text-center">کل کندل</th>
              <th className="p-3 text-center">مدت</th>
              <th className="p-3 text-center">عملیات</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {loading && jobs.length === 0 ? (
              <tr>
                <td colSpan={8} className="p-6 text-center">
                  در حال بارگذاری...
                </td>
              </tr>
            ) : filtered.length === 0 ? (
              <tr>
                <td colSpan={8} className="p-6 text-center">
                  هیچ Jobی یافت نشد.
                </td>
              </tr>
            ) : (
              filtered.map((job) => (
                <JobRow
                  key={job.id}
                  job={job}
                  busy={busyId === job.id}
                  onStart={() => handleStart(job.id)}
                  onStop={() => handleStop(job.id)}
                  formatDate={formatDate}
                  formatDuration={formatDuration}
                />
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Modal */}
      <CreateJobModal
        isOpen={modalOpen}
        onClose={() => setModalOpen(false)}
        onSubmitBackfill={submitBackfill}
        onSubmitRecurring={submitRecurring}
        saving={saving}
      />
    </div>
  );
};

/* ═══════════ Row ═══════════ */

const JobRow: React.FC<{
  job: HistoricalDataCollectionJobSummaryView;
  busy: boolean;
  onStart: () => void;
  onStop: () => void;
  formatDate: (iso: string | null) => string;
  formatDuration: (ms: number | null) => string;
}> = ({ job, busy, onStart, onStop, formatDate, formatDuration }) => {
  const mode = job.mode as HistoricalCollectionMode;
  const state = job.state as HistoricalJobState;

  const showProgress =
    mode === HistoricalCollectionMode.Backfill &&
    state === HistoricalJobState.Running &&
    job.backfillProgressPercent != null;

  return (
    <tr className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
      <td className="p-3">
        <div className="font-medium">{job.name}</div>
        {job.currentIsin && (
          <div className="text-[10px] text-gray-500">
            در حال پردازش: {job.currentIsin}
            {job.currentTimeFrame != null && (
              <> — {TIMEFRAME_LABEL_MAP[job.currentTimeFrame] ?? job.currentTimeFrame}</>
            )}
          </div>
        )}
      </td>
      <td className="p-3">
        <JobModeBadge mode={job.mode} />
        {job.recurrencePatternJson && (
          <div className="mt-1 text-[10px] text-gray-500">
            {fmtPattern(job.recurrencePatternJson)}
          </div>
        )}
      </td>
      <td className="p-3">
        <JobStateBadge state={job.state} />
        {job.lastError && (
          <div
            className="mt-1 max-w-[240px] truncate text-[10px] text-red-400"
            title={job.lastError}
          >
            {job.lastError}
          </div>
        )}
      </td>
      <td className="p-3 text-xs">
        {showProgress ? (
          <>
            <div className="mb-1 flex items-center justify-between text-[10px]">
              <span>
                {job.completedCombinations}/{job.totalCombinations}
              </span>
              <span>{(job.backfillProgressPercent ?? 0).toFixed(1)}%</span>
            </div>
            <div className="h-2 w-32 overflow-hidden rounded bg-slate-700">
              <div
                className="h-2 bg-blue-500 transition-all"
                style={{ width: `${job.backfillProgressPercent ?? 0}%` }}
              />
            </div>
          </>
        ) : job.backfillProgressPercent != null &&
          mode === HistoricalCollectionMode.Backfill ? (
          <span className="text-[10px] text-gray-500">
            {job.backfillProgressPercent.toFixed(1)}%
          </span>
        ) : (
          <span className="text-[10px] text-gray-500">—</span>
        )}
      </td>
      <td className="p-3 text-xs">
        {job.lastRunAt ? (
          <>
            <div>{formatDate(job.lastRunAt)}</div>
            <div className="text-[10px] text-gray-500">
              {job.lastRunInserted} درج / {job.lastRunSkipped} skip
            </div>
          </>
        ) : job.startedAt ? (
          <div className="text-emerald-600 dark:text-emerald-400">
            ▶ {formatDate(job.startedAt)}
          </div>
        ) : (
          "—"
        )}
      </td>
      <td className="p-3 text-center font-mono text-xs">
        {job.totalCandlesCollected.toLocaleString("en-US")}
      </td>
      <td className="p-3 text-center text-xs">
        {formatDuration(job.durationMs)}
      </td>
      <td className="p-3">
        <JobActions
          job={job}
          busy={busy}
          onStart={onStart}
          onStop={onStop}
        />
      </td>
    </tr>
  );
};