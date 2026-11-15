//src/modules/Trader/pages/TradingRuns/components/StrategyConfigEditor.tsx
import React, { useMemo } from "react";
import type { StrategyConfigFieldView } from "../../../models";
import { TimeFrame, TIMEFRAME_LABELS } from "../../../models";

interface Props {
  strategyKey: string;      // ← جدید
  fields: StrategyConfigFieldView[];
  values: Record<string, any>;
  onChange: (name: string, value: any) => void;
}

type InputKind = "number" | "text" | "boolean" | "enum";

/* ─── Enum registry ─── */
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

/* ─── Group config ─── */
interface FieldGroup {
  title: string;
  description?: string;
  fieldNames: string[];   // ← نام property
}

/**
 * گروه‌بندی fields برای استراتژی‌های خاص. برای استراتژی‌های ناشناخته،
 * همه در یک گروه «پارامترها» نمایش داده می‌شن.
 */
const STRATEGY_GROUPS: Record<string, FieldGroup[]> = {
  "mtf-composite": [
    {
      title: "لایه ۱ — روند (Trend)",
      description: "جهت کلی بازار از روی EMA",
      fieldNames: ["EmaFastPeriod", "EmaSlowPeriod", "TrendTimeFrame"],
    },
    {
      title: "لایه ۲ — مومنتوم (Momentum)",
      description: "قدرت حرکت با RSI و MACD",
      fieldNames: [
        "RsiPeriod", "RsiBullishThreshold", "RsiBearishThreshold",
        "MacdFast", "MacdSlow", "MacdSignal",
      ],
    },
    {
      title: "لایه ۳ — میکرو (Microstructure)",
      description: "حجم و صف خرید/فروش",
      fieldNames: [
        "VolumeMultiplier", "BuyRatioThreshold", "SellRatioThreshold",
        "OrderBookDepthRatio",
      ],
    },
    {
      title: "پایه‌های حجم (Volume Baselines)",
      description: "خط پایه‌ی تحلیل حجم عادی و Opening",
      fieldNames: [
        "IntraDayBaselineCandles", "OpeningBaselineDays",
        "MinBaselineSamples",
      ],
    },
    {
      title: "وزن‌ها (Weights)",
      description: "سهم هر لایه در Strength — جمع باید ۱٫۰ باشه",
      fieldNames: ["TrendWeight", "MomentumWeight", "MicroWeight"],
    },
    {
      title: "آستانه‌های سیگنال (Signal Thresholds)",
      description: "کی سیگنال صادر بشه؟",
      fieldNames: ["MinSignalStrength", "MinStrengthDiff"],
    },
    {
      title: "حد ضرر و سود (SL/TP — ATR-based)",
      description: "پیشنهاد Strategy — Run-level Override می‌کنه",
      fieldNames: [
        "AtrPeriod", "AtrStopMultiplier", "AtrTakeProfitMultiplier",
      ],
    },
    {
      title: "Intra-Candle",
      description: "تصمیم قبل از بسته شدن کندل با Sub-Candle",
      fieldNames: [
        "EnableIntraCandleDecision", "SubCandleTimeFrame",
        "SubCandleHistoryDepth", "IntraCandleVolumeThreshold",
      ],
    },
  ],
};

const FIELD_DESCRIPTIONS: Record<string, string> = {
  // Trend
  EmaFastPeriod: "دوره EMA سریع برای تشخیص روند صعودی",
  EmaSlowPeriod: "دوره EMA کند — باید بزرگ‌تر از Fast باشه",
  TrendTimeFrame: "تایم‌فریم تحلیل روند — باید ≥ Primary",

  // Momentum
  RsiPeriod: "دوره RSI (معمولاً ۱۴)",
  RsiBullishThreshold: "RSI بالای این → مومنتوم صعودی",
  RsiBearishThreshold: "RSI زیر این → مومنتوم نزولی",
  MacdFast: "دوره EMA سریع MACD",
  MacdSlow: "دوره EMA کند MACD",
  MacdSignal: "دوره signal EMA",

  // Micro
  VolumeMultiplier: "چند برابر baseline = VolumeScore=1 (پیش‌فرض 1.5)",
  BuyRatioThreshold: "BuyRatio بالای این → Order Book صعودی",
  SellRatioThreshold: "SellRatio بالای این → Order Book نزولی",
  OrderBookDepthRatio: "نسبت عمق Bid/Ask برای سیگنال Order Book",

  // Baselines
  IntraDayBaselineCandles: "تعداد کندل عادی برای baseline حجم",
  OpeningBaselineDays: "تعداد روز Opening Candle برای baseline جدا",
  MinBaselineSamples: "حداقل نمونه برای baseline معتبر",

  // Weights
  TrendWeight: "وزن لایه‌ی Trend در Strength (0-1)",
  MomentumWeight: "وزن لایه‌ی Momentum در Strength (0-1)",
  MicroWeight: "وزن لایه‌ی Micro در Strength (0-1)",

  // Signal
  MinSignalStrength: "حداقل Strength برای صدور سیگنال",
  MinStrengthDiff: "حداقل اختلاف Bullish و Bearish (ضد flip-flop)",

  // SL/TP
  AtrPeriod: "دوره ATR (معمولاً ۱۴)",
  AtrStopMultiplier: "فاصله‌ی SL = این × ATR از Entry",
  AtrTakeProfitMultiplier: "فاصله‌ی TP = این × ATR از Entry",

  // Intra-Candle
  EnableIntraCandleDecision: "تصمیم زودتر از بسته شدن کندل (Live)",
  SubCandleTimeFrame: "تایم‌فریم کندل‌های ریز (پیش‌فرض M5)",
  SubCandleHistoryDepth: "تعداد کندل sub برای baseline",
  IntraCandleVolumeThreshold: "حداقل ratio حجم برای trigger",
};

