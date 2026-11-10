import React from "react";
import {
  PriceCollectionJobState,
  type PriceCollectionJobView,
} from "../../../models";

interface Props {
  job: PriceCollectionJobView;
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
  const state = job.state as PriceCollectionJobState;

  const canStart =
    state === PriceCollectionJobState.Created ||
    state === PriceCollectionJobState.Stopped ||
    state === PriceCollectionJobState.Faulted;

  const canStop = state === PriceCollectionJobState.Running;

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
      {!canStart && !canStop && (
        <span className="text-xs text-gray-500">—</span>
      )}
    </div>
  );
};