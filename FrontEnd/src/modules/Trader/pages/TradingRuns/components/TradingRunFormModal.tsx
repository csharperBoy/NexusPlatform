import React, { useEffect, useState } from "react";
import type {
  CreateTradingRunCommand,
  UpdateTradingRunCommand,
  TradingRunDetailView,
  TradingRunSettingsModel,
  TradingRunSymbolModel,
  StrategyInfoView,
} from "../../../models";
import {
  TimeFrame,
  TIMEFRAME_LABELS,
  CapitalAllocationMode,
  CAPITAL_MODE_LABELS,
  OrderMode,
  ORDER_MODE_LABELS,
  MarketCloseBehavior,
  MARKET_CLOSE_LABELS,
  StopRunBehavior,
  STOP_BEHAVIOR_LABELS,
} from "../../../models";
import { StrategyConfigEditor } from "./StrategyConfigEditor";
import { SymbolAllocationEditor } from "./SymbolAllocationEditor";
import { AccountSelector } from "./AccountSelector";
import { currencyRateApi } from "@/modules/Trader/api";

type SlTpMode = "none" | "percent" | "price";

interface Props {
  isOpen: boolean;
  mode: "create" | "edit";
  initialDetail?: TradingRunDetailView | null;
  strategies: StrategyInfoView[];
  onClose: () => void;
  onSubmit: (
    cmd: CreateTradingRunCommand | UpdateTradingRunCommand,
  ) => Promise<void>;
  saving: boolean;
}

/* ─── Section wrapper ─── */
const Section: React.FC<{ title: string; children: React.ReactNode }> = ({
  title,
  children,
}) => (
  <div className="space-y-3 rounded-lg border border-gray-200 p-4 dark:border-gray-700">
    <h4 className="text-sm font-bold text-gray-800 dark:text-gray-200">
      {title}
    </h4>
    {children}
  </div>
);

