//src/modules/Trader/pages/TradingRuns/components/StrategyConfigEditor.tsx
import React from "react";
import type { StrategyConfigFieldView } from "../../../models";
import { TimeFrame, TIMEFRAME_LABELS } from "../../../models";

interface Props {
  fields: StrategyConfigFieldView[];
  values: Record<string, any>;
  onChange: (name: string, value: any) => void;
}

type InputKind = "number" | "text" | "boolean" | "enum";

/* ─── Enum registry: TypeName → { Name: { label, value } } ─── */
const ENUM_REGISTRY: Record<
  string,
  Record<string, { label: string; value: number }>
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

function getEnumDef(typeName: string) {
  return ENUM_REGISTRY[typeName] ?? null;
}

function getInputKind(typeName: string): InputKind {
  if (getEnumDef(typeName)) return "enum";
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

function isDecimal(typeName: string): boolean {
  const t = typeName.toLowerCase();
  return t.includes("decimal") || t.includes("double") || t.includes("float");
}

/**
 * مقدار ورودی می‌تونه باشه: int (از JSON)، اسم enum (از backend default)،
 * null/undefined (اولین init). همه رو به int یا "" (empty) نرمالایز می‌کنیم.
 */
function normalizeEnumValue(
  raw: any,
  def: Record<string, { label: string; value: number }>,
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

export const StrategyConfigEditor: React.FC<Props> = ({
  fields,
  values,
  onChange,
}) => {
  if (fields.length === 0) {
    return (
      <p className="rounded bg-slate-100 p-3 text-xs text-slate-600 dark:bg-slate-700 dark:text-slate-300">
        این استراتژی پارامتر قابل تنظیمی ندارد
      </p>
    );
  }

  return (
    <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
      {fields.map((f) => {
        const kind = getInputKind(f.typeName);
        const enumDef = getEnumDef(f.typeName);
        const raw = values[f.name];
        const current = raw ?? f.defaultValue ?? "";

        return (
          <div key={f.name} className="flex flex-col gap-1">
            <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
              <span className="font-mono">{f.name}</span>
              {f.isRequired && <span className="mr-1 text-red-500">*</span>}
              <span className="mr-1 text-[10px] font-normal text-gray-500">
                ({f.typeName})
              </span>
            </label>

            {kind === "boolean" ? (
              <input
                type="checkbox"
                checked={!!current}
                onChange={(e) => onChange(f.name, e.target.checked)}
                className="h-5 w-5 rounded border-gray-300 text-blue-600"
              />
            ) : kind === "enum" && enumDef ? (
              <select
                value={String(normalizeEnumValue(current, enumDef))}
                onChange={(e) => {
                  const v = e.target.value;
                  onChange(f.name, v === "" ? null : Number(v));
                }}
                dir="ltr"
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              >
                <option value="">—</option>
                {Object.entries(enumDef).map(([name, opt]) => (
                  <option key={name} value={opt.value}>
                    {opt.label}
                  </option>
                ))}
              </select>
            ) : (
              <input
                type={kind === "number" ? "number" : "text"}
                value={String(current)}
                step={kind === "number" && isDecimal(f.typeName) ? "0.01" : "1"}
                onChange={(e) => {
                  const v = e.target.value;
                  onChange(f.name, kind === "number" ? Number(v) : v);
                }}
                dir="ltr"
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            )}
          </div>
        );
      })}
    </div>
  );
};