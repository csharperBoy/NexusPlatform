import React from "react";
import {
  OrderSide,
  ORDER_STATE_COLORS,
  INTENT_STATE_COLORS,
} from "../../../models";

/* ═══════════════ State Badge ═══════════════ */

interface StateBadgeProps {
  state: number;
  stateName: string;
  variant: "broker" | "intent";
}

export const StateBadge: React.FC<StateBadgeProps> = ({
  state,
  stateName,
  variant,
}) => {
  const colors =
    (variant === "broker" ? ORDER_STATE_COLORS : INTENT_STATE_COLORS)[state] ??
    "bg-slate-700 text-slate-300";

  return (
    <span
      className={`inline-flex rounded-full px-2 py-0.5 text-[11px] font-medium whitespace-nowrap ${colors}`}
    >
      {stateName}
    </span>
  );
};

/* ═══════════════ Side Badge ═══════════════ */

interface SideBadgeProps {
  side: number;
  sideName: string;
}

export const SideBadge: React.FC<SideBadgeProps> = ({ side, sideName }) => {
  const isBuy = side === OrderSide.Buy;
  return (
    <span
      className={`inline-flex rounded px-2 py-0.5 text-[11px] font-bold whitespace-nowrap ${
        isBuy
          ? "bg-emerald-900/40 text-emerald-300"
          : "bg-red-900/40 text-red-300"
      }`}
    >
      {isBuy ? "▲" : "▼"} {sideName}
    </span>
  );
};

/* ═══════════════ Paper / Live Badge ═══════════════ */

interface PaperBadgeProps {
  isPaper: boolean;
}

export const PaperBadge: React.FC<PaperBadgeProps> = ({ isPaper }) => {
  return (
    <span
      className={`inline-flex rounded-full px-2 py-0.5 text-[10px] font-medium whitespace-nowrap ${
        isPaper
          ? "bg-cyan-900/40 text-cyan-300"
          : "bg-red-900/40 text-red-300"
      }`}
    >
      {isPaper ? "📄 Paper" : "🔴 Live"}
    </span>
  );
};