import React, { useEffect, useState } from "react";
import { orderPresetApi } from "../../../api/orderPresetApi";
import type { ResolvedOrderPresetView } from "../../../models";

interface Props {
  isOpen: boolean;
  presetId: string | null;
  onClose: () => void;
}

export const ResolvePreviewDrawer: React.FC<Props> = ({ isOpen, presetId, onClose }) => {
  const [data, setData] = useState<ResolvedOrderPresetView | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = async () => {
    if (!presetId) return;
    setLoading(true);
    setError(null);
    try {
      const result = await orderPresetApi.Resolve(presetId);
      setData(result);
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در Resolve");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (!isOpen || !presetId) return;
    setData(null);
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isOpen, presetId]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex justify-end bg-black/50" onClick={onClose}>
      <div
        className="h-full w-full max-w-xl overflow-y-auto bg-white p-6 shadow-xl dark:bg-gray-800"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="mb-4 flex items-center justify-between border-b border-gray-200 pb-3 dark:border-gray-700">
          <h3 className="text-lg font-bold text-gray-900 dark:text-white">
            Resolve لحظه‌ای Preset
          </h3>
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={load}
              disabled={loading}
              className="rounded border border-gray-300 px-3 py-1 text-xs text-gray-700 hover:bg-gray-100 disabled:opacity-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
            >
              🔄 تازه‌سازی
            </button>
            <button
              type="button"
              onClick={onClose}
              className="text-2xl leading-none text-gray-500 hover:text-gray-700 dark:text-gray-400"
            >
              ×
            </button>
          </div>
        </div>

        {loading && <p className="text-sm text-gray-500">در حال فراخوانی API بروکر...</p>}
        {error && (
          <div className="rounded bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
            ⚠️ {error}
          </div>
        )}

        {data && (
          <div className="space-y-4">
            {/* Result */}
            <div className="rounded-lg border-2 border-blue-500 bg-blue-50 p-4 dark:bg-blue-900/20">
              <div className="grid grid-cols-2 gap-3 text-sm">
                <div>
                  <div className="text-[10px] text-gray-500">قیمت</div>
                  <div className="font-mono text-lg font-bold text-blue-700 dark:text-blue-300">
                    {data.price.toLocaleString("en-US")}
                  </div>
                  <div className="text-[10px] text-gray-500">{data.priceSource}</div>
                </div>
                <div>
                  <div className="text-[10px] text-gray-500">تعداد</div>
                  <div className="font-mono text-lg font-bold text-blue-700 dark:text-blue-300">
                    {data.quantity.toLocaleString("en-US")}
                  </div>
                  <div className="text-[10px] text-gray-500">{data.quantitySource}</div>
                </div>
              </div>
              <div className="mt-2 text-xs text-gray-600 dark:text-gray-400">
                {data.instrumentSymbolName} — {data.sideName}
              </div>
            </div>

            {data.warning && (
              <div className="rounded bg-amber-50 p-3 text-xs text-amber-700 dark:bg-amber-900/20 dark:text-amber-300">
                ⚠️ {data.warning}
              </div>
            )}

            {/* Instrument Range Context */}
            {(data.instrumentRangeUpPercent != null ||
              data.instrumentRangeDownPercent != null) && (
              <div>
                <h4 className="mb-2 text-xs font-bold text-gray-700 dark:text-gray-300">
                  بازه‌ی روزانه‌ی نماد
                </h4>
                <div className="grid grid-cols-2 gap-2 rounded border border-gray-200 p-3 text-xs dark:border-gray-700">
                  <div>
                    <div className="text-[10px] text-gray-500">سقف (٪)</div>
                    <div className="font-mono">
                      {data.instrumentRangeUpPercent != null
                        ? `+${data.instrumentRangeUpPercent}%`
                        : "بدون محدودیت"}
                    </div>
                  </div>
                  <div>
                    <div className="text-[10px] text-gray-500">کف (٪)</div>
                    <div className="font-mono">
                      {data.instrumentRangeDownPercent != null
                        ? `-${data.instrumentRangeDownPercent}%`
                        : "بدون محدودیت"}
                    </div>
                  </div>
                </div>
                {data.liveClosing != null && (
                  <p className="mt-1 text-[10px] text-gray-500">
                    بازه‌ی محاسبه‌شده:{" "}
                    <span className="font-mono">
                      {data.instrumentRangeDownPercent != null
                        ? Math.round(
                            data.liveClosing *
                              (1 - data.instrumentRangeDownPercent / 100),
                          ).toLocaleString("en-US")
                        : "∞"}
                    </span>
                    {" .. "}
                    <span className="font-mono">
                      {data.instrumentRangeUpPercent != null
                        ? Math.round(
                            data.liveClosing *
                              (1 + data.instrumentRangeUpPercent / 100),
                          ).toLocaleString("en-US")
                        : "∞"}
                    </span>
                  </p>
                )}
              </div>
            )}

            {/* Live API values */}
            <div>
              <h4 className="mb-2 text-xs font-bold text-gray-700 dark:text-gray-300">
                مقادیر خام از API
              </h4>
              <div className="grid grid-cols-2 gap-2 rounded border border-gray-200 p-3 text-xs dark:border-gray-700">
                <Row label="High Allowed" value={data.liveHighAllowed} />
                <Row label="Low Allowed" value={data.liveLowAllowed} />
                <Row label="Last Traded" value={data.liveLastTraded} />
                <Row label="Closing" value={data.liveClosing} />
                <Row label="First Traded" value={data.liveFirstTraded} />
              </div>
            </div>

            {data.usedAccountName && (
              <div className="text-[10px] text-gray-500">
                Session از: {data.usedAccountName} (id: {data.usedAccountId})
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

const Row: React.FC<{ label: string; value: number | null }> = ({ label, value }) => (
  <div>
    <div className="text-[10px] text-gray-500">{label}</div>
    <div className="font-mono">
      {value != null ? value.toLocaleString("en-US") : "—"}
    </div>
  </div>
);