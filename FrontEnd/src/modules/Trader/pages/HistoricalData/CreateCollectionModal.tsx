import React, { useEffect, useState } from "react";
import { instrumentApi } from "../../api/instrumentApi";
import { historicalDataCollectionApi } from "../../api/historicalDataCollectionApi";
import type { SelectionListDto } from "@/core/models/SelectionListDto";
import { COLLECTION_TIMEFRAME_OPTIONS } from "../../models";

interface Props {
  isOpen: boolean;
  onClose: () => void;
  onCreated: () => void;
}

const todayISO = (): string => new Date().toISOString().slice(0, 10);
const yearAgoISO = (): string => {
  const d = new Date();
  d.setFullYear(d.getFullYear() - 1);
  return d.toISOString().slice(0, 10);
};

export const CreateCollectionModal: React.FC<Props> = ({
  isOpen,
  onClose,
  onCreated,
}) => {
  const [name, setName] = useState("");
  const [dateFrom, setDateFrom] = useState(yearAgoISO());
  const [dateTo, setDateTo] = useState(todayISO());

  const [instruments, setInstruments] = useState<SelectionListDto[]>([]);
  const [selectedIsins, setSelectedIsins] = useState<Set<string>>(new Set());
  const [search, setSearch] = useState("");
  const [selectAll, setSelectAll] = useState(false);

  const [selectedTfs, setSelectedTfs] = useState<Set<number>>(
    new Set([5, 1]), // پیش‌فرض: M5 و M1
  );

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) return;
    (async () => {
      try {
        const list = await instrumentApi.GetSelectionList();
        setInstruments(list);
      } catch {
        setInstruments([]);
      }
    })();
  }, [isOpen]);

  useEffect(() => {
    if (isOpen) {
      setName("");
      setSelectedIsins(new Set());
      setSearch("");
      setSelectAll(false);
      setError(null);
    }
  }, [isOpen]);

  const filteredInstruments = instruments.filter((i) => {
    if (!search.trim()) return true;
    const q = search.toLowerCase();
    return (
      i.label.toLowerCase().includes(q) ||
      i.value.toLowerCase().includes(q) ||
      (i.display && i.display.toLowerCase().includes(q))
    );
  });

  const toggleIsin = (isin: string) => {
    setSelectedIsins((prev) => {
      const next = new Set(prev);
      if (next.has(isin)) next.delete(isin);
      else next.add(isin);
      return next;
    });
  };

  const handleSelectAll = (checked: boolean) => {
    setSelectAll(checked);
    if (checked) {
      setSelectedIsins(new Set(filteredInstruments.map((i) => i.value)));
    } else {
      setSelectedIsins(new Set());
    }
  };

  const toggleTf = (tf: number) => {
    setSelectedTfs((prev) => {
      const next = new Set(prev);
      if (next.has(tf)) next.delete(tf);
      else next.add(tf);
      return next;
    });
  };

  /* ─── Estimation ─── */
  const estimatedBatches = (() => {
    if (selectedIsins.size === 0 || selectedTfs.size === 0) return 0;

    const days = Math.max(
      1,
      Math.ceil(
        (new Date(dateTo).getTime() - new Date(dateFrom).getTime()) /
          (1000 * 60 * 60 * 24),
      ),
    );

    let batches = 0;
    for (const tf of selectedTfs) {
      // 240 دقیقه معاملاتی در روز
      const candlesPerDay = 240 / tf;
      const totalCandles = candlesPerDay * days;
      batches += Math.ceil(totalCandles / 500); // BatchSize=500
    }

    return batches * selectedIsins.size;
  })();

  const estimatedDurationMin = Math.ceil((estimatedBatches * 250) / 60000);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!name.trim()) {
      setError("نام Job الزامی است.");
      return;
    }
    if (selectedIsins.size === 0) {
      setError("حداقل یک نماد انتخاب کنید.");
      return;
    }
    if (selectedTfs.size === 0) {
      setError("حداقل یک تایم‌فریم انتخاب کنید.");
      return;
    }
    if (new Date(dateTo) <= new Date(dateFrom)) {
      setError("تاریخ پایان باید بعد از تاریخ شروع باشد.");
      return;
    }

    setSubmitting(true);
    try {
      await historicalDataCollectionApi.Collect({
        name: name.trim(),
        isins: Array.from(selectedIsins),
        timeFrames: Array.from(selectedTfs),
        dateFrom: new Date(dateFrom).toISOString(),
        dateTo: new Date(dateTo + "T23:59:59Z").toISOString(),
      });
      onCreated();
      onClose();
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در ساخت Job");
    } finally {
      setSubmitting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/50 p-4">
      <div className="my-8 w-full max-w-3xl rounded-xl bg-white shadow-xl dark:bg-gray-800">
        <div className="flex items-center justify-between border-b border-gray-200 px-5 py-3 dark:border-gray-700">
          <h3 className="text-lg font-bold text-gray-900 dark:text-white">
            جمع‌آوری داده‌ی تاریخی
          </h3>
          <button
            type="button"
            onClick={onClose}
            className="text-2xl text-gray-500 hover:text-gray-700"
          >
            ×
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4 p-5">
          {error && (
            <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
              ⚠️ {error}
            </div>
          )}

          {/* Name */}
          <div>
            <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
              نام Job *
            </label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="مثلاً: ۱۰ نماد پرگردش - ۱ سال"
              className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            />
          </div>

          {/* Date Range */}
          <div className="grid grid-cols-2 gap-2">
            <div>
              <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
                از تاریخ
              </label>
              <input
                type="date"
                value={dateFrom}
                onChange={(e) => setDateFrom(e.target.value)}
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            </div>
            <div>
              <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
                تا تاریخ
              </label>
              <input
                type="date"
                value={dateTo}
                onChange={(e) => setDateTo(e.target.value)}
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            </div>
          </div>

          {/* TimeFrames */}
          <div>
            <label className="mb-2 block text-xs font-medium text-gray-700 dark:text-gray-300">
              تایم‌فریم‌ها *
            </label>
            <div className="flex flex-wrap gap-2">
              {COLLECTION_TIMEFRAME_OPTIONS.map((tf) => {
                const selected = selectedTfs.has(tf.value);
                return (
                  <button
                    key={tf.value}
                    type="button"
                    onClick={() => toggleTf(tf.value)}
                    className={`rounded-lg border px-3 py-1.5 text-xs font-medium ${
                      selected
                        ? "border-blue-500 bg-blue-600 text-white"
                        : "border-gray-300 text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
                    }`}
                  >
                    {tf.label}
                  </button>
                );
              })}
            </div>
            <p className="mt-1 text-[10px] text-gray-500">
              M1 و M5 برای Backtest دقیق‌تر توصیه می‌شود.
            </p>
          </div>

          {/* Instruments */}
          <div>
            <div className="mb-2 flex items-center justify-between">
              <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                نمادها * ({selectedIsins.size} از {instruments.length})
              </label>
              <label className="flex items-center gap-1 text-xs text-gray-600 dark:text-gray-400">
                <input
                  type="checkbox"
                  checked={selectAll}
                  onChange={(e) => handleSelectAll(e.target.checked)}
                  className="rounded"
                />
                انتخاب همه
              </label>
            </div>

            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="جستجو..."
              className="mb-2 w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            />

            <div className="max-h-60 overflow-y-auto rounded border border-gray-300 p-2 dark:border-gray-600">
              {filteredInstruments.length === 0 ? (
                <p className="py-2 text-center text-xs text-gray-500">
                  نمادی یافت نشد.
                </p>
              ) : (
                <div className="grid grid-cols-2 gap-1">
                  {filteredInstruments.map((i) => {
                    const checked = selectedIsins.has(i.value);
                    return (
                      <label
                        key={i.value}
                        className={`flex cursor-pointer items-center gap-2 rounded p-1.5 text-xs ${
                          checked
                            ? "bg-blue-50 dark:bg-blue-900/20"
                            : "hover:bg-gray-50 dark:hover:bg-gray-700"
                        }`}
                      >
                        <input
                          type="checkbox"
                          checked={checked}
                          onChange={() => toggleIsin(i.value)}
                          className="rounded"
                        />
                        <span className="flex-1">
                          {i.display || i.label}
                          <span className="mr-1 font-mono text-[10px] text-gray-400">
                            ({i.value})
                          </span>
                        </span>
                      </label>
                    );
                  })}
                </div>
              )}
            </div>
          </div>

          {/* Estimation */}
          {estimatedBatches > 0 && (
            <div className="rounded-lg border border-blue-200 bg-blue-50 p-3 text-xs text-blue-700 dark:border-blue-800 dark:bg-blue-900/20 dark:text-blue-300">
              <p>
                <strong>تخمین:</strong>{" "}
                {estimatedBatches.toLocaleString("fa-IR")} batch ·~{" "}
                {estimatedDurationMin > 60
                  ? `${(estimatedDurationMin / 60).toFixed(1)} ساعت`
                  : `${estimatedDurationMin} دقیقه`}
              </p>
              <p className="mt-1 text-[10px] text-blue-600 dark:text-blue-400">
                این تخمین حدودی است. سرعت به پاسخ‌دهی کارگزاری بستگی دارد.
              </p>
            </div>
          )}

          <div className="flex justify-end gap-2 border-t border-gray-200 pt-3 dark:border-gray-700">
            <button
              type="button"
              onClick={onClose}
              className="rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300"
            >
              انصراف
            </button>
            <button
              type="submit"
              disabled={submitting}
              className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {submitting ? "در حال ایجاد..." : "شروع جمع‌آوری"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};