function getEnumDef(typeName: string) {
  return ENUM_REGISTRY[typeName] ?? null;
}

function getInputKind(typeName: string): InputKind {
  if (getEnumDef(typeName)) return "enum";
  const t = typeName.toLowerCase();
  if (
    t.includes("int") || t.includes("decimal") ||
    t.includes("double") || t.includes("float") || t.includes("long")
  )
    return "number";
  if (t === "boolean" || t === "bool") return "boolean";
  return "text";
}

function isDecimal(typeName: string): boolean {
  const t = typeName.toLowerCase();
  return t.includes("decimal") || t.includes("double") || t.includes("float");
}

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

/* ─── Single field renderer ─── */
const FieldRenderer: React.FC<{
  field: StrategyConfigFieldView;
  values: Record<string, any>;
  onChange: (name: string, value: any) => void;
}> = ({ field: f, values, onChange }) => {
  const kind = getInputKind(f.typeName);
  const enumDef = getEnumDef(f.typeName);
  const raw = values[f.name];
  const current = raw ?? f.defaultValue ?? "";
  const hint = FIELD_DESCRIPTIONS[f.name];

  return (
    <div className="flex flex-col gap-1">
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
            <option key={name} value={opt.value}>{opt.label}</option>
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

      {hint && (
        <p className="text-[10px] text-gray-500 dark:text-gray-400">{hint}</p>
      )}
    </div>
  );
};

/* ─── Main component ─── */
export const StrategyConfigEditor: React.FC<Props> = ({
  strategyKey,
  fields,
  values,
  onChange,
}) => {
  // استراتژی رو از StrategyKey توی values تشخیص می‌ده (اگه بود)
  const groups = useMemo(() => STRATEGY_GROUPS[strategyKey] ?? null, [strategyKey]);
  // const groups = useMemo<FieldGroup[] | null>(() => {
  //   const key = values["StrategyKey"] ?? null;
  //   if (!key) return null;
  //   return STRATEGY_GROUPS[key] ?? null;
  // }, [values]);

  if (fields.length === 0) {
    return (
      <p className="rounded bg-slate-100 p-3 text-xs text-slate-600 dark:bg-slate-700 dark:text-slate-300">
        این استراتژی پارامتر قابل تنظیمی ندارد
      </p>
    );
  }

  // ─── Flat mode (بدون گروه‌بندی) ───
  if (!groups) {
    return (
      <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
        {fields.map((f) => (
          <FieldRenderer
            key={f.name}
            field={f}
            values={values}
            onChange={onChange}
          />
        ))}
      </div>
    );
  }

  // ─── Grouped mode ───
  const fieldsByName = new Map(fields.map((f) => [f.name, f]));
  const assigned = new Set<string>();

  return (
    <div className="space-y-4">
      {groups.map((g) => {
        const groupFields = g.fieldNames
          .map((n) => {
            const f = fieldsByName.get(n);
            if (f) assigned.add(n);
            return f;
          })
          .filter((f): f is StrategyConfigFieldView => f !== undefined);

        if (groupFields.length === 0) return null;

        return (
          <div
            key={g.title}
            className="rounded-lg border border-gray-200 p-3 dark:border-gray-700"
          >
            <div className="mb-2">
              <h5 className="text-xs font-bold text-gray-800 dark:text-gray-200">
                {g.title}
              </h5>
              {g.description && (
                <p className="mt-0.5 text-[10px] text-gray-500">
                  {g.description}
                </p>
              )}
            </div>
            <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
              {groupFields.map((f) => (
                <FieldRenderer
                  key={f.name}
                  field={f}
                  values={values}
                  onChange={onChange}
                />
              ))}
            </div>
          </div>
        );
      })}

      {/* فیلدهای گروه‌بندی‌نشده — اگه بود */}
      {fields.some((f) => !assigned.has(f.name)) && (
        <div className="rounded-lg border border-dashed border-amber-500 p-3">
          <h5 className="mb-2 text-xs font-bold text-amber-600">
            پارامترهای دیگر
          </h5>
          <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
            {fields
              .filter((f) => !assigned.has(f.name))
              .map((f) => (
                <FieldRenderer
                  key={f.name}
                  field={f}
                  values={values}
                  onChange={onChange}
                />
              ))}
          </div>
        </div>
      )}
    </div>
  );
};