import React, { useEffect, useState } from "react";
import { instrumentApi } from "../../../api/instrumentApi";
import {
  DEFAULT_SIDE_OPTIONS,
  DEFAULT_ORDER_MODE_OPTIONS,
  VALIDITY_TYPE_OPTIONS,
  OrderPriceStrategy,
  OrderQuantityStrategy,
  ORDER_PRICE_STRATEGY_OPTIONS,
  ORDER_QUANTITY_STRATEGY_OPTIONS,
  type CreateOrderPresetCommand,
  type OrderPresetView,
  type UpdateOrderPresetCommand,
} from "../../../models";
import type { InstrumentInfoView } from "../../../models";

interface Props {
  isOpen: boolean;
  mode: "create" | "edit";
  initial: OrderPresetView | null;
  onClose: () => void;
  onCreate: (cmd: CreateOrderPresetCommand) => Promise<void>;
  onUpdate: (cmd: UpdateOrderPresetCommand) => Promise<void>;
  saving: boolean;
}

export const OrderPresetModal: React.FC<Props> = ({
  isOpen,
  mode,
  initial,
  onClose,
  onCreate,
  onUpdate,
  saving,
}) => {
  /* ─── Form state ─── */
  const [instrumentId, setInstrumentId] = useState("");
  const [name, setName] = useState("");
  const [priceStrategy, setPriceStrategy] = useState<number>(OrderPriceStrategy.Fixed);
  const [fixedPrice, setFixedPrice] = useState("");
  const [quantityStrategy, setQuantityStrategy] = useState<number>(
    OrderQuantityStrategy.Fixed,
  );
  const [fixedQuantity, setFixedQuantity] = useState("");
  const [defaultSide, setDefaultSide] = useState(0);
  const [validityType, setValidityType] = useState(0);
  const [commission, setCommission] = useState("0.0037");
  const [defaultOrderMode, setDefaultOrderMode] = useState(0);

  /* ─── Instrument options (full data) ─── */
  const [instruments, setInstruments] = useState<InstrumentInfoView[]>([]);
  const [loadingInstruments, setLoadingInstruments] = useState(false);

  const [error, setError] = useState<string | null>(null);

  /* ─── Load instruments ─── */
  useEffect(() => {
    if (!isOpen) return;
    setLoadingInstruments(true);
    (async () => {
      try {
        // GetList returns InstrumentInfoView[] with id (Guid) and range info
        // (GetSelectionList returns ISIN in .value — wrong for instrumentId)
        const list = await instrumentApi.GetList();
        setInstruments(list.filter((i) => i.isActive));
      } catch {
        setInstruments([]);
      } finally {
        setLoadingInstruments(false);
      }
    })();
  }, [isOpen]);

  /* ─── Init form ─── */
  useEffect(() => {
    if (!isOpen) return;

    if (mode === "edit" && initial) {
      setInstrumentId(initial.instrumentId);
      setName(initial.name);
      setPriceStrategy(initial.priceStrategy);
      setFixedPrice(initial.fixedPrice?.toString() ?? "");
      setQuantityStrategy(initial.quantityStrategy);
      setFixedQuantity(initial.fixedQuantity?.toString() ?? "");
      setDefaultSide(initial.defaultSide);
      setValidityType(initial.validityType);
      setCommission(initial.commission.toString());
      setDefaultOrderMode(initial.defaultOrderMode);
      setError(null);
      return;
    }

    /* create */
    setInstrumentId("");
    setName("");
    setPriceStrategy(OrderPriceStrategy.Fixed);
    setFixedPrice("");
    setQuantityStrategy(OrderQuantityStrategy.Fixed);
    setFixedQuantity("");
    setDefaultSide(0);
    setValidityType(0);
    setCommission("0.0037");
    setDefaultOrderMode(0);
    setError(null);
  }, [isOpen, mode, initial]);

  if (!isOpen) return null;

  /* ─── Range hint lookup ─── */
  const selectedInstrument = instruments.find((i) => i.id === instrumentId);
  const editInstrument =
    mode === "edit" && initial
      ? instruments.find((i) => i.id === initial.instrumentId)
      : null;
  const lookupInstrument = selectedInstrument ?? editInstrument;

  const showRangeHint =
    priceStrategy === OrderPriceStrategy.HighAllowed ||
    priceStrategy === OrderPriceStrategy.LowAllowed;

  const relevantRangePercent = lookupInstrument
    ? priceStrategy === OrderPriceStrategy.HighAllowed
      ? lookupInstrument.dailyPriceRangeUpPercent
      : priceStrategy === OrderPriceStrategy.LowAllowed
        ? lookupInstrument.dailyPriceRangeDownPercent
        : null
    : null;

  const rangeDirectionLabel =
    priceStrategy === OrderPriceStrategy.HighAllowed ? "بالا" : "پایین";

  /* ─── Submit ─── */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (mode === "create" && !instrumentId) {
      setError("انتخاب نماد الزامی است");
      return;
    }
    if (!name.trim()) {
      setError("نام Preset الزامی است");
      return;
    }
    if (priceStrategy === OrderPriceStrategy.Fixed) {
      const price = Number(fixedPrice);
      if (!price || price <= 0) {
        setError("قیمت ثابت باید بزرگ‌تر از صفر باشد");
        return;
      }
    }
    if (quantityStrategy === OrderQuantityStrategy.Fixed) {
      const qty = Number(fixedQuantity);
      if (!qty || qty <= 0 || !Number.isInteger(qty)) {
        setError("تعداد ثابت باید عدد صحیح مثبت باشد");
        return;
      }
    }
    const comm = Number(commission);
    if (isNaN(comm) || comm < 0 || comm > 0.1) {
      setError("کمیسیون باید بین ۰ و ۰.۱ باشد");
      return;
    }

    const baseCmd = {
      name: name.trim(),
      priceStrategy,
      fixedPrice: priceStrategy === OrderPriceStrategy.Fixed ? Number(fixedPrice) : null,
      quantityStrategy,
      fixedQuantity:
        quantityStrategy === OrderQuantityStrategy.Fixed ? Number(fixedQuantity) : null,
      defaultSide,
      validityType,
      commission: comm,
      defaultOrderMode,
    };

    try {
      setError(null);
      if (mode === "create") {
        await onCreate({ instrumentId, ...baseCmd });
      } else if (initial) {
        await onUpdate({ id: initial.id, ...baseCmd });
      }
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
            {mode === "edit" ? "ویرایش Preset سفارش" : "ساخت Preset سفارش جدید"}
          </h3>
          <button
            type="button"
            onClick={onClose}
            className="text-2xl leading-none text-gray-500 hover:text-gray-700 dark:text-gray-400"
          >
            ×
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4 p-6">
          {error && (
            <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
              ⚠️ {error}
            </div>
          )}

          {/* Instrument */}
          <div className="flex flex-col gap-1">
            <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
              نماد <span className="text-red-500">*</span>
            </label>
            {mode === "create" ? (
              <select
                value={instrumentId}
                onChange={(e) => setInstrumentId(e.target.value)}
                disabled={loadingInstruments}
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              >
                <option value="">
                  {loadingInstruments ? "در حال بارگذاری..." : "انتخاب نماد..."}
                </option>
                {instruments.map((i) => (
                  <option key={i.id} value={i.id}>
                    {i.displayName} ({i.symbolName})
                  </option>
                ))}
              </select>
            ) : (
              <div className="rounded border border-gray-200 bg-gray-50 p-2 text-sm dark:border-gray-700 dark:bg-gray-700">
                <span className="font-medium">
                  {initial?.instrumentSymbolName || "—"}
                </span>
                <span className="mr-2 font-mono text-[11px] text-gray-500">
                  ({initial?.instrumentIsin})
                </span>
                <div className="mt-0.5 text-[10px] text-gray-500">
                  نماد در ویرایش قابل تغییر نیست
                </div>
              </div>
            )}
          </div>

          {/* Name */}
          <div className="flex flex-col gap-1">
            <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
              نام Preset <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="مثلاً: سفارش روزانه فولاد"
              className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            />
          </div>

          {/* Price Strategy */}
          <div className="space-y-2 rounded-lg border border-gray-200 p-4 dark:border-gray-700">
            <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
              قیمت <span className="text-red-500">*</span>
            </label>
            <select
              value={priceStrategy}
              onChange={(e) => setPriceStrategy(Number(e.target.value))}
              className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            >
              {ORDER_PRICE_STRATEGY_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>

            {priceStrategy === OrderPriceStrategy.Fixed ? (
              <input
                type="number"
                value={fixedPrice}
                onChange={(e) => setFixedPrice(e.target.value)}
                dir="ltr"
                min={1}
                step={1}
                placeholder="قیمت ثابت (ریال)"
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            ) : (
              <p className="rounded bg-blue-50 p-2 text-[11px] text-blue-700 dark:bg-blue-900/20 dark:text-blue-300">
                ℹ️ قیمت در لحظه‌ی ارسال از API بروکر خوانده می‌شود. برای پیش‌نمایش،
                دکمه‌ی 👁 در لیست استفاده کن.
              </p>
            )}

            {/* Range hint */}
            {showRangeHint && lookupInstrument && (
              <>
                {relevantRangePercent == null ? (
                  <p className="rounded bg-amber-50 p-2 text-[11px] text-amber-700 dark:bg-amber-900/20 dark:text-amber-300">
                    ⚠️ این نماد بازه‌ی روزانه‌ی {rangeDirectionLabel} ندارد
                    (unlimited). اگر API لحظه‌ای مقدار ندهد، Resolve خطا می‌دهد.
                    برای استفاده از fallback، بازه را در صفحه‌ی «نمادها» تنظیم کن.
                  </p>
                ) : (
                  <p className="rounded bg-emerald-50 p-2 text-[11px] text-emerald-700 dark:bg-emerald-900/20 dark:text-emerald-300">
                    ✓ بازه‌ی {rangeDirectionLabel}: ±{relevantRangePercent}٪ از
                    قیمت پایانی. اگر API مقدار نداد، از این بازه تخمین زده
                    می‌شود.
                  </p>
                )}
              </>
            )}
          </div>

          {/* Quantity Strategy */}
          <div className="space-y-2 rounded-lg border border-gray-200 p-4 dark:border-gray-700">
            <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
              تعداد <span className="text-red-500">*</span>
            </label>
            <select
              value={quantityStrategy}
              onChange={(e) => setQuantityStrategy(Number(e.target.value))}
              className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            >
              {ORDER_QUANTITY_STRATEGY_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>

            {quantityStrategy === OrderQuantityStrategy.Fixed ? (
              <input
                type="number"
                value={fixedQuantity}
                onChange={(e) => setFixedQuantity(e.target.value)}
                dir="ltr"
                min={1}
                step={1}
                placeholder="تعداد ثابت"
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            ) : (
              <p className="rounded bg-blue-50 p-2 text-[11px] text-blue-700 dark:bg-blue-900/20 dark:text-blue-300">
                ℹ️ تعداد از تنظیمات نماد (
                {quantityStrategy === OrderQuantityStrategy.MaxOrderQuantity
                  ? "حداکثر مجاز"
                  : "حداقل مجاز"}
                ) خوانده می‌شود.
              </p>
            )}
          </div>

          {/* Side × OrderMode */}
          <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                جهت
              </label>
              <select
                value={defaultSide}
                onChange={(e) => setDefaultSide(Number(e.target.value))}
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              >
                {DEFAULT_SIDE_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                نوع سفارش
              </label>
              <select
                value={defaultOrderMode}
                onChange={(e) => setDefaultOrderMode(Number(e.target.value))}
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              >
                {DEFAULT_ORDER_MODE_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Validity × Commission */}
          <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                اعتبار سفارش
              </label>
              <select
                value={validityType}
                onChange={(e) => setValidityType(Number(e.target.value))}
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              >
                {VALIDITY_TYPE_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                کمیسیون (نسبت)
              </label>
              <input
                type="number"
                value={commission}
                onChange={(e) => setCommission(e.target.value)}
                dir="ltr"
                min={0}
                max={0.1}
                step={0.0001}
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
              <span className="text-[10px] text-gray-500">
                پیش‌فرض ۰.۰۰۳۷ = ۰.۳۷٪
              </span>
            </div>
          </div>

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
                  : "ساخت Preset"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};