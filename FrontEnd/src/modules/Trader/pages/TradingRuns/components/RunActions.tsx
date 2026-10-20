import React from "react";
import { TradingRunState, type TradingRunInfoView } from "../../../models";

interface Props {
  run: TradingRunInfoView;
  busy: boolean;
  onStart: () => void;
  onStop: () => void;
  onEdit: () => void;
  onDelete: () => void;
}

export const RunActions: React.FC<Props> = ({
  run,
  busy,
  onStart,
  onStop,
  onEdit,
  onDelete,
}) => {
  const state = run.state as TradingRunState;
  const canStart =
    state === TradingRunState.Ready || state === TradingRunState.Stopped;
  const canStop = state === TradingRunState.Running;
  const canEdit = state !== TradingRunState.Running;
  const canDelete = state !== TradingRunState.Running;

  const btn =
    "rounded px-2.5 py-1 text-xs font-medium disabled:opacity-40 whitespace-nowrap";

  return (
    <div className="flex flex-wrap justify-end gap-1">
      {canStart && (
        <button
          type="button"
          disabled={busy}
          onClick={onStart}
          className={`${btn} bg-emerald-600 text-white hover:bg-emerald-700`}
          title="شروع"
        >
          ▶ شروع
        </button>
      )}
      {canStop && (
        <button
          type="button"
          disabled={busy}
          onClick={onStop}
          className={`${btn} bg-amber-600 text-white hover:bg-amber-700`}
          title="توقف"
        >
          ⏹ توقف
        </button>
      )}
      {canEdit && (
        <button
          type="button"
          disabled={busy}
          onClick={onEdit}
          className={`${btn} border border-gray-300 text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700`}
          title="ویرایش"
        >
          ✏ ویرایش
        </button>
      )}
      {canDelete && (
        <button
          type="button"
          disabled={busy}
          onClick={onDelete}
          className={`${btn} bg-red-600 text-white hover:bg-red-700`}
          title="حذف"
        >
          🗑 حذف
        </button>
      )}
    </div>
  );
};