export const TradingRunFormModal: React.FC<Props> = ({
  isOpen,
  mode,
  initialDetail,
  strategies,
  onClose,
  onSubmit,
  saving,
}) => {
  /* ─── State ─── */
  const [name, setName] = useState("");
  const [strategyKey, setStrategyKey] = useState("");
  const [strategyConfig, setStrategyConfig] = useState<Record<string, any>>({});
  const [primaryTimeFrame, setPrimaryTimeFrame] = useState<number>(
    TimeFrame.D1,
  );
const [targetCurrency, setTargetCurrency] = useState<string>("");
const [strengthMultiplier, setStrengthMultiplier] = useState<string>("1.5");
const [minDeltaPercent, setMinDeltaPercent] = useState<string>("3");   
const [currencyRateStatus, setCurrencyRateStatus] =
  useState<"idle" | "checking" | "ok" | "missing">("idle");
const [currencyRateValue, setCurrencyRateValue] = useState<number | null>(null);
  const [capitalAllocationMode, setCapitalAllocationMode] = useState<number>(
    CapitalAllocationMode.Fixed,
  );
  const [fixedCapitalAmount, setFixedCapitalAmount] = useState<string>("");

  const [slMode, setSlMode] = useState<SlTpMode>("none");
  const [slPercent, setSlPercent] = useState<string>("");
  const [slPrice, setSlPrice] = useState<string>("");

  const [tpMode, setTpMode] = useState<SlTpMode>("none");
  const [tpPercent, setTpPercent] = useState<string>("");
  const [tpPrice, setTpPrice] = useState<string>("");

  const [defaultOrderMode, setDefaultOrderMode] = useState<number>(
    OrderMode.Quantity,
  );
  const [onMarketClose, setOnMarketClose] = useState<number>(
    MarketCloseBehavior.KeepPositions,
  );
  const [onStop, setOnStop] = useState<number>(StopRunBehavior.KeepPositions);

  const [symbols, setSymbols] = useState<TradingRunSymbolModel[]>([]);
  const [accountIds, setAccountIds] = useState<string[]>([]);

  const [error, setError] = useState<string | null>(null);
useEffect(() => {
  if (!isOpen) return;

  const cur = targetCurrency.trim().toUpperCase();
  if (!cur || cur === "IRR") {
    setCurrencyRateStatus("idle");
    setCurrencyRateValue(null);
    return;
  }

  let cancelled = false;
  setCurrencyRateStatus("checking");
  setCurrencyRateValue(null);

  (async () => {
    try {
      const rate = await currencyRateApi.GetLatest(cur, "IRR");
      if (cancelled) return;
      if (rate != null && rate > 0) {
        setCurrencyRateStatus("ok");
        setCurrencyRateValue(rate);
      } else {
        // Try reverse direction (IRR→cur) in case only that exists
        const reverseRate = await currencyRateApi.GetLatest("IRR", cur);
        if (cancelled) return;
        if (reverseRate != null && reverseRate > 0) {
          setCurrencyRateStatus("ok");
          setCurrencyRateValue(1 / reverseRate);
        } else {
          setCurrencyRateStatus("missing");
        }
      }
    } catch {
      if (!cancelled) setCurrencyRateStatus("missing");
    }
  })();

  return () => {
    cancelled = true;
  };
}, [targetCurrency, isOpen]);

  /* ─── Init: Edit ─── */
  useEffect(() => {
    if (!isOpen) return;

    if (mode === "edit" && initialDetail) {
      setName(initialDetail.name);
      setStrategyKey(initialDetail.strategyKey);
      setPrimaryTimeFrame(initialDetail.primaryTimeFrame);
      setTargetCurrency(initialDetail.targetCurrency ?? "");
      setStrengthMultiplier(initialDetail.strengthMultiplier.toString());
setMinDeltaPercent((initialDetail.minDeltaPercent * 100).toString()); 
      setCapitalAllocationMode(initialDetail.settings.capitalAllocationMode);
      setFixedCapitalAmount(
        initialDetail.settings.fixedCapitalAmount?.toString() ?? "",
      );

      if (initialDetail.settings.stopLossPercentage != null) {
        setSlMode("percent");
        setSlPercent(
          initialDetail.settings.stopLossPercentage.toString(),
        );
        setSlPrice("");
      } else if (initialDetail.settings.stopLossPrice != null) {
        setSlMode("price");
        setSlPercent("");
        setSlPrice(initialDetail.settings.stopLossPrice.toString());
      } else {
        setSlMode("none");
        setSlPercent("");
        setSlPrice("");
      }

      if (initialDetail.settings.takeProfitPercentage != null) {
        setTpMode("percent");
        setTpPercent(
          initialDetail.settings.takeProfitPercentage.toString(),
        );
        setTpPrice("");
      } else if (initialDetail.settings.takeProfitPrice != null) {
        setTpMode("price");
        setTpPercent("");
        setTpPrice(initialDetail.settings.takeProfitPrice.toString());
      } else {
        setTpMode("none");
        setTpPercent("");
        setTpPrice("");
      }

      setDefaultOrderMode(initialDetail.settings.defaultOrderMode);
      setOnMarketClose(initialDetail.settings.onMarketClose);
      setOnStop(initialDetail.settings.onStop);
      setSymbols(initialDetail.symbols);
      setAccountIds(initialDetail.accountIds);
      setError(null);

      try {
        setStrategyConfig(JSON.parse(initialDetail.strategyConfigJson));
      } catch {
        setStrategyConfig({});
      }
      return;
    }

    /* ─── Create ─── */
    setName("");
    setStrategyKey(strategies[0]?.key ?? "");
    setPrimaryTimeFrame(TimeFrame.D1);
    setTargetCurrency("");
    setStrengthMultiplier("1.5");
setMinDeltaPercent("3");   
setCurrencyRateStatus("idle");
setCurrencyRateValue(null);
    setCapitalAllocationMode(CapitalAllocationMode.Fixed);
    setFixedCapitalAmount("");
    setSlMode("none");
    setSlPercent("");
    setSlPrice("");
    setTpMode("none");
    setTpPercent("");
    setTpPrice("");
    setDefaultOrderMode(OrderMode.Quantity);
    setOnMarketClose(MarketCloseBehavior.KeepPositions);
    setOnStop(StopRunBehavior.KeepPositions);
    setSymbols([]);
    setAccountIds([]);
    setError(null);
  }, [isOpen, mode, initialDetail, strategies]);

  /* ─── Strategy config: init from defaults when strategy changes (Create) ─── */
  useEffect(() => {
    if (!isOpen || mode !== "create") return;
    const strategy = strategies.find((s) => s.key === strategyKey);
    if (!strategy) {
      setStrategyConfig({});
      return;
    }
    const init: Record<string, any> = {};
    strategy.configFields.forEach((f) => {
      if (f.defaultValue == null) return;
      const t = f.typeName.toLowerCase();
      if (
        t.includes("int") ||
        t.includes("decimal") ||
        t.includes("double") ||
        t.includes("float") ||
        t.includes("long")
      ) {
        init[f.name] = Number(f.defaultValue);
      } else if (t === "boolean" || t === "bool") {
        init[f.name] = f.defaultValue === "true";
      } else {
        init[f.name] = f.defaultValue;
      }
    });
    setStrategyConfig(init);
  }, [strategyKey, strategies, isOpen, mode]);

  if (!isOpen) return null;

  const selectedStrategy = strategies.find((s) => s.key === strategyKey);

  /* ─── Validation ─── */
  const validate = (): string | null => {
    if (!name.trim()) return "نام پلن الزامی است";
    if (!strategyKey) return "انتخاب استراتژی الزامی است";
    if (symbols.length === 0) return "حداقل یک نماد باید انتخاب شود";
    const total = symbols.reduce((s, x) => s + x.allocationPercentage, 0);
    if (Math.abs(total - 100) > 0.01)
      return "مجموع درصد تخصیص نمادها باید ۱۰۰ باشد";
    if (accountIds.length === 0)
      return "حداقل یک حساب باید انتخاب شود";

    if (capitalAllocationMode === CapitalAllocationMode.Fixed) {
      const amt = Number(fixedCapitalAmount);
      if (!amt || amt <= 0)
        return "مبلغ سرمایه ثابت باید بزرگ‌تر از صفر باشد";
    }
    if (slMode === "percent") {
      const v = Number(slPercent);
      if (!v || v <= 0 || v > 100)
        return "درصد حد ضرر باید بین ۰ و ۱۰۰ باشد";
    }
    if (slMode === "price") {
      const v = Number(slPrice);
      if (!v || v <= 0) return "قیمت حد ضرر باید بزرگ‌تر از صفر باشد";
    }
    if (tpMode === "percent") {
      const v = Number(tpPercent);
      if (!v || v <= 0) return "درصد حد سود باید بزرگ‌تر از صفر باشد";
    }
    if (tpMode === "price") {
      const v = Number(tpPrice);
      if (!v || v <= 0) return "قیمت حد سود باید بزرگ‌تر از صفر باشد";
    }
    if (targetCurrency.trim() && currencyRateStatus === "missing") 
      {
        return "نرخ ارز این Currency در سیستم موجود نیست. ابتدا از صفحه‌ی «نرخ‌های ارز» ثبت کنید.";
      }
    return null;
  };

  /* ─── Submit ─── */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const err = validate();
    if (err) {
      setError(err);
      return;
    }
    setError(null);
    const sm = Number(strengthMultiplier);
    if (isNaN(sm) || sm < 0.1 || sm > 5) {
      return "ضریب قدرت باید بین 0.1 تا 5 باشد";
    }
    const md = Number(minDeltaPercent);
    if (isNaN(md) || md < 0 || md > 100) {
      return "حداقل تغییر باید بین 0 تا 100 درصد باشد";
    }
    const settings: TradingRunSettingsModel = {
      capitalAllocationMode,
      fixedCapitalAmount:
        capitalAllocationMode === CapitalAllocationMode.Fixed
          ? Number(fixedCapitalAmount)
          : null,
      stopLossPercentage:
        slMode === "percent" ? Number(slPercent) : null,
      stopLossPrice: slMode === "price" ? Number(slPrice) : null,
      takeProfitPercentage:
        tpMode === "percent" ? Number(tpPercent) : null,
      takeProfitPrice: tpMode === "price" ? Number(tpPrice) : null,
      defaultOrderMode,
      onMarketClose,
      onStop,
    };

    const base: CreateTradingRunCommand = {
  name: name.trim(),
  strategyKey,
  strategyConfigJson: JSON.stringify(strategyConfig),
  primaryTimeFrame,
  targetCurrency: targetCurrency.trim().toUpperCase() || null,
  strengthMultiplier: Number(strengthMultiplier) || 1.5,  
  minDeltaPercent: (Number(minDeltaPercent) || 0) / 100,   
  settings,
  symbols,
  accountIds,
};

    try {
      if (mode === "edit" && initialDetail) {
        await onSubmit({ id: initialDetail.id, ...base });
      } else {
        await onSubmit(base);
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطای نامشخص");
    }
  };

  /* ─── Render ─── */
  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/50 p-4">
      <div className="my-8 w-full max-w-3xl rounded-xl bg-white shadow-xl dark:bg-gray-800">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4 dark:border-gray-700">
          <h3 className="text-lg font-bold text-gray-900 dark:text-white">
            {mode === "edit" ? "ویرایش پلن معاملاتی" : "ایجاد پلن معاملاتی جدید"}
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

          {/* ۱. Basic */}
          <Section title="اطلاعات پایه">
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                نام پلن <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="مثلاً: RSI Daily فولاد و شپنا"
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            </div>
          </Section>

          {/* ۲. Strategy */}
          <Section title="استراتژی">
            <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
              {/* Currency */}
<div className="mt-3">
  <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
    ارز تحلیل (اختیاری)
  </label>
  <div className="flex items-center gap-2">
    <select
      value={targetCurrency || ""}
      onChange={(e) => setTargetCurrency(e.target.value)}
      className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
    >
      <option value="">— بدون تبدیل (ریال) —</option>
      <option value="USD">دلار آمریکا (USD)</option>
      <option value="EUR">یورو (EUR)</option>
      <option value="GBP">پوند انگلیس (GBP)</option>
      <option value="AED">درهم امارات (AED)</option>
      <option value="TRY">لیر ترکیه (TRY)</option>
      <option value="CNY">یوان چین (CNY)</option>
      <option value="JPY">ین ژاپن (JPY)</option>
      <option value="CHF">فرانک سوئیس (CHF)</option>
      <option value="GOLD">طلا (GOLD)</option>
    </select>
    {currencyRateStatus === "checking" && (
      <span className="whitespace-nowrap text-xs text-gray-500">
        ⏳ بررسی...
      </span>
    )}
    {currencyRateStatus === "ok" && currencyRateValue != null && (
      <span className="whitespace-nowrap text-xs text-emerald-500">
        ✓ {currencyRateValue.toLocaleString("en-US", { maximumFractionDigits: 2 })}
      </span>
    )}
    {currencyRateStatus === "missing" && (
      <span className="whitespace-nowrap text-xs text-red-500">
        ❌ نرخ ثبت نشده
      </span>
    )}
  </div>
  <p className="mt-1 text-[10px] text-gray-500">
    اگر خالی باشد، تحلیل روی قیمت ریالی انجام می‌شود. 
    برای ارز دیگر، ابتدا از صفحه‌ی «نرخ‌های ارز» نرخ ثبت کنید.
  </p>
</div>
{/* Strength Multiplier */}
<div className="mt-3">
  <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
    ضریب قدرت سیگنال (Stepped Buying)
  </label>
  <div className="flex items-center gap-2">
    <input
      type="number"
      value={strengthMultiplier}
      onChange={(e) => setStrengthMultiplier(e.target.value)}
      dir="ltr"
      min={0.1}
      max={5}
      step={0.1}
      className="w-32 rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
    />
    <span className="text-xs text-gray-500">
      {Number(strengthMultiplier) > 0
        ? `مثال: RSI=36 → ${(
            Number(strengthMultiplier) *
            0.1 *
            100
          ).toFixed(1)}% سرمایه`
        : "—"}
    </span>
  </div>
  <p className="mt-1 text-[10px] text-gray-500">
    سرمایه‌ی هدف = سرمایه‌ی Symbol × قدرت سیگنال × این ضریب.
    مقدار بالاتر → خرید بزرگ‌تر. پیش‌فرض ۱.۵.
  </p>
</div>
{/* Min Delta Percent */}
<div className="mt-3">
  <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
    حداقل تغییر سرمایه برای خرید مجدد (%)
  </label>
  <div className="flex items-center gap-2">
    <input
      type="number"
      value={minDeltaPercent}
      onChange={(e) => setMinDeltaPercent(e.target.value)}
      dir="ltr"
      min={0}
      max={100}
      step={0.5}
      className="w-32 rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
    />
    <span className="text-xs text-gray-500">
      {Number(minDeltaPercent) > 0
        ? `خرید مجدد فقط وقتی سهمی از سرمایه‌ی نماد بیش از ${Number(minDeltaPercent).toFixed(1)}٪ تغییر کند`
        : "بدون آستانه — هر افت کوچک باعث خرید می‌شود"}
    </span>
  </div>
  <p className="mt-1 text-[10px] text-gray-500">
    مثال: با ۳٪، در سرمایه‌ی ۱۰ میلیون ریالی، فقط خریدهای بیش از ۳۰۰٬۰۰۰ ریال
    اجرا می‌شوند. از خریدهای کوچک پشت‌سرهم جلوگیری می‌کند.
  </p>
</div>
              <div className="flex flex-col gap-1">
                <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                  استراتژی <span className="text-red-500">*</span>
                </label>
                <select
                  value={strategyKey}
                  onChange={(e) => setStrategyKey(e.target.value)}
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

              <div className="flex flex-col gap-1">
                <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                  تایم‌فریم
                </label>
                <select
                  value={primaryTimeFrame}
                  onChange={(e) =>
                    setPrimaryTimeFrame(Number(e.target.value))
                  }
                  className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                >
                  {Object.entries(TIMEFRAME_LABELS).map(([k, v]) => (
                    <option key={k} value={k}>
                      {v}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            {selectedStrategy && (
              <div className="mt-3">
                <p className="mb-2 text-xs font-semibold text-gray-600 dark:text-gray-400">
                  پارامترهای {selectedStrategy.displayName}
                </p>
                <StrategyConfigEditor
                  fields={selectedStrategy.configFields}
                  values={strategyConfig}
                  onChange={(n, v) =>
                    setStrategyConfig((prev) => ({ ...prev, [n]: v }))
                  }
                />
              </div>
            )}
          </Section>

          {/* ۳. Capital */}
          <Section title="تخصیص سرمایه">
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                حالت سرمایه
              </label>
              <select
                value={capitalAllocationMode}
                onChange={(e) =>
                  setCapitalAllocationMode(Number(e.target.value))
                }
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              >
                {Object.entries(CAPITAL_MODE_LABELS).map(([k, v]) => (
                  <option key={k} value={k}>
                    {v}
                  </option>
                ))}
              </select>
            </div>

            {capitalAllocationMode === CapitalAllocationMode.Fixed && (
              <div className="flex flex-col gap-1">
                <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                  مبلغ سرمایه (ریال) <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  value={fixedCapitalAmount}
                  onChange={(e) => setFixedCapitalAmount(e.target.value)}
                  dir="ltr"
                  min={0}
                  step="1000"
                  placeholder="مثلاً: 100000000"
                  className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                />
              </div>
            )}

            {capitalAllocationMode === CapitalAllocationMode.Dynamic && (
              <p className="rounded bg-slate-100 p-2 text-xs text-slate-600 dark:bg-slate-700 dark:text-slate-300">
                سرمایه از قدرت خرید واقعی حساب در هر لحظه خوانده می‌شود.
              </p>
            )}
          </Section>

          {/* ۴. SL/TP */}
          <Section title="حد ضرر و حد سود">
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              {/* SL */}
              <div className="space-y-2">
                <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                  حد ضرر (Stop Loss)
                </label>
                <select
                  value={slMode}
                  onChange={(e) => setSlMode(e.target.value as SlTpMode)}
                  className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                >
                  <option value="none">بدون حد ضرر</option>
                  <option value="percent">درصدی</option>
                  <option value="price">قیمت مطلق</option>
                </select>
                {slMode === "percent" && (
                  <input
                    type="number"
                    value={slPercent}
                    onChange={(e) => setSlPercent(e.target.value)}
                    dir="ltr"
                    min={0}
                    max={100}
                    step="0.01"
                    placeholder="درصد (مثلاً 5)"
                    className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  />
                )}
                {slMode === "price" && (
                  <input
                    type="number"
                    value={slPrice}
                    onChange={(e) => setSlPrice(e.target.value)}
                    dir="ltr"
                    min={0}
                    step="1"
                    placeholder="قیمت (ریال)"
                    className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  />
                )}
              </div>

              {/* TP */}
              <div className="space-y-2">
                <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                  حد سود (Take Profit)
                </label>
                <select
                  value={tpMode}
                  onChange={(e) => setTpMode(e.target.value as SlTpMode)}
                  className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                >
                  <option value="none">بدون حد سود</option>
                  <option value="percent">درصدی</option>
                  <option value="price">قیمت مطلق</option>
                </select>
                {tpMode === "percent" && (
                  <input
                    type="number"
                    value={tpPercent}
                    onChange={(e) => setTpPercent(e.target.value)}
                    dir="ltr"
                    min={0}
                    max={100}
                    step="0.01"
                    placeholder="درصد (مثلاً 10)"
                    className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  />
                )}
                {tpMode === "price" && (
                  <input
                    type="number"
                    value={tpPrice}
                    onChange={(e) => setTpPrice(e.target.value)}
                    dir="ltr"
                    min={0}
                    step="1"
                    placeholder="قیمت (ریال)"
                    className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  />
                )}
              </div>
            </div>
          </Section>

          {/* ۵. Behavior */}
          <Section title="رفتار اجرا">
            <div className="grid grid-cols-1 gap-3 md:grid-cols-3">
              <div className="flex flex-col gap-1">
                <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                  نوع سفارش
                </label>
                <select
                  value={defaultOrderMode}
                  onChange={(e) =>
                    setDefaultOrderMode(Number(e.target.value))
                  }
                  className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                >
                  {Object.entries(ORDER_MODE_LABELS).map(([k, v]) => (
                    <option key={k} value={k}>
                      {v}
                    </option>
                  ))}
                </select>
              </div>

              <div className="flex flex-col gap-1">
                <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                  پایان بازار
                </label>
                <select
                  value={onMarketClose}
                  onChange={(e) => setOnMarketClose(Number(e.target.value))}
                  className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                >
                  {Object.entries(MARKET_CLOSE_LABELS).map(([k, v]) => (
                    <option key={k} value={k}>
                      {v}
                    </option>
                  ))}
                </select>
              </div>

              <div className="flex flex-col gap-1">
                <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                  هنگام توقف
                </label>
                <select
                  value={onStop}
                  onChange={(e) => setOnStop(Number(e.target.value))}
                  className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                >
                  {Object.entries(STOP_BEHAVIOR_LABELS).map(([k, v]) => (
                    <option key={k} value={k}>
                      {v}
                    </option>
                  ))}
                </select>
              </div>
            </div>
          </Section>

          {/* ۶. Symbols */}
          <Section title="نمادها و تخصیص سرمایه">
            <SymbolAllocationEditor
              symbols={symbols}
              onChange={setSymbols}
            />
          </Section>

          {/* ۷. Accounts */}
          <Section title="حساب‌های معاملاتی">
            <AccountSelector
              value={accountIds}
              onChange={setAccountIds}
            />
          </Section>

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
              {saving
                ? "در حال ذخیره..."
                : mode === "edit"
                  ? "ذخیره تغییرات"
                  : "ایجاد پلن"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};