import React from "react";
import {
  HistoricalJobState,
  HISTORICAL_JOB_STATE_LABELS,
  HISTORICAL_JOB_STATE_COLORS,
} from "../../../models";

interface Props {
  state: number;
}

export const JobStateBadge: React.FC<Props> = ({ state }) => {
  const s = state as HistoricalJobState;
  const label = HISTORICAL_JOB_STATE_LABELS[s] ?? "نامشخص";
  const colors =
    HISTORICAL_JOB_STATE_COLORS[s] ?? "bg-slate-700 text-slate-300";

  return (
    <span
      className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium whitespace-nowrap ${colors}`}
    >
      {label}
    </span>
  );
};