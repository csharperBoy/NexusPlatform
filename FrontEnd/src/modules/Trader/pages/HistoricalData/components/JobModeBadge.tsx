import React from "react";
import {
  HistoricalCollectionMode,
  HISTORICAL_MODE_LABELS,
  HISTORICAL_MODE_COLORS,
} from "../../../models";

interface Props {
  mode: number;
}

export const JobModeBadge: React.FC<Props> = ({ mode }) => {
  const m = mode as HistoricalCollectionMode;
  const label = HISTORICAL_MODE_LABELS[m] ?? "—";
  const colors =
    HISTORICAL_MODE_COLORS[m] ?? "bg-slate-700 text-slate-300";

  return (
    <span
      className={`inline-flex rounded-full px-2 py-0.5 text-[10px] font-medium whitespace-nowrap ${colors}`}
    >
      {m === HistoricalCollectionMode.Backfill ? "📥 " : "🔁 "}
      {label}
    </span>
  );
};