// src/modules/HR/pages/Sync/SyncPage.tsx
import React, { useState } from "react";
import { SyncApi } from "../../api/SyncApi";
import { SyncResult, entityLabels, SyncEntityKey } from "../../models/SyncModels";
import { AxiosError } from "axios";

export const SyncPage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [syncData, setSyncData] = useState<Record<string, SyncResult> | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleSync = async () => {
    setLoading(true);
    setError(null);
    setSyncData(null);

    try {
      const data = await SyncApi.syncWithIrisa();
      setSyncData(data);
    } catch (err) {
      // مدیریت خطاهای برگشتی از سرور
      if (err instanceof AxiosError && err.response) {
        // اگر سرور با BadRequest (یا هر خطای دیگه) جواب داده
        const errorMessage = err.response.data || "خطا در همگام‌سازی";
        setError(typeof errorMessage === "string" ? errorMessage : JSON.stringify(errorMessage));
      } else {
        setError("خطا در برقراری ارتباط با سرور.");
      }
    } finally {
      setLoading(false);
    }
  };

  // رندر نتیجه‌ی هر بخش
  const renderSyncResult = (syncResult: SyncResult) => (
    <div className="flex gap-4 text-sm">
      <span className="text-green-600">➕ افزوده: {syncResult.addedCount}</span>
      <span className="text-blue-600">✏️ بروزرسانی: {syncResult.updatedCount}</span>
      <span className="text-red-600">➖ حذف: {syncResult.deletedCount}</span>
    </div>
  );

  return (
    <div className="max-w-4xl mx-auto p-6 bg-white rounded-xl shadow-md">
      <h1 className="text-2xl font-bold text-gray-800 mb-6">همگام‌سازی با سیستم‌های اطلاعاتی</h1>

      <button
        onClick={handleSync}
        disabled={loading}
        className={`w-full py-3 px-4 rounded-lg text-white font-semibold transition ${
          loading
            ? "bg-gray-400 cursor-not-allowed"
            : "bg-blue-600 hover:bg-blue-700 active:scale-95"
        }`}
      >
        {loading ? "در حال همگام‌سازی..." : "شروع همگام‌سازی"}
      </button>

      {loading && (
        <div className="mt-6 flex justify-center">
          <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
        </div>
      )}

      {error && (
        <div className="mt-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700">
          <span className="font-semibold">خطا: </span> {error}
        </div>
      )}

      {syncData && (
        <div className="mt-8">
          <h2 className="text-xl font-semibold text-gray-700 mb-4">گزارش همگام‌سازی</h2>
          <div className="overflow-x-auto shadow rounded-lg">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    بخش
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    نتیجه
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {(Object.keys(syncData) as SyncEntityKey[]).map((key) => (
                  <tr key={key} className="hover:bg-gray-50 transition">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {entityLabels[key] || key}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {renderSyncResult(syncData[key])}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* جمع‌بندی کلی */}
          <div className="mt-4 text-sm text-gray-500">
            مجموع افزوده‌ها:{" "}
            {Object.values(syncData).reduce((sum, r) => sum + r.addedCount, 0)}
            &nbsp;| مجموع بروزرسانی‌ها:{" "}
            {Object.values(syncData).reduce((sum, r) => sum + r.updatedCount, 0)}
            &nbsp;| مجموع حذف‌ها:{" "}
            {Object.values(syncData).reduce((sum, r) => sum + r.deletedCount, 0)}
          </div>
        </div>
      )}
    </div>
  );
};