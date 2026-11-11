import React, { useEffect, useState } from "react";
import { instrumentApi } from "../../../api/instrumentApi";
import {
  DEFAULT_SIDE_OPTIONS,
  DEFAULT_ORDER_MODE_OPTIONS,
  VALIDITY_TYPE_OPTIONS,
  type CreateOrderPresetCommand,
  type OrderPresetView,
  type UpdateOrderPresetCommand,
} from "../../../models";
import type { SelectionListDto } from "@/core/models/SelectionListDto";

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
  const [defaultPrice, setDefaultPrice] = useState("");
  const [defaultQuantity, setDefaultQuantity] = useState("");
  const [defaultSide, setDefaultSide] = useState(0);
  const [validityType, setValidityType] = useState(0);
  const [commission, setCommission] = useState("0.0037");
  const [defaultOrderMode, setDefaultOrderMode] = useState(0);

  /* ─── Instrument options ─── */
  const [instruments, setInstruments] = useState<SelectionListDto[]>([]);
  const [loadingInstruments, setLoadingInstruments] = useState(false);

  const [error, setError] = useState<string | null>(null);

  /* ─── Load instruments (create only) ─── */
  useEffect(() => {
    if (!isOpen || mode === "edit") return;
    setLoadingInstruments(true);
    (async () => {
      try {
        const list = await instrumentApi.GetSelectionList();
        setInstruments(list);
      } catch {
        setInstruments([]);
      } finally {
        setLoadingInstruments(false);
      }
    })();
  }, [isOpen, mode]);

  /* ─── Init from `initial` (edit) or reset (create) ─── */
  useEffect(() => {
    if (!isOpen) return;

    if (mode === "edit" && initial) {
      setInstrumentId(initial.instrumentId);
      setName(initial.name);
      setDefaultPrice(initial.defaultPrice.toString());
      setDefaultQuantity(initial.defaultQuantity.toString());
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
    setDefaultPrice("");
    setDefaultQuantity("");
    setDefaultSide(0);
    setValidityType(0);
    setCommission("0.0037");
    setDefaultOrderMode(0);
    setError(null);
  }, [isOpen, mode, initial]);

  if (!isOpen) return null;

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
    const price = Number(defaultPrice);
    if (!price || price <= 0) {
      setError("قیمت باید بزرگ‌تر از صفر باشد");
      return;
    }
    const qty = Number(defaultQuantity);
    if (!qty || qty <= 0 || !Number.isInteger(qty)) {
      setError("تعداد باید عدد صحیح مثبت باشد");
      return;
    }
    const comm = Number(commission);
    if (isNaN(comm) || comm < 0 || comm > 0.1) {
      setError("کمیسیون باید بین ۰ و ۰.۱ باشد");
      return;
    }

    try {
      setError(null);
      if (mode === "create") {
        await onCreate({
          instrumentId,
          name: name.trim(),
          defaultPrice: price,
          defaultQuantity: qty,
          defaultSide,
          validityType,
          commission: comm,
          defaultOrderMode,
        });
      } else if (initial) {
        await onUpdate({
          id: initial.id,
          name: name.trim(),
          defaultPrice: price,
          defaultQuantity: qty,
          defaultSide,
          validityType,
          commission: comm,
          defaultOrderMode,
        });
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
                  <option key={i.value} value={i.value}>
                    {i.display || i.label}
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

          {/* Price × Qty */}
          <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                قیمت پیش‌فرض <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                value={defaultPrice}
                onChange={(e) => setDefaultPrice(e.target.value)}
                dir="ltr"
                min={1}
                step={1}
                placeholder="ریال"
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                تعداد پیش‌فرض <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                value={defaultQuantity}
                onChange={(e) => setDefaultQuantity(e.target.value)}
                dir="ltr"
                min={1}
                step={1}
                placeholder="تعداد سهم"
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            </div>
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