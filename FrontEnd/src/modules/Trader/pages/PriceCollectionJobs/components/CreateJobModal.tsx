import React, { useEffect, useState } from "react";
import type {
  CreatePriceCollectionJobCommand,
  PriceSourceView,
} from "../../../models";
import {
  PriceCollectionMode,
  PRICE_COLLECTION_MODE_LABELS,
  RecurrenceType,
  RECURRENCE_TYPE_LABELS,
  buildRecurrencePattern,
} from "../../../models";

interface Props {
  isOpen: boolean;
  sources: PriceSourceView[];
  onClose: () => void;
  onSubmit: (cmd: CreatePriceCollectionJobCommand) => Promise<void>;
  saving: boolean;
}

const PHASE1_PATTERN_TYPES: RecurrenceType[] = [
  RecurrenceType.Minutely,
  RecurrenceType.Hourly,
  RecurrenceType.Daily,
  RecurrenceType.Cron,
];

export const CreateJobModal: React.FC<Props> = ({
  isOpen,
  sources,
  onClose,
  onSubmit,
  saving,
}) => {
  const [priceSourceId, setPriceSourceId] = useState("");
  const [mode, setMode] = useState<PriceCollectionMode>(PriceCollectionMode.Recurring);
  const [patternType, setPatternType] = useState<RecurrenceType>(RecurrenceType.Hourly);
  const [interval, setInterval] = useState("1");
  const [cronExpression, setCronExpression] = useState("0 * * * *");
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) return;
    setPriceSourceId(sources[0]?.id ?? "");
    setMode(PriceCollectionMode.Recurring);
    setPatternType(RecurrenceType.Hourly);
    setInterval("1");
    setCronExpression("0 * * * *");
    setError(null);
  }, [isOpen, sources]);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!priceSourceId) {
      setError("منبع قیمت را انتخاب کنید");
      return;
    }

    if (mode === PriceCollectionMode.Recurring) {
      if (patternType === RecurrenceType.Cron && !cronExpression.trim()) {
        setError("Cron expression الزامی است");
        return;
      }
      const intervalNum = parseInt(interval, 10);
      if (
        patternType !== RecurrenceType.Cron &&
        (isNaN(intervalNum) || intervalNum <= 0)
      ) {
        setError("فاصله باید بزرگ‌تر از صفر باشد");
        return;
      }
    }

    const cmd: CreatePriceCollectionJobCommand = {
      priceSourceId,
      mode,
      recurrencePatternJson:
        mode === PriceCollectionMode.Recurring
          ? buildRecurrencePattern(
              patternType,
              parseInt(interval, 10) || 1,
              cronExpression.trim() || null,
            )
          : null,
    };

    try {
      setError(null);
      await onSubmit(cmd);
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطای نامشخص");
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/50 p-4">
      <div className="my-8 w-full max-w-2xl rounded-xl bg-white shadow-xl dark:bg-gray-800">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4 dark:border-gray-700">
          <h3 className="text-lg font-bold text-gray-900 dark:text-white">
            ساخت Job جمع‌آوری نرخ
          </h3>
          <button
            type="button"
            onClick={onClose}
            className="text-2xl leading-none text-gray-500 hover:text-gray-700 dark:text-gray-400"
          >
            ×
          </button>
        </div>

        {/* Body */}
        <form onSubmit={handleSubmit} className="space-y-4 p-6">
          {error && (
            <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
              ⚠️ {error}
            </div>
          )}

          {sources.length === 0 && (
            <div className="rounded-lg bg-amber-50 p-3 text-sm text-amber-700 dark:bg-amber-900/30 dark:text-amber-400">
              ⚠️ هیچ منبع فعالی وجود ندارد. ابتدا یک منبع در صفحه‌ی «منابع قیمت» ایجاد کنید.
            </div>
          )}

          {/* ۱. Source */}
          <div className="flex flex-col gap-1">
            <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
              منبع قیمت <span className="text-red-500">*</span>
            </label>
            <select
              value={priceSourceId}
              onChange={(e) => setPriceSourceId(e.target.value)}
              disabled={sources.length === 0}
              className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            >
              {sources.length === 0 ? (
                <option value="">—</option>
              ) : (
                sources.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.name} ({s.code} — {s.fromAsset}/{s.toAsset})
                  </option>
                ))
              )}
            </select>
          </div>

          {/* ۲. Mode */}
          <div className="space-y-2">
            <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
              حالت اجرا
            </label>
            {[PriceCollectionMode.Recurring, PriceCollectionMode.Backfill].map(
              (m) => {
                const isSelected = mode === m;
                return (
                  <label
                    key={m}
                    className={`flex cursor-pointer items-start gap-3 rounded-lg border p-3 ${
                      isSelected
                        ? "border-blue-500 bg-blue-50 dark:bg-blue-900/20"
                        : "border-gray-200 dark:border-gray-700"
                    }`}
                  >
                    <input
                      type="radio"
                      checked={isSelected}
                      onChange={() => setMode(m)}
                      className="mt-1"
                    />
                    <div>
                      <p className="text-sm font-medium text-gray-900 dark:text-white">
                        {PRICE_COLLECTION_MODE_LABELS[m]}
                      </p>
                      <p className="mt-0.5 text-xs text-gray-500">
                        {m === PriceCollectionMode.Backfill
                          ? "یک‌بار اجرا — از قدیمی‌ترین رکورد تا max موجود در DB. بعد از این، تکرار نمی‌شود."
                          : "اجرای تکرارشونده — در هر tick فقط جدیدترین رکوردها را Sync می‌کند. Job در Running می‌ماند تا Stop کنید."}
                      </p>
                    </div>
                  </label>
                );
              },
            )}
          </div>

          {/* ۳. Recurrence (فقط Recurring) */}
          {mode === PriceCollectionMode.Recurring && (
            <div className="space-y-3 rounded-lg border border-gray-200 p-4 dark:border-gray-700">
              <div className="flex flex-col gap-1">
                <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                  نوع تکرار
                </label>
                <select
                  value={patternType}
                  onChange={(e) => setPatternType(Number(e.target.value))}
                  className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                >
                  {PHASE1_PATTERN_TYPES.map((t) => (
                    <option key={t} value={t}>
                      {RECURRENCE_TYPE_LABELS[t]}
                    </option>
                  ))}
                </select>
              </div>

              {patternType !== RecurrenceType.Cron && (
                <div className="flex flex-col gap-1">
                  <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                    فاصله
                  </label>
                  <div className="flex items-center gap-2">
                    <input
                      type="number"
                      value={interval}
                      onChange={(e) => setInterval(e.target.value)}
                      dir="ltr"
                      min={1}
                      step={1}
                      className="w-32 rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                    />
                    <span className="text-xs text-gray-500">
                      {patternType === RecurrenceType.Minutely && "دقیقه"}
                      {patternType === RecurrenceType.Hourly && "ساعت"}
                      {patternType === RecurrenceType.Daily && "روز"}
                    </span>
                  </div>
                </div>
              )}

              {patternType === RecurrenceType.Cron && (
                <div className="flex flex-col gap-1">
                  <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                    Cron Expression
                  </label>
                  <input
                    type="text"
                    value={cronExpression}
                    onChange={(e) => setCronExpression(e.target.value)}
                    dir="ltr"
                    placeholder="0 * * * *"
                    className="w-full rounded border border-gray-300 p-2 font-mono text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  />
                  <p className="text-[10px] text-gray-500">
                    ۵ فیلد استاندارد: دقیقه ساعت روز ماه روزهفته. مثال: <code className="font-mono">0 * * * *</code> = سر هر ساعت.
                  </p>
                </div>
              )}

              <div className="rounded bg-slate-100 p-2 text-[11px] text-slate-600 dark:bg-slate-700 dark:text-slate-300">
                ℹ️ اولین اجرا حدود ۱ دقیقه بعد از Start. برای Backfill نیازی به این تنظیم نیست.
              </div>
            </div>
          )}

          {/* Footer */}
          <div className="flex justify-end gap-2 border-t border-gray-200 pt-4 dark:border-gray-700">
            <button
              type="button"
              onClick={onClose}
              className="rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
            >
              انصراف
            </button>
            <button
              type="submit"
              disabled={saving || sources.length === 0}
              className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {saving ? "در حال ساخت..." : "ایجاد Job"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};