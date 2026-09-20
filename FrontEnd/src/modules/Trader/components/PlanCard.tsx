import { useState } from "react";
import {
  useScheduleStore,
  useAccountsStore,
  useSymbolsStore,
} from "../stores";
import { TIME_PRESETS } from "../constants";
import { todayDateKey, tomorrowDateKey } from "../utils";
import type { SchedulePlan, ScheduledOrder } from "../models";

interface Props {
  plan: SchedulePlan;
}

export function PlanCard({ plan }: Props) {
  const updatePlan = useScheduleStore((s) => s.updatePlan);
  const removePlan = useScheduleStore((s) => s.removePlan);
  const duplicatePlan = useScheduleStore((s) => s.duplicatePlan);
  const toggleCollapsed = useScheduleStore((s) => s.toggleCollapsed);
  const addOrder = useScheduleStore((s) => s.addOrder);

  const accounts = useAccountsStore((s) => s.accounts);
  const symbols = useSymbolsStore((s) => s.symbols);

  const planMessage = useScheduleStore((s) => s.runtime.planMessages[plan.id]);
  const planState = useScheduleStore((s) => s.runtime.planStates[plan.id]);
  const planLogs = planState?.logs ?? [];

  const [dupDate, setDupDate] = useState(tomorrowDateKey());
  const [jsonOpen, setJsonOpen] = useState(false);
  const [jsonText, setJsonText] = useState("");
  const [jsonFeedback, setJsonFeedback] = useState<string | null>(null);

  const today = todayDateKey();
  const isToday = plan.date === today;
  const fired = planState?.firedOrderIds.length ?? 0;
  const total = plan.orders.length;

  /* ═══ Export/Import per-plan ═══ */
  const handleExport = () => {
    const data = {
      version: 2,
      exportedAt: new Date().toISOString(),
      plan: {
        name: plan.name,
        date: plan.date,
        enabled: plan.enabled,
        autoLoginAt: plan.autoLoginAt,
        autoRefreshAt: plan.autoRefreshAt,
        orders: plan.orders.map((o) => ({
          accountId: o.accountId,
          symbolIsin: o.symbolIsin,
          side: o.side,
          mode: o.mode,
          quantity: o.quantity,
          totalValue: o.totalValue,
          time: o.time,
        })),
      },
    };
    const text = JSON.stringify(data, null, 2);
    navigator.clipboard.writeText(text).then(
      () => setJsonFeedback("✅ کپی شد"),
      () => {
        setJsonText(text);
        setJsonOpen(true);
        setJsonFeedback("کپی نشد — دستی بردار");
      },
    );
  };

  const handleImport = () => {
    try {
      const data = JSON.parse(jsonText);
      const p = data?.plan ?? data;
      if (!p || !Array.isArray(p.orders)) {
        setJsonFeedback("❌ ساختار نامعتبر");
        return;
      }
      updatePlan(plan.id, {
        name: p.name || plan.name,
        date: p.date || plan.date,
        enabled: p.enabled ?? plan.enabled,
        autoLoginAt: p.autoLoginAt || plan.autoLoginAt,
        autoRefreshAt: p.autoRefreshAt || plan.autoRefreshAt,
        orders: p.orders.map((o: Partial<ScheduledOrder>, i: number) => ({
          id: plan.orders[i]?.id ?? `imp-${Date.now()}-${i}`,
          accountId: o.accountId ?? "",
          symbolIsin: o.symbolIsin ?? "",
          side: o.side === 1 ? (1 as const) : (0 as const),
          mode: o.mode === "totalValue" ? "totalValue" : "quantity",
          quantity: String(o.quantity ?? "100"),
          totalValue: String(o.totalValue ?? "10000000"),
          time: o.time ?? "08:45:00.000",
        })),
      });
      setJsonFeedback("✅ اعمال شد");
      setJsonText("");
    } catch (e) {
      setJsonFeedback(`❌ ${e instanceof Error ? e.message : "خطا"}`);
    }
  };

  const clearPlanLogs = useScheduleStore((s) => s.clearPlanLogs);

  /* ═══ Collapsed ═══ */
  if (plan.collapsed) {
    return (
      <div
        className={`flex flex-wrap items-center gap-3 rounded-xl border bg-slate-900 p-3 ${
          plan.enabled
            ? isToday
              ? "border-green-800/60"
              : "border-slate-700"
            : "border-slate-800 opacity-60"
        }`}
      >
        <button
          type="button"
          className="flex h-7 w-7 items-center justify-center rounded-lg bg-slate-800 text-lg font-bold text-blue-300 hover:bg-slate-700"
          onClick={() => toggleCollapsed(plan.id)}
          title="باز کردن"
        >
          +
        </button>
        <div className="flex flex-1 flex-wrap items-center gap-3">
          <span className="font-semibold text-slate-200">{plan.name}</span>
          <span className="font-mono text-xs text-slate-400">{plan.date}</span>
          {isToday && (
            <span className="rounded bg-green-900/40 px-2 py-0.5 text-[10px] text-green-300">
              امروز
            </span>
          )}
          {!plan.enabled && (
            <span className="rounded bg-slate-800 px-2 py-0.5 text-[10px] text-slate-500">
              غیرفعال
            </span>
          )}
        </div>
        <span className="text-xs text-slate-400">
          {total === 0 ? "بدون سفارش" : `${fired}/${total} ارسال`}
        </span>
      </div>
    );
  }

  /* ═══ Expanded ═══ */
  return (
    <div
      className={`space-y-3 rounded-xl border bg-slate-900 p-3 ${
        plan.enabled
          ? isToday
            ? "border-green-800/60"
            : "border-blue-900/60"
          : "border-slate-800 opacity-80"
      }`}
    >
      {/* Header */}
      <div className="flex flex-wrap items-end gap-2">
        <button
          type="button"
          className="flex h-7 w-7 items-center justify-center rounded-lg bg-slate-800 text-lg font-bold text-blue-300 hover:bg-slate-700"
          onClick={() => toggleCollapsed(plan.id)}
          title="بستن"
        >
          −
        </button>

        <div className="flex-1">
          <label className="mb-1 block text-xs text-slate-400">نام برنامه</label>
          <input
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-2 py-1.5 text-sm"
            value={plan.name}
            onChange={(e) => updatePlan(plan.id, { name: e.target.value })}
          />
        </div>

        <div>
          <label className="mb-1 block text-xs text-slate-400">تاریخ</label>
          <input
            type="date"
            className="rounded-lg border border-slate-700 bg-slate-950 px-2 py-1.5 font-mono text-xs"
            value={plan.date}
            onChange={(e) => updatePlan(plan.id, { date: e.target.value })}
          />
        </div>

        <label className="flex cursor-pointer items-center gap-1 text-xs text-blue-300">
          <input
            type="checkbox"
            checked={plan.enabled}
            onChange={(e) => updatePlan(plan.id, { enabled: e.target.checked })}
          />
          فعال
        </label>

        <button
          type="button"
          className="rounded border border-red-900 px-2 py-1 text-xs text-red-400 hover:bg-red-950"
          onClick={() => {
            if (confirm(`حذف "${plan.name}"؟`)) removePlan(plan.id);
          }}
        >
          ✕
        </button>
      </div>

      {/* Plan status */}
      {planMessage && (
        <div className="rounded-lg border border-blue-900/40 bg-blue-950/20 px-3 py-2 text-xs text-blue-200">
          {planMessage}
        </div>
      )}

      {/* Times */}
      <div className="grid grid-cols-2 gap-3 rounded-lg bg-slate-950 p-3">
        <div>
          <label className="mb-1 block text-xs text-slate-400">
            ساعت لاگین این برنامه
          </label>
          <input
            type="text"
            className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 font-mono text-sm"
            value={plan.autoLoginAt}
            onChange={(e) =>
              updatePlan(plan.id, { autoLoginAt: e.target.value })
            }
            placeholder="08:30:00"
          />
        </div>
        <div>
          <label className="mb-1 block text-xs text-slate-400">
            ساعت رفرش قیمت‌ها
          </label>
          <input
            type="text"
            className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 font-mono text-sm"
            value={plan.autoRefreshAt}
            onChange={(e) =>
              updatePlan(plan.id, { autoRefreshAt: e.target.value })
            }
            placeholder="08:40:00"
          />
        </div>
      </div>

      {/* Plan actions: duplicate + export + import */}
      <div className="flex flex-wrap items-center gap-2 rounded-lg bg-slate-950 p-2">
        <span className="text-xs text-slate-400">کپی برای:</span>
        <input
          type="date"
          className="rounded-lg border border-slate-700 bg-slate-900 px-2 py-1 font-mono text-xs"
          value={dupDate}
          onChange={(e) => setDupDate(e.target.value)}
        />
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-2 py-1 text-xs text-blue-300 hover:bg-slate-700"
          onClick={() => duplicatePlan(plan.id, dupDate)}
        >
          کپی کن
        </button>

        <div className="mr-auto flex gap-2">
          <button
            type="button"
            className="rounded-lg bg-slate-800 px-2 py-1 text-xs text-blue-300 hover:bg-slate-700"
            onClick={handleExport}
          >
            📤 خروجی JSON
          </button>
          <button
            type="button"
            className="rounded-lg bg-slate-800 px-2 py-1 text-xs text-blue-300 hover:bg-slate-700"
            onClick={() => setJsonOpen((v) => !v)}
          >
            📥 ورودی
          </button>
        </div>
      </div>

      {jsonOpen && (
        <div className="rounded-lg border border-slate-700 bg-slate-950 p-2">
          <textarea
            rows={5}
            className="w-full rounded-lg border border-slate-700 bg-slate-900 px-2 py-1.5 font-mono text-[11px]"
            value={jsonText}
            onChange={(e) => setJsonText(e.target.value)}
            placeholder='{"version":2,"plan":{...}}'
          />
          <div className="mt-2 flex gap-2">
            <button
              type="button"
              className="rounded-lg bg-green-600 px-2 py-1 text-xs font-semibold text-slate-950 hover:bg-green-500"
              onClick={handleImport}
            >
              اعمال
            </button>
            <button
              type="button"
              className="rounded-lg bg-slate-800 px-2 py-1 text-xs text-blue-300 hover:bg-slate-700"
              onClick={handleExport}
            >
              بارگذاری خروجی فعلی
            </button>
          </div>
          {jsonFeedback && (
            <div className="mt-1 text-[11px] text-blue-300">{jsonFeedback}</div>
          )}
        </div>
      )}

      {/* Orders */}
      <div className="flex items-center justify-between text-xs text-slate-400">
        <span>
          سفارش‌ها ({plan.orders.length})
          {fired > 0 && (
            <span className="mr-2 text-green-400">({fired} ارسال شده)</span>
          )}
        </span>
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-2 py-1 text-blue-300 hover:bg-slate-700 disabled:opacity-50"
          onClick={() =>
            addOrder(plan.id, symbols[0]?.symbolIsin, accounts[0]?.id)
          }
          disabled={symbols.length === 0 || accounts.length === 0}
        >
          + افزودن سفارش
        </button>
      </div>

      {plan.orders.length === 0 && (
        <div className="rounded-lg bg-slate-950 p-3 text-center text-xs text-slate-500">
          سفارشی نداره
        </div>
      )}

      {plan.orders.map((o, idx) => (
        <OrderRow
          key={o.id}
          planId={plan.id}
          order={o}
          index={idx}
          fired={planState?.firedOrderIds.includes(o.id) ?? false}
        />
      ))}

      {/* Plan-specific log box */}
      <div className="rounded-lg border border-slate-700 bg-slate-950 p-2">
        <div className="mb-1 flex items-center justify-between text-[11px] text-slate-400">
          <span>لاگ این برنامه ({planLogs.length})</span>
          <button
            type="button"
            className="rounded bg-slate-800 px-2 py-0.5 text-blue-300 hover:bg-slate-700"
            onClick={() => clearPlanLogs(plan.id)}
          >
            پاک کن
          </button>
        </div>
        <div className="max-h-40 overflow-auto font-mono text-[11px] leading-5">
          {planLogs.length === 0 && (
            <div className="text-slate-600">لاگی نیست.</div>
          )}
          {planLogs.map((l, i) => (
            <div
              key={i}
              className={
                l.type === "ok"
                  ? "text-green-400"
                  : l.type === "err"
                    ? "text-red-400"
                    : l.type === "send"
                      ? "text-yellow-400"
                      : "text-blue-300"
              }
            >
              <span className="text-slate-500">{l.t}</span> {l.msg}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

/* ══════════════════════════════════════════════ */
function OrderRow({
  planId,
  order,
  index,
  fired,
}: {
  planId: string;
  order: ScheduledOrder;
  index: number;
  fired: boolean;
}) {
  const updateOrder = useScheduleStore((s) => s.updateOrder);
  const removeOrder = useScheduleStore((s) => s.removeOrder);
  const accounts = useAccountsStore((s) => s.accounts);
  const symbols = useSymbolsStore((s) => s.symbols);

  const sym = symbols.find((s) => s.symbolIsin === order.symbolIsin);
  const account = accounts.find((a) => a.id === order.accountId);

  return (
    <div
      className={`space-y-2 rounded-lg border p-2 ${
        fired
          ? "border-green-900/60 bg-green-950/10"
          : "border-slate-700 bg-slate-950"
      }`}
    >
      <div className="flex items-center justify-between text-xs">
        <span className="text-slate-500">
          #{index + 1}
          {fired && <span className="mr-2 text-green-400">✓ ارسال شد</span>}
        </span>
        <span className="font-semibold text-blue-300">
          {sym?.symbolName ?? "—"} — {account?.name ?? "—"}
        </span>
        <button
          type="button"
          className="rounded border border-red-900 px-2 py-0.5 text-red-400 hover:bg-red-950"
          onClick={() => removeOrder(planId, order.id)}
        >
          ✕
        </button>
      </div>

      <div className="grid grid-cols-3 gap-2">
        <select
          className="rounded-lg border border-slate-700 bg-slate-900 px-2 py-1 text-xs"
          value={order.accountId}
          onChange={(e) =>
            updateOrder(planId, order.id, { accountId: e.target.value })
          }
        >
          <option value="">— حساب —</option>
          {accounts.map((a) => (
            <option key={a.id} value={a.id}>
              {a.name}
            </option>
          ))}
        </select>

        <select
          className="rounded-lg border border-slate-700 bg-slate-900 px-2 py-1 text-xs"
          value={order.symbolIsin}
          onChange={(e) =>
            updateOrder(planId, order.id, { symbolIsin: e.target.value })
          }
        >
          <option value="">— نماد —</option>
          {symbols.map((s) => (
            <option key={s.symbolIsin} value={s.symbolIsin}>
              {s.symbolName}
            </option>
          ))}
        </select>

        <select
          className="rounded-lg border border-slate-700 bg-slate-900 px-2 py-1 text-xs"
          value={order.side}
          onChange={(e) =>
            updateOrder(planId, order.id, {
              side: Number(e.target.value) as 0 | 1,
            })
          }
        >
          <option value={0}>خرید</option>
          <option value={1}>فروش</option>
        </select>
      </div>

      <div className="grid grid-cols-2 gap-2">
        <div>
          <div className="mb-1 flex gap-2 text-xs">
            <label className="flex cursor-pointer items-center gap-1">
              <input
                type="radio"
                checked={order.mode === "quantity"}
                onChange={() =>
                  updateOrder(planId, order.id, { mode: "quantity" })
                }
              />
              تعداد
            </label>
            <label className="flex cursor-pointer items-center gap-1">
              <input
                type="radio"
                checked={order.mode === "totalValue"}
                onChange={() =>
                  updateOrder(planId, order.id, { mode: "totalValue" })
                }
              />
              مبلغ کل
            </label>
          </div>
          {order.mode === "quantity" ? (
            <input
              type="number"
              className="w-full rounded-lg border border-slate-700 bg-slate-900 px-2 py-1 text-xs"
              value={order.quantity}
              onChange={(e) =>
                updateOrder(planId, order.id, { quantity: e.target.value })
              }
              placeholder="تعداد"
            />
          ) : (
            <input
              type="number"
              className="w-full rounded-lg border border-slate-700 bg-slate-900 px-2 py-1 text-xs"
              value={order.totalValue}
              onChange={(e) =>
                updateOrder(planId, order.id, { totalValue: e.target.value })
              }
              placeholder="مبلغ (ریال)"
            />
          )}
        </div>

        <div>
          <label className="mb-1 block text-xs text-slate-400">زمان</label>
          <input
            type="text"
            className="w-full rounded-lg border border-slate-700 bg-slate-900 px-2 py-1 font-mono text-xs"
            value={order.time}
            onChange={(e) =>
              updateOrder(planId, order.id, { time: e.target.value })
            }
          />
        </div>
      </div>

      <div className="flex flex-wrap gap-1">
        {TIME_PRESETS.map((t) => (
          <button
            key={t}
            type="button"
            className="rounded border border-slate-700 bg-slate-800 px-2 py-0.5 font-mono text-[10px] text-blue-300 hover:bg-slate-700"
            onClick={() => updateOrder(planId, order.id, { time: t })}
          >
            {t}
          </button>
        ))}
      </div>

      <div className="text-[11px] text-slate-500">
        {order.side === 0 ? "🔵 قیمت = سقف مجاز" : "🔴 قیمت = کف مجاز"}
        {order.mode === "totalValue" && " — تعداد خودکار"}
      </div>
    </div>
  );
}