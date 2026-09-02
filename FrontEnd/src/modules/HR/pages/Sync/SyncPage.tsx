// src/modules/HR/pages/Sync/SyncPage.tsx
import React, { useState } from "react";
import {
  SyncApi
} from "../../api/SyncApi";
import { SyncResult, entityLabels, SyncEntityKey } from "../../models/SyncModels";
import { AxiosError } from "axios";

// تعریف پیکربندی هر موجودیت برای همگام‌سازی تکی
const entityConfigs: {
  key: SyncEntityKey;
  label: string;
  api: () => Promise<SyncResult>;
}[] = [
  { key: "orgUnit", label: entityLabels.orgUnit, api: SyncApi.SyncOrganizationUnit },
  { key: "jobLevel", label: entityLabels.jobLevel, api: SyncApi.SyncJobLevel },
  { key: "jobTitle", label: entityLabels.jobTitle, api: SyncApi.SyncJobTitle },
  { key: "employment", label: entityLabels.employment, api: SyncApi.SyncEmployement },
  { key: "post", label: entityLabels.post, api: SyncApi.SyncPost },
  { key: "assignment", label: entityLabels.assignment, api: SyncApi.SyncAssignments },
];

// نوع وضعیت هر همگام‌سازی تکی
type IndividualSyncState = {
  loading: boolean;
  error: string | null;
  result: SyncResult | null;
};

export const SyncPage: React.FC = () => {
  // وضعیت همگام‌سازی کامل (همه‌ی موجودیت‌ها)
  const [mainLoading, setMainLoading] = useState(false);
  const [mainData, setMainData] = useState<Record<string, SyncResult> | null>(null);
  const [mainError, setMainError] = useState<string | null>(null);

  // وضعیت همگام‌سازی‌های تکی
  const [individualStates, setIndividualStates] = useState<
    Record<SyncEntityKey, IndividualSyncState>
  >(() => {
    const initial = {} as Record<SyncEntityKey, IndividualSyncState>;
    entityConfigs.forEach((cfg) => {
      initial[cfg.key] = { loading: false, error: null, result: null };
    });
    return initial;
  });

  // تابع کمکی برای استخراج پیام خطا از AxiosError
  const getErrorMessage = (err: unknown): string => {
    if (err instanceof AxiosError) {
      if (err.code === "ECONNABORTED" || err.message.includes("timeout")) {
        return "عملیات همگام‌سازی زمان‌بر است، لطفاً منتظر بمانید...";
      }
      if (err.response) {
        const data = err.response.data;
        return typeof data === "string" ? data : JSON.stringify(data);
      }
      return "خطا در برقراری ارتباط با سرور.";
    }
    return "خطای ناشناخته رخ داده است.";
  };

  // همگام‌سازی کامل
  const handleMainSync = async () => {
    setMainLoading(true);
    setMainError(null);
    setMainData(null);

    try {
      const data = await SyncApi.syncWithIrisa();
      setMainData(data);
    } catch (err) {
      setMainError(getErrorMessage(err));
    } finally {
      setMainLoading(false);
    }
  };

  // همگام‌سازی تکی برای یک موجودیت
  const handleIndividualSync = async (key: SyncEntityKey, api: () => Promise<SyncResult>) => {
    // تنظیم وضعیت بارگذاری و پاک کردن نتیجه/خطای قبلی
    setIndividualStates((prev) => ({
      ...prev,
      [key]: { loading: true, error: null, result: null },
    }));

    try {
      const result = await api();
      setIndividualStates((prev) => ({
        ...prev,
        [key]: { loading: false, error: null, result },
      }));
    } catch (err) {
      setIndividualStates((prev) => ({
        ...prev,
        [key]: { loading: false, error: getErrorMessage(err), result: null },
      }));
    }
  };

  // رندر نتیجه‌ی یک همگام‌سازی (مشترک برای کامل و تکی)
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

      {/* دکمه‌ی همگام‌سازی کامل */}
      <button
        onClick={handleMainSync}
        disabled={mainLoading}
        className={`w-full py-3 px-4 rounded-lg text-white font-semibold transition ${
          mainLoading
            ? "bg-gray-400 cursor-not-allowed"
            : "bg-blue-600 hover:bg-blue-700 active:scale-95"
        }`}
      >
        {mainLoading ? "در حال همگام‌سازی (لطفاً صبر کنید)..." : "شروع همگام‌سازی"}
      </button>

      {mainLoading && (
        <div className="mt-6 flex justify-center">
          <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
        </div>
      )}

      {mainError && (
        <div className="mt-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700">
          <span className="font-semibold">خطا: </span> {mainError}
        </div>
      )}

      {/* نمایش نتایج همگام‌سازی کامل */}
      {mainData && (
        <div className="mt-8">
          <h2 className="text-xl font-semibold text-gray-700 mb-4">گزارش همگام‌سازی کامل</h2>
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
                {(Object.keys(mainData) as SyncEntityKey[]).map((key) => (
                  <tr key={key} className="hover:bg-gray-50 transition">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {entityLabels[key] || key}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {renderSyncResult(mainData[key])}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* جمع‌بندی کلی */}
          <div className="mt-4 text-sm text-gray-500">
            مجموع افزوده‌ها:{" "}
            {Object.values(mainData).reduce((sum, r) => sum + r.addedCount, 0)}
            &nbsp;| مجموع بروزرسانی‌ها:{" "}
            {Object.values(mainData).reduce((sum, r) => sum + r.updatedCount, 0)}
            &nbsp;| مجموع حذف‌ها:{" "}
            {Object.values(mainData).reduce((sum, r) => sum + r.deletedCount, 0)}
          </div>
        </div>
      )}

      {/* بخش همگام‌سازی‌های تکی */}
      <div className="mt-10">
        <h2 className="text-xl font-semibold text-gray-700 mb-4">همگام‌سازی بخش‌ها به‌صورت مجزا</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {entityConfigs.map(({ key, label, api }) => {
            const state = individualStates[key];
            return (
              <div key={key} className="border rounded-lg p-4 bg-gray-50">
                <div className="flex items-center justify-between mb-2">
                  <span className="font-medium text-gray-800">{label}</span>
                  <button
                    onClick={() => handleIndividualSync(key, api)}
                    disabled={state.loading || mainLoading}
                    className={`px-4 py-2 rounded-lg text-sm font-semibold text-white transition ${
                      state.loading || mainLoading
                        ? "bg-gray-400 cursor-not-allowed"
                        : "bg-indigo-600 hover:bg-indigo-700 active:scale-95"
                    }`}
                  >
                    {state.loading ? "در حال..." : "همگام‌سازی"}
                  </button>
                </div>

                {state.loading && (
                  <div className="flex justify-center my-2">
                    <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-indigo-600"></div>
                  </div>
                )}

                {state.error && (
                  <div className="mt-2 p-2 bg-red-50 border border-red-200 rounded text-red-700 text-sm">
                    <span className="font-semibold">خطا: </span> {state.error}
                  </div>
                )}

                {state.result && (
                  <div className="mt-2 p-2 bg-white rounded border">
                    {renderSyncResult(state.result)}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};