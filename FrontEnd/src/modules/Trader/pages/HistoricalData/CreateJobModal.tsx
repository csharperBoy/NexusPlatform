import React, { useEffect, useMemo, useState } from "react";
import { instrumentApi } from "../../api/instrumentApi";
import {
  COLLECTION_TIMEFRAME_OPTIONS,
  HistoricalCollectionMode,
  toEndOfDayUtc,
  toStartOfDayUtc,
  type CreateHistoricalBackfillCommand,
  type CreateHistoricalRecurringCommand,
} from "../../models";
import {
  RecurrenceType,
  RECURRENCE_TYPE_LABELS,
  buildRecurrencePattern,
} from "../../models/PriceCollection";
import type { SelectionListDto } from "@/core/models/SelectionListDto";

type Mode = HistoricalCollectionMode;

const PHASE1_PATTERN_TYPES: RecurrenceType[] = [
  RecurrenceType.Minutely,
  RecurrenceType.Hourly,
  RecurrenceType.Daily,
  RecurrenceType.Cron,
];

interface Props {
  isOpen: boolean;
  onClose: () => void;
  onSubmitBackfill: (cmd: CreateHistoricalBackfillCommand) => Promise<void>;
  onSubmitRecurring: (cmd: CreateHistoricalRecurringCommand) => Promise<void>;
  saving: boolean;
}

