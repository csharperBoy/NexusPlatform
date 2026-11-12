import React, { useMemo, useState } from "react";
import { useOrderPresetsManagement } from "../../hooks/useOrderPresetsManagement";
import {
  DEFAULT_SIDE_OPTIONS,
  DEFAULT_ORDER_MODE_OPTIONS,
  VALIDITY_TYPE_OPTIONS,
  labelOf,
  type CreateOrderPresetCommand,
  type OrderPresetView,
  type UpdateOrderPresetCommand,
} from "../../models";
import { OrderPresetModal } from "./components/OrderPresetModal";
import { ResolvePreviewDrawer } from "./components/ResolvePreviewDrawer";

export const OrderPresetsManagementPage: React.FC = () => {
  const {
    presets,
    loading,
    busyId,
    error,
    refresh,
    handleCreate,
    handleUpdate,
    handleDelete,
  } = useOrderPresetsManagement();

  const [search, setSearch] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<"create" | "edit">("create");
  const [modalInitial, setModalInitial] = useState<OrderPresetView | null>(null);
  const [saving, setSaving] = useState(false);
  const [resolveId, setResolveId] = useState<string | null>(null);

  const [deleteTarget, setDeleteTarget] = useState<OrderPresetView | null>(null);
  const [deleting, setDeleting] = useState(false);

  const filtered = useMemo(() => {
    if (!search.trim()) return presets;
    const q = search.toLowerCase();
    return presets.filter(
      (p) =>
        p.name.toLowerCase().includes(q) ||
        p.instrumentSymbolName.toLowerCase().includes(q) ||
        p.instrumentIsin.toLowerCase().includes(q),
    );
  }, [presets, search]);

  const openCreate = () => {
    setModalMode("create");
    setModalInitial(null);
    setModalOpen(true);
  };

  const openEdit = (p: OrderPresetView) => {
    setModalMode("edit");
    setModalInitial(p);
    setModalOpen(true);
  };

  const submitCreate = async (cmd: CreateOrderPresetCommand) => {
    setSaving(true);
    try {
      await handleCreate(cmd);
      setModalOpen(false);
    } finally {
      setSaving(false);
    }
  };

  const submitUpdate = async (cmd: UpdateOrderPresetCommand) => {
    setSaving(true);
    try {
      await handleUpdate(cmd);
      setModalOpen(false);
    } finally {
      setSaving(false);
    }
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await handleDelete(deleteTarget.id);
      setDeleteTarget(null);
    } finally {
      setDeleting(false);
    }
  };

  return (
    <div className="space-y-4 p-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-900 dark:text-white">
            تنظیمات پیش‌فرض سفارش
          </h1>
          <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
            {presets.length} Preset — پیش‌فرض‌های ارسال دستی و سرخطی
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={refresh}
            disabled={loading}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 disabled:opacity-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
          >
            🔄 بروزرسانی
          </button>
          <button
            type="button"
            onClick={openCreate}
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
          >
            ➕ Preset جدید
          </button>
        </div>
      </div>

      <div className="rounded-lg border border-blue-700 bg-blue-900/20 p-3 text-xs text-blue-200">
        ℹ️ Preset سفارش فقط برای ارسال دستی و سرخطی استفاده می‌شود. برای اجرای
        الگوریتمی، تنظیمات از TradingRun گرفته می‌شود. دکمه‌ی 👁 مقدار لحظه‌ای
        قیمت و تعداد را از API بروکر resolve می‌کند.
      </div>

      {error && (
        <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
          ⚠️ {error}
        </div>
      )}

      {/* Filter */}
      <input
        type="text"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        placeholder="جستجو (نام، نماد، ISIN)..."
        className="w-full max-w-md rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
      />

      {/* Table */}
      <div className="overflow-x-auto rounded-lg border border-gray-200 dark:border-gray-700">
        <table className="w-full text-right text-sm text-gray-700 dark:text-gray-300">
          <thead className="bg-gray-50 text-xs dark:bg-gray-700">
            <tr>
              <th className="p-3">نام</th>
              <th className="p-3">نماد</th>
              <th className="p-3">جهت</th>
              <th className="p-3">قیمت (استراتژی)</th>
              <th className="p-3">تعداد (استراتژی)</th>
              <th className="p-3">کمیسیون</th>
              <th className="p-3">اعتبار</th>
              <th className="p-3">نوع سفارش</th>
              <th className="p-3 text-center">عملیات</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {loading && presets.length === 0 ? (
              <tr>
                <td colSpan={9} className="p-6 text-center">
                  در حال بارگذاری...
                </td>
              </tr>
            ) : filtered.length === 0 ? (
              <tr>
                <td colSpan={9} className="p-6 text-center">
                  هیچ Presetی یافت نشد
                </td>
              </tr>
            ) : (
              filtered.map((p) => (
                <tr
                  key={p.id}
                  className="hover:bg-gray-50 dark:hover:bg-gray-800/50"
                >
                  <td className="p-3 font-medium">{p.name}</td>
                  <td className="p-3">
                    <div>{p.instrumentSymbolName || "—"}</div>
                    <div className="font-mono text-[10px] text-gray-500">
                      {p.instrumentIsin}
                    </div>
                  </td>
                  <td className="p-3">
                    <span
                      className={`rounded px-2 py-0.5 text-[11px] font-bold ${
                        p.defaultSide === 0
                          ? "bg-emerald-900/40 text-emerald-300"
                          : "bg-red-900/40 text-red-300"
                      }`}
                    >
                      {p.defaultSideName || labelOf(DEFAULT_SIDE_OPTIONS, p.defaultSide)}
                    </span>
                  </td>
                  <td className="p-3 text-xs">
                    <div>{p.priceStrategyName}</div>
                    {p.fixedPrice != null && (
                      <div className="font-mono text-[10px] text-gray-500">
                        {p.fixedPrice.toLocaleString("en-US")}
                      </div>
                    )}
                  </td>
                  <td className="p-3 text-xs">
                    <div>{p.quantityStrategyName}</div>
                    {p.fixedQuantity != null && (
                      <div className="font-mono text-[10px] text-gray-500">
                        {p.fixedQuantity.toLocaleString("en-US")}
                      </div>
                    )}
                  </td>
                  <td className="p-3 font-mono text-xs">
                    {(p.commission * 100).toFixed(2)}%
                  </td>
                  <td className="p-3 text-xs">
                    {labelOf(VALIDITY_TYPE_OPTIONS, p.validityType)}
                  </td>
                  <td className="p-3 text-xs">
                    {p.defaultOrderModeName || labelOf(DEFAULT_ORDER_MODE_OPTIONS, p.defaultOrderMode)}
                  </td>
                  <td className="p-3">
                    <div className="flex justify-end gap-1">
                      <button
                        type="button"
                        onClick={() => setResolveId(p.id)}
                        title="Resolve لحظه‌ای"
                        className="rounded border border-gray-300 px-2 py-1 text-xs text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
                      >
                        👁
                      </button>
                      <button
                        type="button"
                        disabled={busyId === p.id}
                        onClick={() => openEdit(p)}
                        className="rounded border border-gray-300 px-2 py-1 text-xs text-gray-700 hover:bg-gray-100 disabled:opacity-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
                      >
                        ✏ ویرایش
                      </button>
                      <button
                        type="button"
                        disabled={busyId === p.id}
                        onClick={() => setDeleteTarget(p)}
                        className="rounded bg-red-600 px-2 py-1 text-xs text-white hover:bg-red-700 disabled:opacity-50"
                      >
                        🗑 حذف
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Create / Edit Modal */}
      <OrderPresetModal
        isOpen={modalOpen}
        mode={modalMode}
        initial={modalInitial}
        onClose={() => setModalOpen(false)}
        onCreate={submitCreate}
        onUpdate={submitUpdate}
        saving={saving}
      />

      {/* Resolve Drawer */}
      <ResolvePreviewDrawer
        isOpen={resolveId !== null}
        presetId={resolveId}
        onClose={() => setResolveId(null)}
      />

      {/* Delete Confirm */}
      {deleteTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl dark:bg-gray-800">
            <h3 className="mb-2 text-lg font-bold text-gray-900 dark:text-white">
              تایید حذف
            </h3>
            <p className="mb-4 text-sm text-gray-600 dark:text-gray-300">
              آیا از حذف Preset{" "}
              <strong className="text-gray-900 dark:text-white">
                {deleteTarget.name}
              </strong>{" "}
              (نماد {deleteTarget.instrumentSymbolName}) اطمینان دارید؟
            </p>
            <div className="flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setDeleteTarget(null)}
                className="rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
              >
                انصراف
              </button>
              <button
                type="button"
                onClick={confirmDelete}
                disabled={deleting}
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
              >
                {deleting ? "در حال حذف..." : "حذف قطعی"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};