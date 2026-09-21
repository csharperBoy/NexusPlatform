import { useState } from "react";
import { useScheduleStore, useServerClockStore, useAccountsStore } from "../../stores";
import { PlanCard } from "../../components";
import { todayDateKey, tomorrowDateKey } from "../../utils";
import { useServerClock } from "../../hooks";

export function ScheduledOrdersPage() {
  const enabled = useScheduleStore((s) => s.enabled);
  const setEnabled = useScheduleStore((s) => s.setEnabled);
  const plans = useScheduleStore((s) => s.plans);
  const addPlan = useScheduleStore((s) => s.addPlan);
  const exportAll = useScheduleStore((s) => s.exportAll);
  const importAll = useScheduleStore((s) => s.importAll);
  const runtime = useScheduleStore((s) => s.runtime);
  const resetRuntime = useScheduleStore((s) => s.resetRuntime);

  const clockStore = useServerClockStore();
  const firstToken = useAccountsStore(
    (s) => s.accounts.find((a) => a.token?.trim())?.token ?? "",
  );
  const { sync: runClockSync } = useServerClock(firstToken);

  const [showBulkJson, setShowBulkJson] = useState(false);
  const [jsonText, setJsonText] = useState("");
  const [feedback, setFeedback] = useState<{ ok: boolean; msg: string } | null>(
    null,
  );

  const handleExportAll = () => {
    const json = exportAll();
    navigator.clipboard.writeText(json).then(
      () => setFeedback({ ok: true, msg: "✅ همه برنامه‌ها کپی شد" }),
      () => {
        setJsonText(json);
        setShowBulkJson(true);
        setFeedback({ ok: true, msg: "کپی نشد — دستی بردار" });
      },
    );
  };

  const handleImportAll = () => {
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
  const today = todayDateKey();
  const activeCount = sortedPlans.filter(
    (p) => p.enabled && p.date === today && p.orders.length > 0,
  ).length;

  const totalLeadTime = clockStore.getTotalLeadTimeMs();

  return (
    <div className="space-y-4">
      {/* Global bar */}
      <div className="space-y-3 rounded-xl border border-slate-800 bg-slate-900 p-4">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <h1 className="text-lg font-bold text-slate-200">
            برنامه‌ریزی سفارشات (ارسال سرخطی سر وقت)
          </h1>
          <button
            type="button"
            className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
            onClick={resetRuntime}
          >
            ↺ ریست وضعیت
          </button>
        </div>

        <label className="flex cursor-pointer items-center gap-2 text-sm text-blue-300">
          <input
            type="checkbox"
            checked={enabled}
            onChange={(e) => setEnabled(e.target.checked)}
          />
          <span className="font-semibold">فعال‌سازی زمان‌بند</span>
          <span className="text-[11px] text-slate-500">
            (تب مرورگر باید باز بماند)
          </span>
        </label>

        {enabled && (
          <div className="rounded-lg border border-blue-900/50 bg-blue-950/20 p-3 text-xs">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <span className="text-blue-300">
                وضعیت کلی:{" "}
                <b className="text-blue-200">{statusLabel(runtime.status)}</b>
              </span>
              {activeCount > 0 && (
                <span className="text-slate-400">
                  {activeCount} برنامه فعال برای امروز
                </span>
              )}
            </div>
            {runtime.message && (
              <div className="mt-1 text-slate-400">{runtime.message}</div>
            )}
          </div>
        )}

        {/* Live Network Latency & Clock Offset Card */}
        <div className="rounded-lg border border-emerald-900/50 bg-emerald-950/20 p-3.5 text-xs text-slate-300 space-y-2">
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-emerald-900/30 pb-2">
            <div className="flex items-center gap-2 font-semibold text-emerald-400">
              <span className="inline-block h-2 w-2 rounded-full bg-emerald-400 animate-pulse" />
              تنظیمات تاخیر شبکه و همگام‌سازی ساعت سرور
            </div>
            <button
              type="button"
              className="rounded bg-emerald-900/60 px-2.5 py-1 text-[11px] font-medium text-emerald-200 hover:bg-emerald-800 disabled:opacity-50"
              onClick={() => runClockSync(3)}
              disabled={clockStore.busy}
            >
              {clockStore.busy ? "در حال سنجش..." : "⚡ سنجش مجدد تاخیر شبکه"}
            </button>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-4 gap-3 pt-1">
            <div className="rounded bg-slate-950/60 p-2 border border-slate-800">
              <div className="text-[11px] text-slate-400">اختلاف ساعت (Offset)</div>
              <div className="text-sm font-bold text-emerald-300 dir-ltr text-right">
                {clockStore.offset >= 0 ? `+${clockStore.offset}` : clockStore.offset} ms
              </div>
            </div>

            <div className="rounded bg-slate-950/60 p-2 border border-slate-800">
              <div className="text-[11px] text-slate-400">تاخیر رفت‌وبرگشت (RTT)</div>
              <div className="text-sm font-bold text-sky-300 dir-ltr text-right">
                {clockStore.rtt} ms (یک‌طرفه: {clockStore.oneWayLatency} ms)
              </div>
            </div>

            <div className="rounded bg-slate-950/60 p-2 border border-slate-800">
              <div className="text-[11px] text-slate-400">پیش‌افتادگی دستی (اضافی)</div>
              <div className="flex items-center gap-1 mt-0.5">
                <input
                  type="number"
                  min="0"
                  max="2000"
                  step="10"
                  className="w-20 rounded border border-slate-700 bg-slate-900 px-2 py-0.5 text-xs text-slate-100 dir-ltr"
                  value={clockStore.manualLeadTimeMs}
                  onChange={(e) => clockStore.setManualLeadTimeMs(Number(e.target.value) || 0)}
                />
                <span className="text-[11px] text-slate-400">ms</span>
              </div>
            </div>

            <div className="rounded bg-emerald-950/40 p-2 border border-emerald-800/60">
              <div className="text-[11px] text-emerald-300 font-semibold">پیش‌افتادگی کل ارسال</div>
              <div className="text-sm font-black text-emerald-200 dir-ltr text-right">
                {totalLeadTime} ms
              </div>
            </div>
          </div>

          <div className="text-[11px] text-slate-400 leading-relaxed pt-1">
            💡 درخواست سفارش به اندازه <b>{totalLeadTime} میلی‌ثانیه زودتر</b> ارسال می‌شود تا با احتساب زمان سفر در شبکه ({clockStore.oneWayLatency}ms) و پیش‌افتادگی دستی ({clockStore.manualLeadTimeMs}ms)، دقیقا سر رأس زمان تنظیم‌شده به سرور کارگزاری برسد.
          </div>
        </div>

        {/* Global actions */}
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
            onClick={handleExportAll}
            disabled={plans.length === 0}
            title="خروجی همه برنامه‌ها به‌صورت یکجا"
          >
            📋 خروجی همه
          </button>
          <button
            type="button"
            className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
            onClick={() => setShowBulkJson((v) => !v)}
            title="وارد کردن چند برنامه به‌صورت یکجا"
          >
            📥 ورودی گروهی
          </button>
        </div>

        {showBulkJson && (
          <div className="rounded-lg border border-slate-700 bg-slate-950 p-3">
            <label className="mb-1 block text-xs text-slate-400">
              JSON چند برنامه (خروجی «خروجی همه»)
            </label>
            <textarea
              rows={5}
              className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 font-mono text-xs"
              value={jsonText}
              onChange={(e) => setJsonText(e.target.value)}
              placeholder='{"version":2,"plans":[...]}'
            />
            <div className="mt-2 flex gap-2">
              <button
                type="button"
                className="rounded-lg bg-green-600 px-3 py-1.5 text-xs font-semibold text-slate-950 hover:bg-green-500"
                onClick={handleImportAll}
              >
                افزودن به لیست
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
      </div>

      {/* Plans */}
      {sortedPlans.length === 0 && (
        <div className="rounded-xl border border-slate-800 bg-slate-900 p-6 text-center text-xs text-slate-500">
          هیچ برنامه‌ای ندارید. یکی از دکمه‌های بالا رو بزنید.
        </div>
      )}

      {sortedPlans.map((p) => (
        <PlanCard key={p.id} plan={p} />
      ))}
    </div>
  );
}

function statusLabel(s: string): string {
  return (
    {
      idle: "غیرفعال",
      waiting: "در انتظار",
      "logging-in": "در حال لاگین",
      refreshing: "در حال رفرش",
      "waiting-fire": "منتظر شلیک",
      firing: "در حال شلیک",
      done: "تمام",
      cancelled: "کنسل شده",
      error: "خطا",
    }[s] ?? s
  );
}
