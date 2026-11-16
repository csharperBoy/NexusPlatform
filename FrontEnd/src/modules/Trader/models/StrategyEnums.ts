// src/modules/Trader/models/StrategyEnums.ts
/* ═══════════════ Shared Strategy Enum Registry ═══════════════
   Single source of truth for enum defs used by any strategy config.
   Used by:
     • StrategyConfigEditor (rendering)
     • strategyDefaults (default init)
   ════════════════════════════════════════════════════════════ */

import { TimeFrame, TIMEFRAME_LABELS } from "./TradingRunEnums";

export interface StrategyEnumOption {
  label: string;
  value: number;
}

export const STRATEGY_ENUM_REGISTRY: Record<
  string,
  Record<string, StrategyEnumOption>
> = {
  TimeFrame: {
    M1:  { label: TIMEFRAME_LABELS[TimeFrame.M1],  value: TimeFrame.M1 },
    M5:  { label: TIMEFRAME_LABELS[TimeFrame.M5],  value: TimeFrame.M5 },
    M15: { label: TIMEFRAME_LABELS[TimeFrame.M15], value: TimeFrame.M15 },
    M30: { label: TIMEFRAME_LABELS[TimeFrame.M30], value: TimeFrame.M30 },
    H1:  { label: TIMEFRAME_LABELS[TimeFrame.H1],  value: TimeFrame.H1 },
    H4:  { label: TIMEFRAME_LABELS[TimeFrame.H4],  value: TimeFrame.H4 },
    D1:  { label: TIMEFRAME_LABELS[TimeFrame.D1],  value: TimeFrame.D1 },
    W1:  { label: TIMEFRAME_LABELS[TimeFrame.W1],  value: TimeFrame.W1 },
  },
};

export type StrategyInputKind = "number" | "text" | "boolean" | "enum";

export function getStrategyEnumDef(
  typeName: string,
): Record<string, StrategyEnumOption> | null {
  return STRATEGY_ENUM_REGISTRY[typeName] ?? null;
}

export function isStrategyEnumType(typeName: string): boolean {
  return getStrategyEnumDef(typeName) !== null;
}

export function getStrategyInputKind(typeName: string): StrategyInputKind {
  if (isStrategyEnumType(typeName)) return "enum";
  const t = typeName.toLowerCase();
  if (
    t.includes("int") ||
    t.includes("decimal") ||
    t.includes("double") ||
    t.includes("float") ||
    t.includes("long")
  )
    return "number";
  if (t === "boolean" || t === "bool") return "boolean";
  return "text";
}

export function isDecimalType(typeName: string): boolean {
  const t = typeName.toLowerCase();
  return t.includes("decimal") || t.includes("double") || t.includes("float");
}

export function normalizeEnumValue(
  raw: unknown,
  def: Record<string, StrategyEnumOption>,
): number | "" {
  if (raw === null || raw === undefined || raw === "") return "";
  if (typeof raw === "number") return raw;
  if (typeof raw === "string") {
    const byName = def[raw];
    if (byName) return byName.value;
    const n = Number(raw);
    if (!isNaN(n)) return n;
  }
  return "";
}