export const CreateJobModal: React.FC<Props> = ({
  isOpen,
  onClose,
  onSubmitBackfill,
  onSubmitRecurring,
  saving,
}) => {
  /* ─── Basic ─── */
  const [name, setName] = useState("");
  const [mode, setMode] = useState<Mode>(HistoricalCollectionMode.Backfill);

  /* ─── Universe ─── */
  const [isins, setIsins] = useState<string[]>([]);
  const [timeFrames, setTimeFrames] = useState<number[]>([]);

  /* ─── Range ─── */
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");

  /* ─── Recurrence (Recurring only) ─── */
  const [patternType, setPatternType] = useState<RecurrenceType>(
    RecurrenceType.Hourly,
  );
  const [interval, setInterval] = useState("1");
  const [cronExpression, setCronExpression] = useState("0 * * * *");

  /* ─── ISIN options ─── */
  const [isinOptions, setIsinOptions] = useState<SelectionListDto[]>([]);
  const [isinSearch, setIsinSearch] = useState("");
  const [loadingIsins, setLoadingIsins] = useState(false);

  const [error, setError] = useState<string | null>(null);

  /* ─── Reset on open ─── */
  useEffect(() => {
    if (!isOpen) return;
    setName("");
    setMode(HistoricalCollectionMode.Backfill);
    setIsins([]);
    setTimeFrames([1440]);
    setDateFrom(defaultDateFrom());
    setDateTo(defaultDateTo());
    setPatternType(RecurrenceType.Hourly);
    setInterval("1");
    setCronExpression("0 * * * *");
    setIsinSearch("");
    setError(null);
  }, [isOpen]);

  /* ─── Load ISIN options once ─── */
  useEffect(() => {
    if (!isOpen) return;
    setLoadingIsins(true);
    (async () => {
      try {
        const list = await instrumentApi.GetSelectionList();
        setIsinOptions(list);
      } catch {
        setIsinOptions([]);
      } finally {
        setLoadingIsins(false);
      }
    })();
  }, [isOpen]);

  /* ─── Filtered ISIN list ─── */
  const filteredIsins = useMemo(() => {
    const q = isinSearch.trim().toLowerCase();
    if (!q) return isinOptions;
    return isinOptions.filter(
      (o) =>
        o.label.toLowerCase().includes(q) ||
        o.value.toLowerCase().includes(q) ||
        (o.display ?? "").toLowerCase().includes(q),
    );
  }, [isinOptions, isinSearch]);

  if (!isOpen) return null;

  /* ═══════════ Submit ═══════════ */

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!name.trim()) {
      setError("نام Job الزامی است");
      return;
    }
    if (isins.length === 0) {
      setError("حداقل یک نماد انتخاب کنید");
      return;
    }
    if (timeFrames.length === 0) {
      setError("حداقل یک تایم‌فریم انتخاب کنید");
      return;
    }
    if (!dateFrom) {
      setError("تاریخ شروع الزامی است");
      return;
    }
    if (mode === HistoricalCollectionMode.Backfill && !dateTo) {
      setError("تاریخ پایان برای Backfill الزامی است");
      return;
    }
    if (dateTo && dateTo < dateFrom) {
      setError("تاریخ پایان باید بعد از تاریخ شروع باشد");
      return;
    }
    if (mode === HistoricalCollectionMode.Recurring) {
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

    try {
      setError(null);

      if (mode === HistoricalCollectionMode.Backfill) {
        await onSubmitBackfill({
          name: name.trim(),
          isins,
          timeFrames,
          dateFrom: toStartOfDayUtc(dateFrom),
          dateTo: toEndOfDayUtc(dateTo),
        });
      } else {
        const patternJson = buildRecurrencePattern(
          patternType,
          parseInt(interval, 10) || 1,
          cronExpression.trim() || null,
        );
        await onSubmitRecurring({
          name: name.trim(),
          isins,
          timeFrames,
          dateFrom: toStartOfDayUtc(dateFrom),
          recurrencePatternJson: patternJson,
        });
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطای نامشخص");
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/50 p-4">
      <div className="my-8 w-full max-w-3xl rounded-xl bg-white shadow-xl dark:bg-gray-800">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4 dark:border-gray-700">
          <h3 className="text-lg font-bold text-gray-900 dark:text-white">
            ساخت Job جمع‌آوری داده‌ی تاریخی
          </h3>
          <button
            type="button"
            onClick={onClose}
            className="text-2xl leading-none text-gray-500 hover:text-gray-700 dark:text-gray-400"
          >
            ×
          </button>
        </div>

        <form onSubmit={submit} className="space-y-4 p-6">
          {error && (
            <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
              ⚠️ {error}
            </div>
          )}

          {/* ۱. Name */}
          <div className="flex flex-col gap-1">
            <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
              نام Job <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="مثلاً: فولاد و شپنا — روزانه"
              className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            />
          </div>

          {/* ۲. Mode */}
          <div className="space-y-2">
            <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
              حالت اجرا
            </label>
            {[HistoricalCollectionMode.Backfill, HistoricalCollectionMode.Recurring].map(
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
                        {m === HistoricalCollectionMode.Backfill
                          ? "Backfill — یک‌بار"
                          : "تکرارشونده — Recurring"}
                      </p>
                      <p className="mt-0.5 text-xs text-gray-500">
                        {m === HistoricalCollectionMode.Backfill
                          ? "از تاریخ شروع تا پایان، یک‌بار همه‌ی کندل‌ها را fetch می‌کند و سپس Completed می‌شود."
                          : "در هر tick، از max(OpenTime) هر (نماد، تایم‌فریم) تا الان را fetch می‌کند. Job در Running می‌ماند تا Stop کنید."}
                      </p>
                    </div>
                  </label>
                );
              },
            )}
          </div>

          {/* ۳. ISINs */}
          <div className="space-y-2 rounded-lg border border-gray-200 p-4 dark:border-gray-700">
            <div className="flex items-center justify-between">
              <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                نمادها <span className="text-red-500">*</span>
              </label>
              <span className="text-xs text-gray-500">
                {isins.length} انتخاب شده
              </span>
            </div>

            {loadingIsins ? (
              <p className="text-xs text-gray-500">در حال بارگذاری...</p>
            ) : isinOptions.length === 0 ? (
              <p className="rounded border border-dashed border-amber-400 bg-amber-50 p-2 text-xs text-amber-700 dark:border-amber-500 dark:bg-amber-900/20 dark:text-amber-300">
                ⚠️ هیچ نمادی موجود نیست. ابتدا در صفحه‌ی «نمادها» نماد بسازید.
              </p>
            ) : (
              <>
                <input
                  type="text"
                  value={isinSearch}
                  onChange={(e) => setIsinSearch(e.target.value)}
                  placeholder="جستجو (نام یا ISIN)..."
                  className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                />
                <div className="max-h-48 overflow-y-auto rounded border border-gray-200 dark:border-gray-700">
                  {filteredIsins.length === 0 ? (
                    <p className="p-3 text-center text-xs text-gray-500">
                      نتیجه‌ای یافت نشد
                    </p>
                  ) : (
                    filteredIsins.map((opt) => {
                      const checked = isins.includes(opt.value);
                      return (
                        <label
                          key={opt.value}
                          className={`flex cursor-pointer items-center gap-2 border-b border-gray-100 px-3 py-2 text-sm last:border-0 hover:bg-gray-50 dark:border-gray-700 dark:hover:bg-gray-700/50 ${
                            checked ? "bg-blue-50 dark:bg-blue-900/20" : ""
                          }`}
                        >
                          <input
                            type="checkbox"
                            checked={checked}
                            onChange={() =>
                              setIsins((prev) =>
                                prev.includes(opt.value)
                                  ? prev.filter((x) => x !== opt.value)
                                  : [...prev, opt.value],
                              )
                            }
                          />
                          <span className="flex-1">{opt.display || opt.label}</span>
                          <span className="font-mono text-[10px] text-gray-400">
                            {opt.value}
                          </span>
                        </label>
                      );
                    })
                  )}
                </div>
                {isins.length > 0 && (
                  <button
                    type="button"
                    onClick={() => setIsins([])}
                    className="text-xs text-red-600 hover:underline"
                  >
                    پاک کردن همه
                  </button>
                )}
              </>
            )}
          </div>

          {/* ۴. TimeFrames */}
          <div className="space-y-2 rounded-lg border border-gray-200 p-4 dark:border-gray-700">
            <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
              تایم‌فریم‌ها <span className="text-red-500">*</span>
            </label>
            <div className="flex flex-wrap gap-2">
              {COLLECTION_TIMEFRAME_OPTIONS.map((tf) => {
                const checked = timeFrames.includes(tf.value);
                return (
                  <label
                    key={tf.value}
                    className={`flex cursor-pointer items-center gap-2 rounded-lg border px-3 py-1.5 text-xs ${
                      checked
                        ? "border-blue-500 bg-blue-50 text-blue-700 dark:bg-blue-900/20 dark:text-blue-300"
                        : "border-gray-300 text-gray-700 dark:border-gray-600 dark:text-gray-300"
                    }`}
                  >
                    <input
                      type="checkbox"
                      checked={checked}
                      onChange={() =>
                        setTimeFrames((prev) =>
                          prev.includes(tf.value)
                            ? prev.filter((x) => x !== tf.value)
                            : [...prev, tf.value],
                        )
                      }
                    />
                    {tf.label}
                  </label>
                );
              })}
            </div>
          </div>

          {/* ۵. Range */}
          <div className="grid grid-cols-1 gap-3 rounded-lg border border-gray-200 p-4 md:grid-cols-2 dark:border-gray-700">
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                تاریخ شروع <span className="text-red-500">*</span>
                {mode === HistoricalCollectionMode.Recurring && (
                  <span className="mr-1 text-[10px] text-gray-500">
                    (fallback اگر هیچ کندلی در DB نبود)
                  </span>
                )}
              </label>
              <input
                type="date"
                value={dateFrom}
                onChange={(e) => setDateFrom(e.target.value)}
                dir="ltr"
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            </div>

            {mode === HistoricalCollectionMode.Backfill && (
              <div className="flex flex-col gap-1">
                <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                  تاریخ پایان <span className="text-red-500">*</span>
                </label>
                <input
                  type="date"
                  value={dateTo}
                  onChange={(e) => setDateTo(e.target.value)}
                  dir="ltr"
                  className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                />
              </div>
            )}
          </div>

          {/* ۶. Recurrence */}
          {mode === HistoricalCollectionMode.Recurring && (
            <div className="space-y-3 rounded-lg border border-gray-200 p-4 dark:border-gray-700">
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

              {patternType !== RecurrenceType.Cron && (
                <div className="flex items-center gap-2">
                  <span className="text-xs text-gray-500">هر</span>
                  <input
                    type="number"
                    value={interval}
                    onChange={(e) => setInterval(e.target.value)}
                    dir="ltr"
                    min={1}
                    step={1}
                    className="w-24 rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  />
                  <span className="text-xs text-gray-500">
                    {patternType === RecurrenceType.Minutely && "دقیقه"}
                    {patternType === RecurrenceType.Hourly && "ساعت"}
                    {patternType === RecurrenceType.Daily && "روز"}
                  </span>
                </div>
              )}

              {patternType === RecurrenceType.Cron && (
                <>
                  <input
                    type="text"
                    value={cronExpression}
                    onChange={(e) => setCronExpression(e.target.value)}
                    dir="ltr"
                    placeholder="0 * * * *"
                    className="w-full rounded border border-gray-300 p-2 font-mono text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  />
                  <p className="text-[10px] text-gray-500">
                    ۵ فیلد: دقیقه ساعت روز ماه روزهفته. مثال:{" "}
                    <code className="font-mono">0 * * * *</code> = سر هر ساعت.
                  </p>
                </>
              )}

              <div className="rounded bg-slate-100 p-2 text-[11px] text-slate-600 dark:bg-slate-700 dark:text-slate-300">
                ℹ️ اولین اجرا ~۱ دقیقه بعد از Start. برای هر (نماد، تایم‌فریم)
                فقط کندل‌های جدیدتر از آخرین رکورد موجود fetch می‌شوند.
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
              disabled={saving}
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

/* ═══════════ Helpers ═══════════ */

function defaultDateFrom(): string {
  const d = new Date();
  d.setMonth(d.getMonth() - 6);
  return d.toISOString().slice(0, 10);
}

function defaultDateTo(): string {
  return new Date().toISOString().slice(0, 10);
}