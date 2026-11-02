import React from "react";
import {
  BacktestStatus,
  BACKTEST_STATUS_LABELS,
  BACKTEST_STATUS_COLORS,
} from "../../../models";

interface Props {
  status: number;
}

export const BacktestStatusBadge: React.FC<Props> = ({ status }) => {
  const s = status as BacktestStatus;
  return (
    <span
      className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium whitespace-nowrap ${
        BACKTEST_STATUS_COLORS[s] ?? "bg-slate-700 text-slate-300"
      }`}
    >
      {BACKTEST_STATUS_LABELS[s] ?? "نامشخص"}
    </span>
  );
};