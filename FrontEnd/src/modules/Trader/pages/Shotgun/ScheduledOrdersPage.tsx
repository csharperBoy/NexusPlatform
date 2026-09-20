import { useState } from "react";
import {
  useScheduleStore,
  useAccountsStore,
  useSymbolsStore,
} from "../../stores";
import { LogBox } from "../../components";
import { TIME_PRESETS } from "../../constants";
import { todayDateKey, tomorrowDateKey } from "../../utils";
import type { SchedulePlan, ScheduledOrder } from "../../models";

export function ScheduledOrdersPage() {
  const enabled = useScheduleStore((s) => s.enabled);
  const setEnabled = useScheduleStore((s) => s.setEnabled);
  const autoLoginAt = useScheduleStore((s) => s.autoLoginAt);
  const autoRefreshAt = useScheduleStore((s) => s.autoRefreshAt);
  const setAutoLoginAt = useScheduleStore((s) => s.setAutoLoginAt);
  const setAutoRefreshAt = useScheduleStore((s) => s.setAutoRefreshAt);
  const plans = useScheduleStore((s) => s.plans);
  const addPlan = useScheduleStore((s) => s.addPlan);
  const exportAll = useScheduleStore((s) => s.exportAll);
  const importAll = useScheduleStore((s) => s.importAll);
  const runtime = useScheduleStore((s) => s.runtime);
  const resetRuntime = useScheduleStore((s) => s.resetRuntime);

  const [showJson, setShowJson] = useState(false);
  const [jsonText, setJsonText] = useState("");
  const [feedback, setFeedback] = useState<{ ok: boolean; msg: string } | null>(
    null,
  );

  const handleExport = () => {
    const json = exportAll();
    navigator.clipboard.writeText(json).then(
      () => setFeedback({ ok: true, msg: "✅ JSON کپی شد" }),
      () => {
        setJsonText(json);
        setShowJson(true);
        setFeedback({ ok: true, msg: "کپی نشد — دستی از باکس بردار" });
      },
    );
  };

  const handleImport = () => {
    if (!jsonText.trim()) {
      setFeedback({ ok: false, msg: "اول JSON رو پیست کن" });
      return;
    }
    const r = importAll(jsonText);
    if (r.ok) {
      setFeedback({ ok: true, msg: `✅ ${r.count} برنامه اضافه شد` });
      setJsonText("");
    } else {
      setFeedback({ ok: false, msg: `❌ ${r.error}` });
    }
  };

  const sortedPlans = [...plans].sort((a, b) => a.date.localeCompare(b.date));

  return (
    <div className="space-y-4 rounded-xl border border-slate-800 bg-slate-900 p-4">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-bold text-slate-200">برنامه‌ریزی سفارشات</h1>
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
          onClick={resetRuntime}
        >
          ↺ ریست وضعیت
        </button>
      </div>

      {/* Enable */}
      <label className="flex cursor-pointer items-center gap-2 rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-blue-300">
        <input
          type="checkbox"
          checked={enabled}
          onChange={(e) => setEnabled(e.target.checked)}
        />
        فعال‌سازی زمان‌بند (تب باید باز بمونه)
      </label>

      {/* Runtime status */}
      {enabled && (
        <div className="rounded-lg border border-blue-900/50 bg-blue-950/20 p-3 text-xs">
          <div className="mb-1 flex items-center justify-between">
            <span className="text-blue-300">وضعیت:</span>
            <span className="font-semibold text-blue-200">
              {statusLabel(runtime.status)}
            </span>
          </div>
          {runtime.message && (
            <div className="text-slate-400">{runtime.message}</div>
          )}
        </div>
      )}

      {/* Auto times */}
      <div className="grid grid-cols-2 gap-3 rounded-lg border border-slate-700 bg-slate-950 p-3">
        <div>
          <label className="mb-1 block text-xs text-slate-400">
            ساعت لاگین خودکار
          </label>
          <input
            type="text"
            className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 font-mono text-sm"
            value={autoLoginAt}
            onChange={(e) => setAutoLoginAt(e.target.value)}
            placeholder="08:30:00"
          />
        </div>
        <div>
          <label className="mb-1 block text-xs text-slate-400">
            ساعت بروزرسانی قیمت‌ها
          </label>
          <input
            type="text"
            className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 font-mono text-sm"
            value={autoRefreshAt}
            onChange={(e) => setAutoRefreshAt(e.target.value)}
            placeholder="08:40:00"
          />
        </div>
      </div>

      {/* Actions */}
      <div className="flex flex-wrap gap-2">
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
          onClick={() => addPlan(todayDateKey(), "برنامه امروز")}
        >
          + برنامه امروز
        </button>
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
          onClick={() => addPlan(tomorrowDateKey(), "برنامه فردا")}
        >
          + برنامه فردا
        </button>
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700 disabled:opacity-50"
          onClick={handleExport}
          disabled={plans.length === 0}
        >
          📋 Export JSON
        </button>
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
          onClick={() => setShowJson((v) => !v)}
        >
          📥 Import
        </button>
      </div>

      {/* JSON textarea */}
      {showJson && (
        <div className="rounded-lg border border-slate-700 bg-slate-950 p-3">
          <label className="mb-1 block text-xs text-slate-400">
            JSON برنامه‌ها رو پیست کن
          </label>
          <textarea
            rows={6}
            className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 font-mono text-xs"
            value={jsonText}
            onChange={(e) => setJsonText(e.target.value)}
            placeholder='{"version":1,"plans":[...]}'
          />
          <div className="mt-2 flex gap-2">
            <button
              type="button"
              className="rounded-lg bg-green-600 px-3 py-1.5 text-xs font-semibold text-slate-950 hover:bg-green-500"
              onClick={handleImport}
            >
              افزودن به لیست
            </button>
            <button
              type="button"
              className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
              onClick={() => setJsonText(exportAll())}
            >
              بارگذاری خروجی فعلی
            </button>
          </div>
        </div>
      )}

      {feedback && (
        <div
          className={`rounded-lg p-2 text-xs ${
            feedback.ok
              ? "bg-green-950/30 text-green-400"
              : "bg-red-950/30 text-red-400"
          }`}
        >
          {feedback.msg}
        </div>
      )}

      {/* Plans list */}
      {sortedPlans.length === 0 && (
        <div className="rounded-lg bg-slate-950 p-4 text-center text-xs text-slate-500">
          هیچ برنامه‌ای ندارید. یکی از دکمه‌های بالا رو بزنید.
        </div>
      )}

      {sortedPlans.map((p) => (
        <PlanCard key={p.id} plan={p} />
      ))}

      <LogBox />
    </div>
  );
}

