import React from "react";
import {
  CollectionJobStatus,
  COLLECTION_STATUS_LABELS,
  COLLECTION_STATUS_COLORS,
} from "../../models";

interface Props {
  status: number;
}

export const CollectionStatusBadge: React.FC<Props> = ({ status }) => {
  const s = status as CollectionJobStatus;
  return (
    <span
      className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium whitespace-nowrap ${
        COLLECTION_STATUS_COLORS[s] ?? "bg-slate-700 text-slate-300"
      }`}
    >
      {COLLECTION_STATUS_LABELS[s] ?? "نامشخص"}
    </span>
  );
};