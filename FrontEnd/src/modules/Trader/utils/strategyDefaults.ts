// src/modules/Trader/utils/strategyDefaults.ts
import type { StrategyConfigFieldView } from "../models";
import {
  getStrategyEnumDef,
  getStrategyInputKind,
} from "../models/StrategyEnums";

/**
 * ساخت object پیش‌فرض از configFields استراتژی.
 * مقادیر string در defaultValue به number/boolean/enum-int تبدیل می‌شن.
 *
 * استفاده در: TradingRun create، Backtest virtual، هر جا که
 * استراتژی جدید انتخاب می‌شه و باید پارامترهاش با defaults پر شن.
 */
export function buildDefaultStrategyConfig(
  fields: StrategyConfigFieldView[],
): Record<string, unknown> {
  const init: Record<string, unknown> = {};

  for (const f of fields) {
    if (f.defaultValue == null || f.defaultValue === "") continue;

    const kind = getStrategyInputKind(f.typeName);

    if (kind === "number") {
      const n = Number(f.defaultValue);
      if (!isNaN(n)) init[f.name] = n;
      continue;
    }

    if (kind === "boolean") {
      init[f.name] =
        f.defaultValue === "true" || f.defaultValue === "True";
      continue;
    }

    if (kind === "enum") {
      const enumDef = getStrategyEnumDef(f.typeName);
      if (!enumDef) continue;
      const byName = enumDef[f.defaultValue];
      if (byName) {
        init[f.name] = byName.value;
        continue;
      }
      const n = Number(f.defaultValue);
      if (!isNaN(n)) init[f.name] = n;
      continue;
    }

    init[f.name] = f.defaultValue;
  }

  return init;
}

/**
 * Parse ایمن strategyConfigJson (مثلاً در edit mode).
 * اگر JSON نامعتبر بود → object خالی.
 */
export function parseStrategyConfig(
  json: string | null | undefined,
): Record<string, unknown> {
  if (!json) return {};
  try {
    const parsed = JSON.parse(json);
    return typeof parsed === "object" && parsed !== null
      ? (parsed as Record<string, unknown>)
      : {};
  } catch {
    return {};
  }
}