/* ══════════════════════════════════════════════ */
function PlanCard({ plan }: { plan: SchedulePlan }) {
  const updatePlan = useScheduleStore((s) => s.updatePlan);
  const removePlan = useScheduleStore((s) => s.removePlan);
  const duplicatePlan = useScheduleStore((s) => s.duplicatePlan);
  const addOrder = useScheduleStore((s) => s.addOrder);
  const accounts = useAccountsStore((s) => s.accounts);
  const symbols = useSymbolsStore((s) => s.symbols);

  const [dupDate, setDupDate] = useState(tomorrowDateKey());

  return (
    <div className="space-y-3 rounded-xl border border-blue-900/60 bg-slate-950 p-3">
      {/* Header */}
      <div className="flex flex-wrap items-end gap-2">
        <div className="flex-1">
          <label className="mb-1 block text-xs text-slate-400">نام برنامه</label>
          <input
            className="w-full rounded-lg border border-slate-700 bg-slate-900 px-2 py-1.5 text-sm"
            value={plan.name}
            onChange={(e) => updatePlan(plan.id, { name: e.target.value })}
          />
        </div>
        <div>
          <label className="mb-1 block text-xs text-slate-400">تاریخ</label>
          <input
            type="date"
            className="rounded-lg border border-slate-700 bg-slate-900 px-2 py-1.5 font-mono text-xs"
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

      {/* Duplicate */}
      <div className="flex flex-wrap items-center gap-2 rounded-lg bg-slate-900 p-2">
        <span className="text-xs text-slate-400">کپی برای:</span>
        <input
          type="date"
          className="rounded-lg border border-slate-700 bg-slate-950 px-2 py-1 font-mono text-xs"
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
      </div>

      {/* Orders head */}
      <div className="flex items-center justify-between text-xs text-slate-400">
        <span>سفارش‌ها ({plan.orders.length})</span>
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
        <div className="rounded-lg bg-slate-900 p-2 text-center text-xs text-slate-500">
          سفارشی نداره
        </div>
      )}

      {plan.orders.map((o, idx) => (
        <OrderCard key={o.id} planId={plan.id} order={o} index={idx} />
      ))}
    </div>
  );
}

/* ══════════════════════════════════════════════ */
function OrderCard({
  planId,
  order,
  index,
}: {
  planId: string;
  order: ScheduledOrder;
  index: number;
}) {
  const updateOrder = useScheduleStore((s) => s.updateOrder);
  const removeOrder = useScheduleStore((s) => s.removeOrder);
  const accounts = useAccountsStore((s) => s.accounts);
  const symbols = useSymbolsStore((s) => s.symbols);

  const sym = symbols.find((s) => s.symbolIsin === order.symbolIsin);
  const account = accounts.find((a) => a.id === order.accountId);

  return (
    <div className="space-y-2 rounded-lg border border-slate-700 bg-slate-900 p-2">
      <div className="flex items-center justify-between text-xs">
        <span className="text-slate-500">#{index + 1}</span>
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
          className="rounded-lg border border-slate-700 bg-slate-950 px-2 py-1 text-xs"
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
          className="rounded-lg border border-slate-700 bg-slate-950 px-2 py-1 text-xs"
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
          className="rounded-lg border border-slate-700 bg-slate-950 px-2 py-1 text-xs"
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
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-2 py-1 text-xs"
              value={order.quantity}
              onChange={(e) =>
                updateOrder(planId, order.id, { quantity: e.target.value })
              }
              placeholder="تعداد"
            />
          ) : (
            <input
              type="number"
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-2 py-1 text-xs"
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
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-2 py-1 font-mono text-xs"
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

/* ══════════════════════════════════════════════ */
function statusLabel(s: string): string {
  return (
    {
      idle: "غیرفعال",
      "waiting-login": "منتظر لاگین",
      "logging-in": "در حال لاگین",
      "waiting-refresh": "منتظر رفرش",
      refreshing: "در حال رفرش",
      "waiting-fire": "منتظر شلیک",
      firing: "در حال شلیک",
      done: "تمام",
      cancelled: "کنسل شده",
      error: "خطا",
    }[s] ?? s
  );
}