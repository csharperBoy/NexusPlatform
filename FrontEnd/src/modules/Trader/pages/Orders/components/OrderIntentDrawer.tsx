import React, { useEffect, useState } from "react";
import { orderIntentApi } from "../../../api/ordersApi";
import type { OrderIntentView } from "../../../models";
import { PaperBadge, SideBadge, StateBadge } from "./OrderBadges";

interface Props {
  isOpen: boolean;
  intentId: string | null;
  onClose: () => void;
}

export const OrderIntentDrawer: React.FC<Props> = ({
  isOpen,
  intentId,
  onClose,
}) => {
  const [intent, setIntent] = useState<OrderIntentView | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen || !intentId) return;
    setLoading(true);
    setError(null);
    setIntent(null);
    (async () => {
      try {
        const data = await orderIntentApi.GetById(intentId);
        setIntent(data);
      } catch (e) {
        setError(e instanceof Error ? e.message : "خطا در دریافت جزئیات");
      } finally {
        setLoading(false);
      }
    })();
  }, [isOpen, intentId]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex justify-end bg-black/50" onClick={onClose}>
      <div
        className="h-full w-full max-w-xl overflow-y-auto bg-white p-6 shadow-xl dark:bg-gray-800"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="mb-4 flex items-center justify-between border-b border-gray-200 pb-3 dark:border-gray-700">
          <h3 className="text-lg font-bold text-gray-900 dark:text-white">
            جزئیات تصمیم اجرا
          </h3>
          <button
            type="button"
            onClick={onClose}
            className="text-2xl leading-none text-gray-500 hover:text-gray-700 dark:text-gray-400"
          >
            ×
          </button>
        </div>

        {loading && <p className="text-sm text-gray-500">در حال بارگذاری...</p>}
        {error && (
          <div className="rounded bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
            ⚠️ {error}
          </div>
        )}

        {intent && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-3 text-sm">
              <Field label="نماد" value={intent.symbolName ?? intent.isin} mono />
              <Field label="حساب" value={intent.accountName ?? "—"} />
              <Field label="جهت" value={<SideBadge side={intent.side} sideName={intent.sideName} />} />
              <Field label="حالت" value={<PaperBadge isPaper={intent.isPaper} />} />
              <Field
                label="وضعیت"
                value={
                  <StateBadge
                    state={intent.state}
                    stateName={intent.stateName}
                    variant="intent"
                  />
                }
              />
              <Field label="نوع سفارش" value={intent.orderModeName} />
              <Field
                label="قیمت × تعداد"
                value={`${intent.price.toLocaleString("en-US")} × ${intent.quantity.toLocaleString("en-US")}`}
                mono
              />
              <Field
                label="SL / TP"
                value={
                  intent.stopLossPrice || intent.takeProfitPrice
                    ? `${intent.stopLossPrice?.toLocaleString("en-US") ?? "—"} / ${intent.takeProfitPrice?.toLocaleString("en-US") ?? "—"}`
                    : "—"
                }
                mono
              />
              <Field
                label="زمان ایجاد"
                value={new Date(intent.createdAt).toLocaleString("fa-IR")}
              />
              <Field
                label="زمان ارسال"
                value={
                  intent.dispatchedAt
                    ? new Date(intent.dispatchedAt).toLocaleString("fa-IR")
                    : "—"
                }
              />
            </div>

            {intent.reason && (
              <div className="rounded bg-slate-100 p-2 text-xs dark:bg-slate-700">
                <div className="text-[10px] text-gray-500">دلیل:</div>
                <div className="mt-0.5 text-gray-900 dark:text-gray-100">{intent.reason}</div>
              </div>
            )}

            {intent.failureReason && (
              <div className="rounded bg-red-50 p-3 text-xs dark:bg-red-900/30">
                <div className="text-[10px] font-bold text-red-600 dark:text-red-400">
                  {intent.state === 5 ? "🚫 مسدود توسط Risk:" : "❌ خطا:"}
                </div>
                <div className="mt-0.5 text-red-700 dark:text-red-300">
                  {intent.failureReason}
                </div>
              </div>
            )}

            {(intent.sourceSignalId || intent.sourceEvaluationId) && (
              <div className="space-y-1 rounded bg-slate-100 p-2 text-[11px] dark:bg-slate-700">
                {intent.sourceSignalId && (
                  <div>
                    <span className="text-gray-500">Signal Id:</span>{" "}
                    <span className="font-mono">{intent.sourceSignalId}</span>
                  </div>
                )}
                {intent.sourceEvaluationId && (
                  <div>
                    <span className="text-gray-500">Evaluation Id:</span>{" "}
                    <span className="font-mono">{intent.sourceEvaluationId}</span>
                  </div>
                )}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

const Field: React.FC<{ label: string; value: React.ReactNode; mono?: boolean }> = ({
  label,
  value,
  mono,
}) => (
  <div>
    <div className="text-[10px] text-gray-500 dark:text-gray-400">{label}</div>
    <div className={`mt-0.5 ${mono ? "font-mono text-xs" : ""} text-gray-900 dark:text-gray-100`}>
      {value}
    </div>
  </div>
);