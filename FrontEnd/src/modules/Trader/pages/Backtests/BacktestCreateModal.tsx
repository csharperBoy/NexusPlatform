// src/modules/Trader/pages/Backtests/BacktestCreateModal.tsx
import React, { useEffect, useState } from "react";
import { tradingRunApi } from "../../api/tradingRunApi";
import { backtestApi } from "../../api/backtestApi";
import { strategyApi } from "../../api/strategyApi";
import { currencyRateApi } from "../../api/currencyRateApi";
import type {
  TradingRunInfoView,
  RunBacktestModel,
  RunBacktestConfigModel,
  TradingRunSymbolModel,
  StrategyInfoView,
} from "../../models";
import { TimeFrame } from "../../models";
import { SymbolAllocationEditor } from "../TradingRuns/components/SymbolAllocationEditor";
import { StrategyConfigSection } from "../../components/StrategyConfigSection";
import { buildDefaultStrategyConfig } from "../../utils/strategyDefaults";

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
  const [strategies, setStrategies] = useState<StrategyInfoView[]>([]);

  const [name, setName] = useState("");
  const [sourceRunId, setSourceRunId] = useState<string>(
    initialSourceRunId ?? "",
  );
  const [dateFrom, setDateFrom] = useState(monthAgoISO());
  const [dateTo, setDateTo] = useState(todayISO());
  const [initialCapital, setInitialCapital] = useState("10000000");
  const [slippage, setSlippage] = useState("0.1"); // percent
  const [subCandleTf, setSubCandleTf] = useState("1");
  const [targetCurrency, setTargetCurrency] = useState("");
  const [currencyRateStatus, setCurrencyRateStatus] =
    useState<"idle" | "checking" | "ok" | "late-start" | "missing">("idle");

  // Virtual config
  const [strategyKey, setStrategyKey] = useState("");
  const [strategyConfig, setStrategyConfig] = useState<Record<string, unknown>>({});
  const [primaryTimeFrame, setPrimaryTimeFrame] = useState<number>(TimeFrame.M5);
  const [strengthMultiplier, setStrengthMultiplier] = useState("1.5");
  const [minDeltaPercent, setMinDeltaPercent] = useState("3");
  const [stopLossPct, setStopLossPct] = useState("20");
  const [takeProfitPct, setTakeProfitPct] = useState("50");
  const [symbols, setSymbols] = useState<TradingRunSymbolModel[]>([]);

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /* ─── Fetch trading runs ─── */
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

  /* ─── Fetch strategies + auto-select first ─── */
  useEffect(() => {
    if (!isOpen) return;
    (async () => {
      try {
        const list = await strategyApi.GetList();
        setStrategies(list);
        if (list.length > 0 && !strategyKey) {
          setStrategyKey(list[0].key);
          setStrategyConfig(buildDefaultStrategyConfig(list[0].configFields));
        }
      } catch {
        setStrategies([]);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isOpen]);

  /* ─── Reset on open ─── */
  useEffect(() => {
    if (isOpen) {
      setName("");
      setTargetCurrency("");
      setCurrencyRateStatus("idle");
      setError(null);
    }
  }, [isOpen]);

  /* ─── Strategy change handler ─── */
  const handleStrategyKeyChange = (k: string) => {
    setStrategyKey(k);
    const s = strategies.find((x) => x.key === k);
    if (s) setStrategyConfig(buildDefaultStrategyConfig(s.configFields));
    else setStrategyConfig({});
  };

  /* ─── Currency rate availability check ─── */
  useEffect(() => {
    if (!isOpen) return;

    const cur = targetCurrency.trim().toUpperCase();
    if (!cur || cur === "IRR") {
      setCurrencyRateStatus("idle");
      return;
    }

    let cancelled = false;
    setCurrencyRateStatus("checking");

    (async () => {
      try {
        const hasBaseline = await currencyRateApi.CheckRange(
          cur, "IRR", "1900-01-01", dateFrom,
        );
        if (cancelled) return;
        if (hasBaseline) {
          setCurrencyRateStatus("ok");
          return;
        }
        const hasAny = await currencyRateApi.CheckRange(
          cur, "IRR", "1900-01-01", dateTo,
        );
        if (cancelled) return;
        setCurrencyRateStatus(hasAny ? "late-start" : "missing");
      } catch {
        if (!cancelled) setCurrencyRateStatus("missing");
      }
    })();

    return () => { cancelled = true; };
  }, [targetCurrency, isOpen, dateFrom, dateTo]);

  /* ─── Build virtual config ─── */
  const buildVirtualConfig = (): RunBacktestConfigModel => ({
    strategyKey,
    strategyConfigJson: JSON.stringify(strategyConfig),
    primaryTimeFrame,
    targetCurrency: targetCurrency.trim().toUpperCase() || null,
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
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!name.trim()) { setError("نام بکتست الزامی است."); return; }
    if (new Date(dateTo) <= new Date(dateFrom)) {
      setError("تاریخ پایان باید بعد از تاریخ شروع باشد."); return;
    }
    const ic = Number(initialCapital);
    if (!ic || ic <= 0) {
      setError("سرمایه‌ی اولیه باید بزرگ‌تر از صفر باشد."); return;
    }
    if (mode === "existing" && !sourceRunId) {
      setError("یک Run انتخاب کنید."); return;
    }
    if (mode === "virtual") {
      if (!strategyKey) { setError("یک استراتژی انتخاب کنید."); return; }
      if (symbols.length === 0) {
        setError("حداقل یک نماد انتخاب کنید."); return;
      }
      const total = symbols.reduce((s, x) => s + x.allocationPercentage, 0);
      if (Math.abs(total - 100) > 0.01) {
        setError("مجموع درصد تخصیص نمادها باید ۱۰۰ باشد."); return;
      }
    }
    if (targetCurrency && currencyRateStatus === "missing") {
      setError(
        "نرخ ارز برای بازه‌ی انتخابی موجود نیست. ابتدا از صفحه‌ی «نرخ‌های ارز» ثبت کنید.",
      );
      return;
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

              {strategies.length === 0 ? (
                <div className="rounded bg-slate-100 p-3 text-xs text-slate-600 dark:bg-slate-700 dark:text-slate-300">
                  در حال بارگذاری استراتژی‌ها...
                </div>
              ) : (
                <StrategyConfigSection
                  strategies={strategies}
                  strategyKey={strategyKey}
                  strategyConfig={strategyConfig}
                  primaryTimeFrame={primaryTimeFrame}
                  showPrimaryTimeFrame={true}
                  onStrategyKeyChange={handleStrategyKeyChange}
                  onConfigChange={setStrategyConfig}
                  onPrimaryTimeFrameChange={setPrimaryTimeFrame}
                  strategyHint={
                    strategyKey === "mtf-composite"
                      ? "این استراتژی به کندل‌های D1 (برای Trend) و M5 (برای Intra-Candle) نیاز دارد — خودکار لود می‌شوند."
                      : undefined
                  }
                />
              )}

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
                    ارز تحلیل (اختیاری)
                  </label>
                  <div className="flex items-center gap-2">
                    <select
                      value={targetCurrency}
                      onChange={(e) => setTargetCurrency(e.target.value)}
                      className="w-full rounded border border-gray-300 p-2 text-xs dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                    >
                      <option value="">IRR (پایه)</option>
                      <option value="USD">دلار آمریکا (USD)</option>
                      <option value="EUR">یورو (EUR)</option>
                      <option value="GBP">پوند انگلیس (GBP)</option>
                      <option value="AED">درهم امارات (AED)</option>
                      <option value="TRY">لیر ترکیه (TRY)</option>
                      <option value="CNY">یوان چین (CNY)</option>
                    </select>
                    {currencyRateStatus === "checking" && (
                      <span className="whitespace-nowrap text-xs text-gray-500">⏳</span>
                    )}
                    {currencyRateStatus === "ok" && (
                      <span className="whitespace-nowrap text-xs text-emerald-600">✓</span>
                    )}
                    {currencyRateStatus === "late-start" && (
                      <span
                        className="whitespace-nowrap text-xs text-amber-600"
                        title="نرخ‌ها بعد از تاریخ شروع بازه ثبت شده‌اند. کندل‌های اولیه با اولین نرخ تخمین زده می‌شوند."
                      >
                        ⚠️
                      </span>
                    )}
                    {currencyRateStatus === "missing" && (
                      <span className="whitespace-nowrap text-xs text-red-600">❌</span>
                    )}
                  </div>
                  <p className="mt-1 text-[10px] text-gray-500">
                    برای ارز غیر از IRR، باید حداقل یک نرخ قبل از شروع بازه ثبت شده باشد.
                  </p>
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
                <SymbolAllocationEditor symbols={symbols} onChange={setSymbols} />
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

          {/* Capital + Slippage + Sub-Candle */}
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
                دقت شبیه‌سازی SL/TP
              </label>
              <select
                value={subCandleTf}
                onChange={(e) => setSubCandleTf(e.target.value)}
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              >
                <option value="">Pessimistic (بدون Sub)</option>
                <option value={TimeFrame.M1}>M1</option>
                <option value={TimeFrame.M5}>M5</option>
                <option value={TimeFrame.M15}>M15</option>
              </select>
              <p className="mt-1 text-[10px] text-gray-500">
                فقط برای رفع ابهام SL/TP داخل کندل. Sub-Candle استراتژی جداست.
              </p>
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