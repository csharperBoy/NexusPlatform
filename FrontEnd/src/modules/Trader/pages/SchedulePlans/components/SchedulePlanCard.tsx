import { useEffect, useState } from "react";
import type {
  SchedulePlanInfoView,
  UpdateSchedulePlanCommand,
  ScheduledOrderInfoView,
  OrderMode,
} from "../../../models";

interface Props {
  plan: SchedulePlanInfoView;
  accounts: { id: string; name: string }[];
  symbols: { symbolIsin: string; symbolName: string }[];
  saving: boolean;
  onSave: (cmd: UpdateSchedulePlanCommand) => void;
  onDelete: (id: string) => void;
  onEnable: (id: string) => void;
  onDisable: (id: string) => void;
}

const TIME_PRESETS = [
  "08:45:00.000",
  "08:45:00.100",
  "08:45:00.250",
  "08:45:00.500",
];

export function SchedulePlanCard({
  plan,
  accounts,
  symbols,
  saving,
  onSave,
  onDelete,
  onEnable,
  onDisable,
}: Props) {
  const [collapsed, setCollapsed] = useState(false);
  const [draft, setDraft] = useState<SchedulePlanInfoView>(plan);
  const [dirty, setDirty] = useState(false);

  /* هر بار که plan از سرور اومد، draft رو reset کن */
  useEffect(() => {
    setDraft(plan);
    setDirty(false);
  }, [plan]);

  const update = <K extends keyof SchedulePlanInfoView>(
    key: K,
    value: SchedulePlanInfoView[K],
  ) => {
    setDraft((prev) => ({ ...prev, [key]: value }));
    setDirty(true);
  };

  const updateOrder = (
    orderId: string,
    patch: Partial<ScheduledOrderInfoView>,
  ) => {
    setDraft((prev) => ({
      ...prev,
      orders: prev.orders.map((o) =>
        o.id === orderId ? { ...o, ...patch } : o,
      ),
    }));
    setDirty(true);
  };

  const addOrder = () => {
    if (accounts.length === 0 || symbols.length === 0) return;
    const newOrder: ScheduledOrderInfoView = {
      id: `temp-${Date.now()}`,
      accountId: accounts[0].id,
      symbolIsin: symbols[0].symbolIsin,
      side: 0,
      mode: "quantity",
      quantity: "100",
      totalValue: "10000000",
      time: "08:45:00.000",
      fired: false,
    };
    update("orders", [...draft.orders, newOrder]);
  };

  const removeOrder = (orderId: string) => {
    update(
      "orders",
      draft.orders.filter((o) => o.id !== orderId),
    );
  };

  const handleSave = () => {
    onSave({
      id: draft.id,
      name: draft.name,
      date: draft.date,
      enabled: draft.enabled,
      autoLoginAt: draft.autoLoginAt,
      autoRefreshAt: draft.autoRefreshAt,
      orders: draft.orders.map((o) => ({
        accountId: o.accountId,
        symbolIsin: o.symbolIsin,
        side: o.side,
        mode: o.mode,
        quantity: o.quantity,
        totalValue: o.totalValue,
        time: o.time,
      })),
    });
  };

  /* ─── Collapsed view ─── */
  if (collapsed) {
    return (
      <div className="flex flex-wrap items-center gap-3 rounded-xl border border-slate-700 bg-slate-900 p-3">
        <button
          type="button"
          className="flex h-7 w-7 items-center justify-center rounded-lg bg-slate-800 text-lg font-bold text-blue-300 hover:bg-slate-700"
          onClick={() => setCollapsed(false)}
        >
          +
        </button>
        <span className="font-semibold text-slate-200">{plan.name}</span>
        <span className="font-mono text-xs text-slate-400">{plan.date}</span>
        {plan.enabled && (
          <span className="rounded bg-emerald-900/40 px-2 py-0.5 text-[10px] text-emerald-300">
            فعال
          </span>
        )}
        <span className="text-xs text-slate-400">
          {plan.orders.length} سفارش
        </span>
      </div>
    );
  }

  /* ─── Expanded view ─── */
  return (
    <div className="space-y-3 rounded-xl border border-slate-700 bg-slate-900 p-3">
      {/* Header */}
      <div className="flex flex-wrap items-end gap-2">
        <button
          type="button"
          className="flex h-7 w-7 items-center justify-center rounded-lg bg-slate-800 text-lg font-bold text-blue-300 hover:bg-slate-700"
          onClick={() => setCollapsed(true)}
        >
          −
        </button>

        <div className="flex-1">
          <label className="mb-1 block text-xs text-slate-400">نام برنامه</label>
          <input
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-2 py-1.5 text-sm text-slate-100"
            value={draft.name}
            onChange={(e) => update("name", e.target.value)}
          />
        </div>

        <div>
          <label className="mb-1 block text-xs text-slate-400">تاریخ</label>
          <input
            type="date"
            className="rounded-lg border border-slate-700 bg-slate-950 px-2 py-1.5 font-mono text-xs text-slate-100"
            value={draft.date}
            onChange={(e) => update("date", e.target.value)}
          />
        </div>

        <button
          type="button"
          className={`rounded-lg px-3 py-1.5 text-xs font-medium ${
            draft.enabled
              ? "bg-emerald-600 text-white hover:bg-emerald-500"
              : "bg-slate-700 text-slate-300 hover:bg-slate-600"
          }`}
          onClick={() =>
            draft.enabled ? onDisable(draft.id) : onEnable(draft.id)
          }
        >
          {draft.enabled ? "✓ فعال" : "غیرفعال"}
        </button>

        <button
          type="button"
          className="rounded border border-red-900 px-2 py-1 text-xs text-red-400 hover:bg-red-950"
          onClick={() => {
            if (confirm(`حذف "${draft.name}"؟`)) onDelete(draft.id);
          }}
        >
          ✕
        </button>
      </div>

      {/* Status message */}
      {plan.message && (
        <div className="rounded-lg border border-blue-900/40 bg-blue-950/20 px-3 py-2 text-xs text-blue-200">
          {plan.status}: {plan.message}
        </div>
      )}

      {/* Times */}
      <div className="grid grid-cols-2 gap-3 rounded-lg bg-slate-950 p-3">
        <div>
          <label className="mb-1 block text-xs text-slate-400">
            ساعت لاگین
          </label>
          <input
            type="text"
            className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 font-mono text-sm text-slate-100"
            value={draft.autoLoginAt}
            onChange={(e) => update("autoLoginAt", e.target.value)}
          />
        </div>
        <div>
          <label className="mb-1 block text-xs text-slate-400">
            ساعت رفرش قیمت
          </label>
          <input
            type="text"
            className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 font-mono text-sm text-slate-100"
            value={draft.autoRefreshAt}
            onChange={(e) => update("autoRefreshAt", e.target.value)}
          />
        </div>
      </div>

      {/* Orders */}
      <div className="flex items-center justify-between text-xs text-slate-400">
        <span>سفارش‌ها ({draft.orders.length})</span>
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-2 py-1 text-blue-300 hover:bg-slate-700 disabled:opacity-50"
          onClick={addOrder}
          disabled={accounts.length === 0 || symbols.length === 0}
        >
          + افزودن سفارش
        </button>
      </div>

      {draft.orders.length === 0 && (
        <div className="rounded-lg bg-slate-950 p-3 text-center text-xs text-slate-500">
          سفارشی نداره
        </div>
      )}

      {draft.orders.map((o, idx) => (
        <OrderRow
          key={o.id}
          order={o}
          index={idx}
          accounts={accounts}
          symbols={symbols}
          onChange={(patch) => updateOrder(o.id, patch)}
          onRemove={() => removeOrder(o.id)}
        />
      ))}

      {/* Save button */}
      {dirty && (
        <div className="flex justify-end gap-2 border-t border-slate-700 pt-3">
          <button
            type="button"
            className="rounded-lg border border-slate-600 px-3 py-1.5 text-xs text-slate-300 hover:bg-slate-800"
            onClick={() => {
              setDraft(plan);
              setDirty(false);
            }}
          >
            لغو تغییرات
          </button>
          <button
            type="button"
            className="rounded-lg bg-emerald-600 px-4 py-1.5 text-xs font-bold text-white hover:bg-emerald-500 disabled:opacity-50"
            onClick={handleSave}
            disabled={saving}
          >
            {saving ? "در حال ذخیره..." : "💾 ذخیره تغییرات"}
          </button>
        </div>
      )}
    </div>
  );
}

