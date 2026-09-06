// src/modules/HR/pages/Sync/SyncPage.tsx
import React, { useState } from "react";
import { SyncApi } from "../../api/SyncApi";
import { SyncResult,  entityLabels, SyncEntityKey } from "../../models/SyncModels";
import { AxiosError } from "axios";
import { BatchResult } from "@/core/models/apiResults";

// تعریف پیکربندی هر موجودیت
const entityConfigs: {
  key: SyncEntityKey;
  label: string;
  api: () => Promise<BatchResult<SyncResult>>;
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
  networkError: string | null;        // خطای شبکه یا HTTP
  batchResult: BatchResult<SyncResult> | null; // نتیجه‌ی کامل از سرور
};

export const SyncPage: React.FC = () => {
  // وضعیت همگام‌سازی کامل (فقط برای غیرفعال‌سازی دکمه‌ها)
  const [mainLoading, setMainLoading] = useState(false);

  // وضعیت هر بخش
  const [individualStates, setIndividualStates] = useState<
    Record<SyncEntityKey, IndividualSyncState>
  >(() => {
    const initial = {} as Record<SyncEntityKey, IndividualSyncState>;
    entityConfigs.forEach((cfg) => {
      initial[cfg.key] = { loading: false, networkError: null, batchResult: null };
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

  // همگام‌سازی کامل (اجرای سریالی همه‌ی بخش‌ها)
  const handleMainSync = async () => {
    setMainLoading(true);

    // ریست کردن وضعیت‌های قبلی
    const resetStates = {} as Record<SyncEntityKey, IndividualSyncState>;
    entityConfigs.forEach((cfg) => {
      resetStates[cfg.key] = { loading: false, networkError: null, batchResult: null };
    });
    setIndividualStates(resetStates);

    for (const config of entityConfigs) {
      const { key, api } = config;

      // علامت‌گذاری در حال اجرا
      setIndividualStates((prev) => ({
        ...prev,
        [key]: { loading: true, networkError: null, batchResult: null },
      }));

      try {
        const batchResult = await api();
        setIndividualStates((prev) => ({
          ...prev,
          [key]: { loading: false, networkError: null, batchResult },
        }));
      } catch (err) {
        const errorMsg = getErrorMessage(err);
        setIndividualStates((prev) => ({
          ...prev,
          [key]: { loading: false, networkError: errorMsg, batchResult: null },
        }));
        // ادامه‌ی حلقه با وجود خطا
      }
    }

    setMainLoading(false);
  };

  // همگام‌سازی تکی
  const handleIndividualSync = async (key: SyncEntityKey, api: () => Promise<BatchResult<SyncResult>>) => {
    setIndividualStates((prev) => ({
      ...prev,
      [key]: { loading: true, networkError: null, batchResult: null },
    }));

    try {
      const batchResult = await api();
      setIndividualStates((prev) => ({
        ...prev,
        [key]: { loading: false, networkError: null, batchResult },
      }));
    } catch (err) {
      setIndividualStates((prev) => ({
        ...prev,
        [key]: { loading: false, networkError: getErrorMessage(err), batchResult: null },
      }));
    }
  };

  // رندر محتوای نتیجه (آمار و پیام‌ها)
  const renderBatchResultContent = (batchResult: BatchResult<SyncResult>) => {
    const { succeeded, data, errors, successMessages } = batchResult;

    return (
      <div className="space-y-2">
         {/* داده‌های آماری (در صورت وجود) */}
        {data && (
          <div className="flex gap-4 text-sm">
            <span className="text-green-600">➕ افزوده: {data.addedCount}</span>
            <span className="text-blue-600">✏️ بروزرسانی: {data.updatedCount}</span>
            <span className="text-red-600">➖ حذف: {data.deletedCount}</span>
          </div>
        )}
        
        {/* اگر کل عملیات ناموفق بوده و خطایی ثبت نشده باشد */}
        {!succeeded && (!errors || errors.length === 0) && (
          <div className="text-red-700 text-sm bg-red-50 p-2 rounded border border-red-200">
            <span className="font-semibold">❌ خطا:</span> عملیات با شکست مواجه شد.
          </div>
        )}
       

        {/* خطاها (business errors) */}
        {errors && errors.length > 0 && (
          <div className="text-red-700 text-sm bg-red-50 p-2 rounded border border-red-200">
            <span className="font-semibold">❌ خطاها:</span>
            <ul className="list-disc list-inside pr-4 mt-1">
              {errors.map((err, idx) => (
                <li key={idx}>{err}</li>
              ))}
            </ul>
          </div>
        )}


        
        {/* پیام‌های موفقیت */}
        {successMessages && successMessages.length > 0 && (
          <div className="text-green-700 text-sm bg-green-50 p-2 rounded border border-green-200">
            <span className="font-semibold">✅ پیام‌های موفقیت:</span>
            <ul className="list-disc list-inside pr-4 mt-1">
              {successMessages.map((msg, idx) => (
                <li key={idx}>{msg}</li>
              ))}
            </ul>
          </div>
        )}

      </div>
    );
  };

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
        {mainLoading ? "در حال همگام‌سازی (لطفاً صبر کنید)..." : "شروع همگام‌سازی کامل"}
      </button>

      {mainLoading && (
        <div className="mt-6 flex justify-center">
          <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
        </div>
      )}

      {/* بخش همگام‌سازی تکی (یک ستون) */}
      <div className="mt-10">
        <h2 className="text-xl font-semibold text-gray-700 mb-4">همگام‌سازی بخش‌ها به‌صورت مجزا</h2>
        <div className="space-y-4">
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

                {state.networkError && (
                  <div className="mt-2 p-2 bg-red-50 border border-red-200 rounded text-red-700 text-sm">
                    <span className="font-semibold">خطا: </span> {state.networkError}
                  </div>
                )}

                {state.batchResult && (
                  <div className="mt-2 p-2 bg-white rounded border">
                    {renderBatchResultContent(state.batchResult)}
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