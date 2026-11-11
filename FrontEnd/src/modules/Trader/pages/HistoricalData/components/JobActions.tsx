import React from "react";
import {
  HistoricalJobState,
  HistoricalCollectionMode,
  type HistoricalDataCollectionJobSummaryView,
} from "../../../models";

interface Props {
  job: HistoricalDataCollectionJobSummaryView;
  busy: boolean;
  onStart: () => void;
  onStop: () => void;
}

export const JobActions: React.FC<Props> = ({
  job,
  busy,
  onStart,
  onStop,
}) => {
  const state = job.state as HistoricalJobState;
  const mode = job.mode as HistoricalCollectionMode;

  const canStart =
    state === HistoricalJobState.Created ||
    state === HistoricalJobState.Stopped ||
    state === HistoricalJobState.Faulted;

  const canStop = state === HistoricalJobState.Running;

  const btn =
    "rounded px-2.5 py-1 text-xs font-medium disabled:opacity-40 whitespace-nowrap";

  /* Backfill completed → terminal، no further action */
  if (
    mode === HistoricalCollectionMode.Backfill &&
    state === HistoricalJobState.Completed
  ) {
    return (
      <span className="text-xs text-gray-500">پایان یافته</span>
    );
  }

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
      {!canStart && !canStop && (
        <span className="text-xs text-gray-500">—</span>
      )}
    </div>
  );
};