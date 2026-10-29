import React, { useEffect, useMemo, useState } from "react";
import { useTradingRunManagement } from "../../hooks/useTradingRunManagement";
import { strategyApi } from "../../api/strategyApi";
import { tradingRunApi } from "../../api/tradingRunApi";
import type {
  CreateTradingRunCommand,
  UpdateTradingRunCommand,
  TradingRunInfoView,
  TradingRunDetailView,
  StrategyInfoView,
} from "../../models";
import {
  TradingRunState,
  TRADING_RUN_STATE_LABELS,
  CapitalAllocationMode,
  CAPITAL_MODE_LABELS,
  TIMEFRAME_LABELS,
  TimeFrame,
  StopRunBehavior,
} from "../../models";
import { RunStateBadge } from "./components/RunStateBadge";
import { RunActions } from "./components/RunActions";
import { StopRunConfirmModal } from "./components/StopRunConfirmModal";
import { TradingRunFormModal } from "./components/TradingRunFormModal";

export const TradingRunsManagementPage: React.FC = () => {
  const {
    runs,
    loading,
    busyId,
    error,
    refresh,
    handleCreate,
    handleUpdate,
    handleStart,
    handleStop,
    handleDelete,
  } = useTradingRunManagement();

  const [strategies, setStrategies] = useState<StrategyInfoView[]>([]);
  const [formOpen, setFormOpen] = useState(false);
  const [formMode, setFormMode] = useState<"create" | "edit">("create");
  const [formDetail, setFormDetail] =
    useState<TradingRunDetailView | null>(null);
  const [formSaving, setFormSaving] = useState(false);

  const [stopTarget, setStopTarget] =
    useState<TradingRunInfoView | null>(null);
  const [stopSaving, setStopSaving] = useState(false);

  const [deleteTarget, setDeleteTarget] =
    useState<TradingRunInfoView | null>(null);
  const [deleteSaving, setDeleteSaving] = useState(false);

  const [search, setSearch] = useState("");
  const [stateFilter, setStateFilter] = useState<string>("");

  /* ─── Load strategies once ─── */
  useEffect(() => {
    (async () => {
      try {
        const data = await strategyApi.GetList();
        setStrategies(data);
      } catch {
        setStrategies([]);
      }
    })();
  }, []);

  /* ─── Filtered runs ─── */
  const filtered = useMemo(() => {
    return runs.filter((r) => {
      if (search.trim()) {
        const q = search.toLowerCase();
        const match =
          r.name.toLowerCase().includes(q) ||
          r.strategyKey.toLowerCase().includes(q);
        if (!match) return false;
      }
      if (stateFilter !== "" && r.state !== Number(stateFilter)) {
        return false;
      }
      return true;
    });
  }, [runs, search, stateFilter]);

  /* ─── Handlers ─── */
  const openCreate = () => {
    setFormMode("create");
    setFormDetail(null);
    setFormOpen(true);
  };

  const openEdit = async (run: TradingRunInfoView) => {
    try {
      const detail = await tradingRunApi.GetById(run.id);
      setFormDetail(detail);
      setFormMode("edit");
      setFormOpen(true);
    } catch (e) {
      alert(
        "خطا در دریافت جزئیات: " +
          (e instanceof Error ? e.message : "نامشخص"),
      );
    }
  };

  const submitForm = async (
    cmd: CreateTradingRunCommand | UpdateTradingRunCommand,
  ) => {
    setFormSaving(true);
    try {
      if ("id" in cmd) {
        await handleUpdate(cmd);
      } else {
        await handleCreate(cmd);
      }
      setFormOpen(false);
    } finally {
      setFormSaving(false);
    }
  };

  const confirmStop = async (behavior: StopRunBehavior) => {
    if (!stopTarget) return;
    setStopSaving(true);
    try {
      await handleStop(stopTarget.id, behavior);
      setStopTarget(null);
    } finally {
      setStopSaving(false);
    }
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    setDeleteSaving(true);
    try {
      await handleDelete(deleteTarget.id);
      setDeleteTarget(null);
    } finally {
      setDeleteSaving(false);
    }
  };

  /* ─── Render ─── */
  return (
    <div className="space-y-4 p-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-900 dark:text-white">
            پلن‌های معاملاتی
          </h1>
          <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
            {runs.length} پلن · {filtered.length} نمایش داده شده
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
            onClick={openCreate}
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
          >
            ➕ پلن جدید
          </button>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-2">
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="جستجو (نام یا استراتژی)..."
          className="flex-1 min-w-[200px] rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        />
        <select
          value={stateFilter}
          onChange={(e) => setStateFilter(e.target.value)}
          className="rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        >
          <option value="">همه‌ی وضعیت‌ها</option>
          {Object.entries(TRADING_RUN_STATE_LABELS).map(([k, v]) => (
            <option key={k} value={k}>
              {v}
            </option>
          ))}
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
              <th className="p-3">وضعیت</th>
              <th className="p-3">استراتژی</th>
              <th className="p-3">تایم‌فریم</th>
              <th className="p-3">ارز</th>
              <th className="p-3 text-center">ضریب</th>
              <th className="p-3">سرمایه</th>              
              <th className="p-3 text-center">نمادها</th>
              <th className="p-3 text-center">حساب‌ها</th>
              <th className="p-3">شروع/توقف</th>
              <th className="p-3 text-center">عملیات</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {loading ? (
              <tr>
                <td colSpan={10} className="p-6 text-center">
                  در حال بارگذاری...
                </td>
              </tr>
            ) : filtered.length === 0 ? (
              <tr>
                <td colSpan={10} className="p-6 text-center">
                  هیچ پلنی یافت نشد.
                </td>
              </tr>
            ) : (
              filtered.map((run) => {
                const capMode =
                  run.capitalAllocationMode as CapitalAllocationMode;
                const capLabel =
                  CAPITAL_MODE_LABELS[capMode] ?? "نامشخص";
                const capValue =
                  capMode === CapitalAllocationMode.Fixed && run.fixedCapital
                    ? `${run.fixedCapital.toLocaleString("fa-IR")} ریال`
                    : "داینامیک";
                const tf =
                  TIMEFRAME_LABELS[run.primaryTimeFrame as TimeFrame] ??
                  run.primaryTimeFrameName;

                return (
                  <tr
                    key={run.id}
                    className="hover:bg-gray-50 dark:hover:bg-gray-800/50"
                  >
                    <td className="p-3 font-medium">{run.name}</td>
                    <td className="p-3">
                      <RunStateBadge state={run.state} />
                    </td>
                    <td className="p-3 font-mono text-xs">
                      {run.strategyKey}
                    </td>
                    <td className="p-3">{tf}</td>
                    <td className="p-3 font-mono text-xs">
                      {run.targetCurrency ?? "IRR"}
                    </td>
                    <td className="p-3 text-center font-mono text-xs">
  {run.strengthMultiplier.toFixed(1)}×
</td>
                    <td className="p-3">
                      <span className="text-xs">
                        {capLabel}
                        <br />
                        <span className="text-gray-500">{capValue}</span>
                      </span>
                    </td>
                    <td className="p-3 text-center">{run.symbolCount}</td>
                    <td className="p-3 text-center">{run.accountCount}</td>
                    <td className="p-3 text-xs">
                      {run.startedAt && (
                        <div className="text-emerald-600 dark:text-emerald-400">
                          ▶ {new Date(run.startedAt).toLocaleString("fa-IR")}
                        </div>
                      )}
                      {run.stoppedAt && (
                        <div className="text-amber-600 dark:text-amber-400">
                          ⏹ {new Date(run.stoppedAt).toLocaleString("fa-IR")}
                        </div>
                      )}
                      {!run.startedAt && !run.stoppedAt && "—"}
                    </td>
                    <td className="p-3">
                      <RunActions
                        run={run}
                        busy={busyId === run.id}
                        onStart={() => handleStart(run.id)}
                        onStop={() => setStopTarget(run)}
                        onEdit={() => openEdit(run)}
                        onDelete={() => setDeleteTarget(run)}
                      />
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* Create / Edit Modal */}
      <TradingRunFormModal
        isOpen={formOpen}
        mode={formMode}
        initialDetail={formDetail}
        strategies={strategies}
        onClose={() => setFormOpen(false)}
        onSubmit={submitForm}
        saving={formSaving}
      />

      {/* Stop Confirm */}
      <StopRunConfirmModal
        isOpen={stopTarget !== null}
        runName={stopTarget?.name ?? ""}
        onCancel={() => setStopTarget(null)}
        onConfirm={confirmStop}
        saving={stopSaving}
      />

      {/* Delete Confirm */}
      {deleteTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl dark:bg-gray-800">
            <h3 className="mb-2 text-lg font-bold text-gray-900 dark:text-white">
              تایید حذف
            </h3>
            <p className="mb-4 text-sm text-gray-600 dark:text-gray-300">
              آیا از حذف پلن{" "}
              <strong className="text-gray-900 dark:text-white">
                {deleteTarget.name}
              </strong>{" "}
              اطمینان دارید؟
            </p>
            <div className="flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setDeleteTarget(null)}
                className="rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
              >
                انصراف
              </button>
              <button
                type="button"
                onClick={confirmDelete}
                disabled={deleteSaving}
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
              >
                {deleteSaving ? "در حال حذف..." : "حذف قطعی"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};