/* ══════════════════════════════════════════════ */
interface OrderRowProps {
  order: ScheduledOrderInfoView;
  index: number;
  accounts: { id: string; name: string }[];
  symbols: { symbolIsin: string; symbolName: string }[];
  onChange: (patch: Partial<ScheduledOrderInfoView>) => void;
  onRemove: () => void;
}

function OrderRow({
  order,
  index,
  accounts,
  symbols,
  onChange,
  onRemove,
}: OrderRowProps) {
  const sym = symbols.find((s) => s.symbolIsin === order.symbolIsin);
  const acc = accounts.find((a) => a.id === order.accountId);

  return (
    <div
      className={`space-y-2 rounded-lg border p-2 ${
        order.fired
          ? "border-emerald-900/60 bg-emerald-950/10"
          : "border-slate-700 bg-slate-950"
      }`}
    >
      <div className="flex items-center justify-between text-xs">
        <span className="text-slate-500">
          #{index + 1}
          {order.fired && (
            <span className="mr-2 text-emerald-400">✓ ارسال شد</span>
          )}
        </span>
        <span className="font-semibold text-blue-300">
          {sym?.symbolName ?? "—"} — {acc?.name ?? "—"}
        </span>
        <button
          type="button"
          className="rounded border border-red-900 px-2 py-0.5 text-red-400 hover:bg-red-950"
          onClick={onRemove}
        >
          ✕
        </button>
      </div>

      <div className="grid grid-cols-3 gap-2">
        <select
          className="rounded-lg border border-slate-700 bg-slate-900 px-2 py-1 text-xs text-slate-100"
          value={order.accountId}
          onChange={(e) => onChange({ accountId: e.target.value })}
        >
          <option value="">— حساب —</option>
          {accounts.map((a) => (
            <option key={a.id} value={a.id}>
              {a.name}
            </option>
          ))}
        </select>

        <select
          className="rounded-lg border border-slate-700 bg-slate-900 px-2 py-1 text-xs text-slate-100"
          value={order.symbolIsin}
          onChange={(e) => onChange({ symbolIsin: e.target.value })}
        >
          <option value="">— نماد —</option>
          {symbols.map((s) => (
            <option key={s.symbolIsin} value={s.symbolIsin}>
              {s.symbolName}
            </option>
          ))}
        </select>

        <select
          className="rounded-lg border border-slate-700 bg-slate-900 px-2 py-1 text-xs text-slate-100"
          value={order.side}
          onChange={(e) => onChange({ side: Number(e.target.value) })}
        >
          <option value={0}>خرید</option>
          <option value={1}>فروش</option>
        </select>
      </div>

      <div className="grid grid-cols-2 gap-2">
        <div>
          <div className="mb-1 flex gap-2 text-xs text-slate-300">
            <label className="flex cursor-pointer items-center gap-1">
              <input
                type="radio"
                checked={order.mode === "quantity"}
                onChange={() => onChange({ mode: "quantity" as OrderMode })}
              />
              تعداد
            </label>
            <label className="flex cursor-pointer items-center gap-1">
              <input
                type="radio"
                checked={order.mode === "totalValue"}
                onChange={() => onChange({ mode: "totalValue" as OrderMode })}
              />
              مبلغ کل
            </label>
          </div>
          {order.mode === "quantity" ? (
            <input
              type="number"
              className="w-full rounded-lg border border-slate-700 bg-slate-900 px-2 py-1 text-xs text-slate-100"
              value={order.quantity}
              onChange={(e) => onChange({ quantity: e.target.value })}
              placeholder="تعداد"
            />
          ) : (
            <input
              type="number"
              className="w-full rounded-lg border border-slate-700 bg-slate-900 px-2 py-1 text-xs text-slate-100"
              value={order.totalValue}
              onChange={(e) => onChange({ totalValue: e.target.value })}
              placeholder="مبلغ (ریال)"
            />
          )}
        </div>

        <div>
          <label className="mb-1 block text-xs text-slate-400">زمان</label>
          <input
            type="text"
            className="w-full rounded-lg border border-slate-700 bg-slate-900 px-2 py-1 font-mono text-xs text-slate-100"
            value={order.time}
            onChange={(e) => onChange({ time: e.target.value })}
          />
        </div>
      </div>

      <div className="flex flex-wrap gap-1">
        {TIME_PRESETS.map((t) => (
          <button
            key={t}
            type="button"
            className="rounded border border-slate-700 bg-slate-800 px-2 py-0.5 font-mono text-[10px] text-blue-300 hover:bg-slate-700"
            onClick={() => onChange({ time: t })}
          >
            {t}
          </button>
        ))}
      </div>
    </div>
  );
}