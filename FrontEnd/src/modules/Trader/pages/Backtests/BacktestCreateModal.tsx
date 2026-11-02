import React, { useEffect, useState } from "react";
import { tradingRunApi } from "../../api/tradingRunApi";
import { backtestApi } from "../../api/backtestApi";
import type {
  TradingRunInfoView,
  RunBacktestModel,
  RunBacktestConfigModel,
} from "../../models";
import { TimeFrame, TIMEFRAME_LABELS } from "../../models";
import { SymbolAllocationEditor } from "../TradingRuns/components/SymbolAllocationEditor";
import type { TradingRunSymbolModel } from "../../models";

interface Props {
  isOpen: boolean;
  initialSourceRunId?: string | null;
  onClose: () => void;
  onCreated: (backtestRunId: string) => void;
}

type Mode = "existing" | "virtual";

const todayISO = (): string => new Date().toISOString().slice(0, 10);
const monthAgoISO = (): string => {
  const d = new Date();
  d.setMonth(d.getMonth() - 1);
  return d.toISOString().slice(0, 10);
};

export const BacktestCreateModal: React.FC<Props> = ({
  isOpen,
  initialSourceRunId,
  onClose,
  onCreated,
}) => {
  const [mode, setMode] = useState<Mode>(
    initialSourceRunId ? "existing" : "virtual",
  );
  const [runs, setRuns] = useState<TradingRunInfoView[]>([]);

  const [name, setName] = useState("");
  const [sourceRunId, setSourceRunId] = useState<string>(
    initialSourceRunId ?? "",
  );
  const [dateFrom, setDateFrom] = useState(monthAgoISO());
  const [dateTo, setDateTo] = useState(todayISO());
  const [initialCapital, setInitialCapital] = useState("10000000");
  const [slippage, setSlippage] = useState("0.1"); // percent
  const [subCandleTf, setSubCandleTf] = useState("1");

  // Virtual config
  const [strategyKey, setStrategyKey] = useState("rsi");
  const [strategyConfigJson, setStrategyConfigJson] = useState(
    '{"Period":14,"OversoldThreshold":30,"OverboughtThreshold":70,"MinSignalStrength":0.6}',
  );
  const [primaryTimeFrame, setPrimaryTimeFrame] = useState<number>(TimeFrame.M5);
  const [strengthMultiplier, setStrengthMultiplier] = useState("1.5");
  const [minDeltaPercent, setMinDeltaPercent] = useState("3");
  const [stopLossPct, setStopLossPct] = useState("20");
  const [takeProfitPct, setTakeProfitPct] = useState("50");
  const [symbols, setSymbols] = useState<TradingRunSymbolModel[]>([]);

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) return;
    (async () => {
      try {
        const list = await tradingRunApi.GetList();
        setRuns(list);
      } catch {
        setRuns([]);
      }
    })();
  }, [isOpen]);

  useEffect(() => {
    if (isOpen) {
      setName("");
      setError(null);
    }
  }, [isOpen]);

  const buildVirtualConfig = (): RunBacktestConfigModel => {
  return {
    strategyKey,
    strategyConfigJson,
    primaryTimeFrame,
    targetCurrency: null,
    strengthMultiplier: Number(strengthMultiplier),
    minDeltaPercent: Number(minDeltaPercent) / 100,
    stopLossPercentage: stopLossPct ? Number(stopLossPct) : null,
    stopLossPrice: null,
    takeProfitPercentage: takeProfitPct ? Number(takeProfitPct) : null,
    takeProfitPrice: null,
    defaultOrderMode: 0,
    onMarketClose: 0,
    onStop: 0,
    symbols: symbols.map((s) => ({
      isin: s.isin,
      allocationPercentage: s.allocationPercentage,
    })),
  };
};

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!name.trim()) {
      setError("نام بکتست الزامی است.");
      return;
    }
    if (new Date(dateTo) <= new Date(dateFrom)) {
      setError("تاریخ پایان باید بعد از تاریخ شروع باشد.");
      return;
    }
    const ic = Number(initialCapital);
    if (!ic || ic <= 0) {
      setError("سرمایه‌ی اولیه باید بزرگ‌تر از صفر باشد.");
      return;
    }

    if (mode === "existing" && !sourceRunId) {
      setError("یک Run انتخاب کنید.");
      return;
    }

    if (mode === "virtual") {
  if (symbols.length === 0) {
    setError("حداقل یک نماد انتخاب کنید.");
    return;
  }

  const total = symbols.reduce((s, x) => s + x.allocationPercentage, 0);
  if (Math.abs(total - 100) > 0.01) {
    setError("مجموع درصد تخصیص نمادها باید ۱۰۰ باشد.");
    return;
  }
}

    const model: RunBacktestModel = {
      name: name.trim(),
      sourceRunId: mode === "existing" ? sourceRunId : null,
      configOverride: mode === "virtual" ? buildVirtualConfig() : null,
      dateFrom: new Date(dateFrom).toISOString(),
      dateTo: new Date(dateTo + "T23:59:59Z").toISOString(),
      initialCapital: ic,
      slippagePercent: Number(slippage) / 100,
      subCandleTimeFrame: subCandleTf ? Number(subCandleTf) : null,
      timeZoneId: "Asia/Tehran",
    };

    setSubmitting(true);
    try {
      const id = await backtestApi.Run(model);
      onCreated(id);
      onClose();
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در اجرا");
    } finally {
      setSubmitting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/50 p-4">
      <div className="my-8 w-full max-w-2xl rounded-xl bg-white shadow-xl dark:bg-gray-800">
        <div className="flex items-center justify-between border-b border-gray-200 px-5 py-3 dark:border-gray-700">
          <h3 className="text-lg font-bold text-gray-900 dark:text-white">
            بکتست جدید
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

          {/* Mode toggle */}
          <div className="flex gap-1 border-b border-gray-200 dark:border-gray-700">
            <button
              type="button"
              onClick={() => setMode("existing")}
              className={`border-b-2 px-4 py-2 text-sm font-medium ${
                mode === "existing"
                  ? "border-blue-500 text-blue-600"
                  : "border-transparent text-gray-500"
              }`}
            >
              از Run موجود
            </button>
            <button
              type="button"
              onClick={() => setMode("virtual")}
              className={`border-b-2 px-4 py-2 text-sm font-medium ${
                mode === "virtual"
                  ? "border-blue-500 text-blue-600"
                  : "border-transparent text-gray-500"
              }`}
            >
              Config دلخواه
            </button>
          </div>

          {/* Name */}
          <div>
            <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
              نام بکتست *
            </label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="مثلاً: RSI M5 - 1 ماه"
              className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            />
          </div>

          {/* Source Run OR Virtual Config */}
          {mode === "existing" ? (
            <div>
              <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
                Trading Run *
              </label>
              <select
                value={sourceRunId}
                onChange={(e) => setSourceRunId(e.target.value)}
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              >
                <option value="">انتخاب کنید...</option>
                {runs.map((r) => (
                  <option key={r.id} value={r.id}>
                    {r.name} ({r.strategyKey})
                  </option>
                ))}
              </select>
            </div>
          ) : (
            <div className="space-y-3 rounded-lg border border-gray-200 p-3 dark:border-gray-700">
              <p className="text-xs font-bold text-gray-700 dark:text-gray-300">
                Config دلخواه
              </p>

              <div className="grid grid-cols-2 gap-2">
                <div>
                  <label className="mb-1 block text-xs text-gray-600 dark:text-gray-400">
                    استراتژی
                  </label>
                  <input
                    type="text"
                    value={strategyKey}
                    onChange={(e) => setStrategyKey(e.target.value)}
                    dir="ltr"
                    className="w-full rounded border border-gray-300 p-2 text-xs dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  />
                </div>
                <div>
                  <label className="mb-1 block text-xs text-gray-600 dark:text-gray-400">
                    تایم‌فریم
                  </label>
                  <select
                    value={primaryTimeFrame}
                    onChange={(e) => setPrimaryTimeFrame(Number(e.target.value))}
                    className="w-full rounded border border-gray-300 p-2 text-xs dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  >
                    {Object.entries(TIMEFRAME_LABELS).map(([k, v]) => (
                      <option key={k} value={k}>
                        {v}
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              <div>
                <label className="mb-1 block text-xs text-gray-600 dark:text-gray-400">
                  Config JSON
                </label>
                <textarea
                  value={strategyConfigJson}
                  onChange={(e) => setStrategyConfigJson(e.target.value)}
                  rows={3}
                  dir="ltr"
                  className="w-full rounded border border-gray-300 p-2 font-mono text-xs dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                />
              </div>

              <div className="grid grid-cols-2 gap-2">
                <div>
                  <label className="mb-1 block text-xs text-gray-600 dark:text-gray-400">
                    Strength Multiplier
                  </label>
                  <input
                    type="number"
                    value={strengthMultiplier}
                    onChange={(e) => setStrengthMultiplier(e.target.value)}
                    step={0.1}
                    dir="ltr"
                    className="w-full rounded border border-gray-300 p-2 text-xs dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  />
                </div>
                <div>
                  <label className="mb-1 block text-xs text-gray-600 dark:text-gray-400">
                    MinDelta (%)
                  </label>
                  <input
                    type="number"
                    value={minDeltaPercent}
                    onChange={(e) => setMinDeltaPercent(e.target.value)}
                    step={0.5}
                    dir="ltr"
                    className="w-full rounded border border-gray-300 p-2 text-xs dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  />
                </div>
                <div>
                  <label className="mb-1 block text-xs text-gray-600 dark:text-gray-400">
                    SL (%)
                  </label>
                  <input
                    type="number"
                    value={stopLossPct}
                    onChange={(e) => setStopLossPct(e.target.value)}
                    step={1}
                    dir="ltr"
                    className="w-full rounded border border-gray-300 p-2 text-xs dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  />
                </div>
                <div>
                  <label className="mb-1 block text-xs text-gray-600 dark:text-gray-400">
                    TP (%)
                  </label>
                  <input
                    type="number"
                    value={takeProfitPct}
                    onChange={(e) => setTakeProfitPct(e.target.value)}
                    step={1}
                    dir="ltr"
                    className="w-full rounded border border-gray-300 p-2 text-xs dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  />
                </div>
              </div>

              <div>
                <label className="mb-2 block text-xs text-gray-600 dark:text-gray-400">
                    نمادها و تخصیص سرمایه *
                </label>
                <SymbolAllocationEditor
                    symbols={symbols}
                    onChange={setSymbols}
                />
                </div>
            </div>
          )}

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

          {/* Capital + Slippage */}
          <div className="grid grid-cols-3 gap-2">
            <div>
              <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
                سرمایه اولیه (ریال)
              </label>
              <input
                type="number"
                value={initialCapital}
                onChange={(e) => setInitialCapital(e.target.value)}
                dir="ltr"
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            </div>
            <div>
              <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
                Slippage (%)
              </label>
              <input
                type="number"
                value={slippage}
                onChange={(e) => setSlippage(e.target.value)}
                step={0.05}
                dir="ltr"
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            </div>
            <div>
              <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
                Sub-Candle
              </label>
              <select
                value={subCandleTf}
                onChange={(e) => setSubCandleTf(e.target.value)}
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              >
                <option value="">Pessimistic</option>
                <option value={TimeFrame.M1}>M1</option>
                <option value={TimeFrame.M5}>M5</option>
                <option value={TimeFrame.M15}>M15</option>
              </select>
            </div>
          </div>

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
              {submitting ? "در حال ارسال..." : "شروع بکتست"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};