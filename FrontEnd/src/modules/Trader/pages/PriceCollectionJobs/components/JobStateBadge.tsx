import React from "react";
import {
  PriceCollectionJobState,
  PRICE_COLLECTION_JOB_STATE_LABELS,
  PRICE_COLLECTION_JOB_STATE_COLORS,
} from "../../../models";

interface Props {
  state: number;
}

export const JobStateBadge: React.FC<Props> = ({ state }) => {
  const s = state as PriceCollectionJobState;
  const label = PRICE_COLLECTION_JOB_STATE_LABELS[s] ?? "نامشخص";
  const colors =
    PRICE_COLLECTION_JOB_STATE_COLORS[s] ?? "bg-slate-700 text-slate-300";

  return (
    <span
      className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium whitespace-nowrap ${colors}`}
    >
      {label}
    </span>
  );
};