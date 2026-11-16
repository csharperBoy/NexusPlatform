// src/modules/Trader/components/StrategyConfigSection.tsx
import React, { useMemo } from "react";
import type { StrategyInfoView } from "../models";
import { TIMEFRAME_LABELS } from "../models";
import { StrategyConfigEditor } from "./StrategyConfigEditor";

interface Props {
  strategies: StrategyInfoView[];
  strategyKey: string;
  strategyConfig: Record<string, unknown>;
  primaryTimeFrame?: number;
  showPrimaryTimeFrame?: boolean;
  onStrategyKeyChange: (key: string) => void;
  onConfigChange: (config: Record<string, unknown>) => void;
  onPrimaryTimeFrameChange?: (tf: number) => void;
  disabled?: boolean;
  /** پیام hint مخصوص استراتژی (مثلاً MTF به ExtraTimeFrames نیاز دارد) */
  strategyHint?: string;
  /** پیش‌نمایش TFهای اضافی که خودکار لود می‌شن (مخصوص MTF) */
  showExtraTimeFramesPreview?: boolean;
}

export const StrategyConfigSection: React.FC<Props> = ({
  strategies,
  strategyKey,
  strategyConfig,
  primaryTimeFrame,
  showPrimaryTimeFrame = true,
  onStrategyKeyChange,
  onConfigChange,
  onPrimaryTimeFrameChange,
  disabled,
  strategyHint,
  showExtraTimeFramesPreview = true,
}) => {
  const selectedStrategy = strategies.find((s) => s.key === strategyKey);

  const extraTfsPreview = useMemo(() => {
    if (!showExtraTimeFramesPreview) return null;
    if (strategyKey !== "mtf-composite") return null;

    const trendRaw = strategyConfig["TrendTimeFrame"];
    const subRaw = strategyConfig["SubCandleTimeFrame"];
    const trendTf = typeof trendRaw === "number" ? trendRaw : 1440; // D1
    const subTf = typeof subRaw === "number" ? subRaw : 5;          // M5

    const labels: string[] = [];
    if (primaryTimeFrame !== undefined && trendTf !== primaryTimeFrame) {
      labels.push(
        TIMEFRAME_LABELS[trendTf as keyof typeof TIMEFRAME_LABELS] ?? String(trendTf),
      );
    }
    if (primaryTimeFrame !== undefined && subTf !== primaryTimeFrame) {
      labels.push(
        TIMEFRAME_LABELS[subTf as keyof typeof TIMEFRAME_LABELS] ?? String(subTf),
      );
    }

    if (labels.length === 0) return null;
    return labels.join(" + ");
  }, [strategyKey, strategyConfig, primaryTimeFrame, showExtraTimeFramesPreview]);

  return (
    <div className="space-y-3">
      <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
        <div className="flex flex-col gap-1">
          <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
            استراتژی <span className="text-red-500">*</span>
          </label>
          <select
            value={strategyKey}
            onChange={(e) => onStrategyKeyChange(e.target.value)}
            disabled={disabled}
            className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
          >
            <option value="">انتخاب کنید...</option>
            {strategies.map((s) => (
              <option key={s.key} value={s.key}>
                {s.displayName}
              </option>
            ))}
          </select>
        </div>

        {showPrimaryTimeFrame && onPrimaryTimeFrameChange && (
          <div className="flex flex-col gap-1">
            <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
              تایم‌فریم
            </label>
            <select
              value={primaryTimeFrame}
              onChange={(e) => onPrimaryTimeFrameChange(Number(e.target.value))}
              disabled={disabled}
              className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            >
              {Object.entries(TIMEFRAME_LABELS).map(([k, v]) => (
                <option key={k} value={k}>
                  {v}
                </option>
              ))}
            </select>
          </div>
        )}
      </div>

      {extraTfsPreview && (
        <div className="rounded border border-blue-300 bg-blue-50 p-2 text-[10px] text-blue-700 dark:border-blue-700 dark:bg-blue-900/20 dark:text-blue-300">
          ℹ️ کندل‌های اضافی که خودکار بارگذاری می‌شن:{" "}
          <strong>{extraTfsPreview}</strong>
        </div>
      )}

      {selectedStrategy && (
        <div>
          <p className="mb-2 text-xs font-semibold text-gray-600 dark:text-gray-400">
            پارامترهای {selectedStrategy.displayName}
          </p>
          <StrategyConfigEditor
            strategyKey={strategyKey}
            fields={selectedStrategy.configFields}
            values={strategyConfig}
            onChange={(n, v) => onConfigChange({ ...strategyConfig, [n]: v })}
          />
        </div>
      )}

      {strategyHint && (
        <div className="rounded-lg border border-blue-700 bg-blue-900/20 p-3 text-[10px] text-blue-300">
          ℹ️ {strategyHint}
        </div>
      )}
    </div>
  );
};