import React, { useEffect, useState } from "react";
import { instrumentApi } from "../../../api/instrumentApi";
import type { TradingRunSymbolModel } from "../../../models";
import type { SelectionListDto } from "@/core/models/SelectionListDto";

interface Props {
  symbols: TradingRunSymbolModel[];
  onChange: (symbols: TradingRunSymbolModel[]) => void;
}

export const SymbolAllocationEditor: React.FC<Props> = ({
  symbols,
  onChange,
}) => {
  const [options, setOptions] = useState<SelectionListDto[]>([]);
  const [selectedIsin, setSelectedIsin] = useState<string>("");
  const [percentage, setPercentage] = useState<string>("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        const list = await instrumentApi.GetSelectionList();
        setOptions(list);
      } catch {
        setOptions([]);
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  const add = () => {
    if (!selectedIsin) return;
    const pct = Number(percentage);
    if (isNaN(pct) || pct <= 0) {
      alert("درصد باید بزرگ‌تر از صفر باشد");
      return;
    }
    if (symbols.some((s) => s.isin === selectedIsin)) {
      alert("این نماد قبلاً اضافه شده است");
      return;
    }
    onChange([...symbols, { isin: selectedIsin, allocationPercentage: pct }]);
    setSelectedIsin("");
    setPercentage("");
  };

  const remove = (isin: string) => {
    onChange(symbols.filter((s) => s.isin !== isin));
  };

  const update = (isin: string, pct: number) => {
    onChange(
      symbols.map((s) =>
        s.isin === isin ? { ...s, allocationPercentage: pct } : s,
      ),
    );
  };

  const labelFor = (isin: string): string => {
    const opt = options.find((o) => o.value === isin);
    return opt ? opt.display || opt.label : isin;
  };

  const total = symbols.reduce((sum, s) => sum + s.allocationPercentage, 0);
  const totalOk = Math.abs(total - 100) < 0.01;

  const availableOptions = options.filter(
    (o) => !symbols.some((s) => s.isin === o.value),
  );

  return (
    <div className="space-y-3">
      {/* ─── افزودن ─── */}
      <div className="flex flex-col gap-2 sm:flex-row">
        <select
          value={selectedIsin}
          onChange={(e) => setSelectedIsin(e.target.value)}
          disabled={loading}
          className="flex-1 rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        >
          <option value="">
            {loading ? "در حال بارگذاری..." : "انتخاب نماد..."}
          </option>
          {availableOptions.map((o) => (
            <option key={o.value} value={o.value}>
              {o.display || o.label}
            </option>
          ))}
        </select>
        <input
          type="number"
          placeholder="درصد"
          value={percentage}
          onChange={(e) => setPercentage(e.target.value)}
          min={0}
          max={100}
          step="0.01"
          dir="ltr"
          className="w-full rounded border border-gray-300 p-2 text-sm sm:w-24 dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        />
        <button
          type="button"
          onClick={add}
          disabled={!selectedIsin || !percentage}
          className="rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
        >
          افزودن
        </button>
      </div>

      {/* ─── لیست ─── */}
      {symbols.length === 0 ? (
        <p className="rounded border border-dashed border-gray-300 p-4 text-center text-xs text-gray-500 dark:border-gray-600">
          هیچ نمادی اضافه نشده است
        </p>
      ) : (
        <div className="overflow-hidden rounded border border-gray-200 dark:border-gray-700">
          <div className="flex items-center justify-between bg-slate-100 px-3 py-2 text-xs font-medium dark:bg-slate-700">
            <span>جمع تخصیص:</span>
            <span className={totalOk ? "text-emerald-600" : "text-red-600"}>
              {total.toFixed(2)}%{" "}
              {totalOk ? "✅" : "⚠️ باید ۱۰۰٪ باشد"}
            </span>
          </div>
          <div className="divide-y divide-gray-200 dark:divide-gray-700">
            {symbols.map((s) => (
              <div key={s.isin} className="flex items-center gap-2 p-2">
                <span className="flex-1 text-sm">
                  {labelFor(s.isin)}
                  <span className="mr-1 font-mono text-[10px] text-gray-400">
                    ({s.isin})
                  </span>
                </span>
                <input
                  type="number"
                  value={s.allocationPercentage}
                  onChange={(e) => update(s.isin, Number(e.target.value))}
                  min={0}
                  max={100}
                  step="0.01"
                  dir="ltr"
                  className="w-24 rounded border border-gray-300 p-1 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                />
                <span className="text-xs text-gray-500">%</span>
                <button
                  type="button"
                  onClick={() => remove(s.isin)}
                  className="rounded p-1 text-xs text-red-600 hover:bg-red-50 dark:hover:bg-red-900/30"
                >
                  حذف
                </button>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};