import React from "react";
import {
  TradingRunState,
  TRADING_RUN_STATE_LABELS,
  TRADING_RUN_STATE_COLORS,
} from "../../../models";

interface Props {
  state: number;
}

export const RunStateBadge: React.FC<Props> = ({ state }) => {
  const s = state as TradingRunState;
  const label = TRADING_RUN_STATE_LABELS[s] ?? "نامشخص";
  const colors = TRADING_RUN_STATE_COLORS[s] ?? "bg-slate-700 text-slate-300";

  return (
    <span
      className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium whitespace-nowrap ${colors}`}
    >
      {label}
    </span>
  );
};