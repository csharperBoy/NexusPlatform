import React from "react";
import type { StrategyConfigFieldView } from "../../../models";

interface Props {
  fields: StrategyConfigFieldView[];
  values: Record<string, any>;
  onChange: (name: string, value: any) => void;
}

type InputKind = "number" | "text" | "boolean";

function getInputKind(typeName: string): InputKind {
  const t = typeName.toLowerCase();
  if (
    t.includes("int") ||
    t.includes("decimal") ||
    t.includes("double") ||
    t.includes("float") ||
    t.includes("long")
  ) return "number";
  if (t === "boolean" || t === "bool") return "boolean";
  return "text";
}

function isDecimal(typeName: string): boolean {
  const t = typeName.toLowerCase();
  return t.includes("decimal") || t.includes("double") || t.includes("float");
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
            ) : (
              <input
                type={kind === "number" ? "number" : "text"}
                value={String(current)}
                step={
                  kind === "number" && isDecimal(f.typeName) ? "0.01" : "1"
                }
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