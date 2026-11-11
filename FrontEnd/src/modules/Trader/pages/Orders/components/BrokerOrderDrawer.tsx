import React, { useEffect, useState } from "react";
import { brokerOrderApi } from "../../../api/ordersApi";
import type { BrokerOrderDetailView } from "../../../models";
import { PaperBadge, SideBadge, StateBadge } from "./OrderBadges";

interface Props {
  isOpen: boolean;
  orderId: string | null;
  onClose: () => void;
}

export const BrokerOrderDrawer: React.FC<Props> = ({
  isOpen,
  orderId,
  onClose,
}) => {
  const [detail, setDetail] = useState<BrokerOrderDetailView | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen || !orderId) return;
    setLoading(true);
    setError(null);
    setDetail(null);
    (async () => {
      try {
        const data = await brokerOrderApi.GetById(orderId);
        setDetail(data);
      } catch (e) {
        setError(e instanceof Error ? e.message : "خطا در دریافت جزئیات");
      } finally {
        setLoading(false);
      }
    })();
  }, [isOpen, orderId]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex justify-end bg-black/50" onClick={onClose}>
      <div
        className="h-full w-full max-w-xl overflow-y-auto bg-white p-6 shadow-xl dark:bg-gray-800"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="mb-4 flex items-center justify-between border-b border-gray-200 pb-3 dark:border-gray-700">
          <h3 className="text-lg font-bold text-gray-900 dark:text-white">
            جزئیات سفارش
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

        {detail && (
          <div className="space-y-4">
            {/* Summary */}
            <div className="grid grid-cols-2 gap-3 text-sm">
              <Field label="نماد" value={detail.summary.symbolName ?? detail.summary.isin} mono />
              <Field label="حساب" value={detail.summary.accountName ?? "—"} />
              <Field
                label="جهت"
                value={
                  <SideBadge
                    side={detail.summary.side}
                    sideName={detail.summary.sideName}
                  />
                }
              />
              <Field
                label="حالت"
                value={<PaperBadge isPaper={detail.summary.isPaper} />}
              />
              <Field
                label="وضعیت"
                value={
                  <StateBadge
                    state={detail.summary.state}
                    stateName={detail.summary.stateName}
                    variant="broker"
                  />
                }
              />
              <Field label="Broker Order Id" value={detail.summary.brokerOrderId ?? "—"} mono />
              <Field
                label="قیمت × تعداد"
                value={`${detail.summary.price.toLocaleString("en-US")} × ${detail.summary.quantity.toLocaleString("en-US")}`}
                mono
              />
              <Field
                label="اجرا شده"
                value={`${detail.summary.filledQuantity.toLocaleString("en-US")} (${detail.summary.filledRatio.toFixed(1)}%)`}
                mono
              />
              <Field
                label="زمان ارسال"
                value={new Date(detail.summary.submittedAt).toLocaleString("fa-IR")}
              />
              <Field
                label="آخرین Sync"
                value={
                  detail.summary.lastSyncedAt
                    ? new Date(detail.summary.lastSyncedAt).toLocaleString("fa-IR")
                    : "—"
                }
              />
            </div>

            {detail.summary.orderIntentId && (
              <div className="rounded bg-slate-100 p-2 text-xs dark:bg-slate-700">
                <span className="text-gray-500">OrderIntent Id:</span>{" "}
                <span className="font-mono">{detail.summary.orderIntentId}</span>
              </div>
            )}

            {/* Trades */}
            <div>
              <h4 className="mb-2 text-sm font-bold text-gray-800 dark:text-gray-200">
                معاملات ({detail.trades.length})
              </h4>
              {detail.trades.length === 0 ? (
                <p className="text-xs text-gray-500">هیچ معامله‌ای ثبت نشده</p>
              ) : (
                <div className="overflow-hidden rounded border border-gray-200 dark:border-gray-700">
                  <table className="w-full text-right text-xs">
                    <thead className="bg-gray-50 dark:bg-gray-700">
                      <tr>
                        <th className="p-2">#</th>
                        <th className="p-2">قیمت</th>
                        <th className="p-2">تعداد</th>
                        <th className="p-2">زمان</th>
                        <th className="p-2">وضعیت</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                      {detail.trades.map((t) => (
                        <tr key={t.id}>
                          <td className="p-2 font-mono">{t.tradeNumber}</td>
                          <td className="p-2 font-mono">{t.price.toLocaleString("en-US")}</td>
                          <td className="p-2 font-mono">{t.quantity.toLocaleString("en-US")}</td>
                          <td className="p-2">{new Date(t.tradeDate).toLocaleString("fa-IR")}</td>
                          <td className="p-2">
                            {t.isCanceled ? (
                              <span className="text-red-400">لغو شده</span>
                            ) : (
                              <span className="text-emerald-400">فعال</span>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

/* ═══════════ Helpers ═══════════ */

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