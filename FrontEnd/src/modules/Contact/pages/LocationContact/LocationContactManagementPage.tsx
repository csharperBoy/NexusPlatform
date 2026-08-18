// src/modules/HR/pages/locationContact/LocationContactManagementPage.tsx

import React from "react";
import { useLocationContactManagement } from "../../hooks/LocationContact/useLocationContactManagement";
import { TagInput } from "@/core/components/Input/TagInput";

export const LocationContactManagementPage: React.FC = () => {
  const {
    locationContacts,
    loading,
    saving,
    error,
    successMessage,
    globalSearch,
    setGlobalSearch,
    isModified,
    modifiedCount,
    handleFieldChange,
    handleSaveChanges,
    reload,
  } = useLocationContactManagement();

  return (
    <div className="p-6 max-w-7xl mx-auto space-y-6 dir-rtl">
      {/* هدر و دکمه‌های عملیات */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white dark:bg-gray-800 p-4 rounded-xl shadow-sm border border-gray-100 dark:border-gray-700">
        <div>
          <h1 className="text-xl font-bold text-gray-800 dark:text-white">
            مدیریت شماره‌های تماس واحدها
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
            اطلاعات تلفن ثابت و موبایل واحدهای سازمانی را مدیریت و به‌روزرسانی کنید.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={reload}
            disabled={loading || saving}
            className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 dark:bg-gray-700 dark:text-gray-200 dark:hover:bg-gray-600 rounded-lg transition-colors disabled:opacity-50"
          >
            بازنشانی
          </button>
          <button
            type="button"
            onClick={handleSaveChanges}
            disabled={!isModified || saving || loading}
            className={`px-5 py-2 text-sm font-medium text-white rounded-lg transition-colors flex items-center gap-2 ${
              isModified && !saving && !loading
                ? "bg-blue-600 hover:bg-blue-700 shadow-md shadow-blue-500/20"
                : "bg-gray-400 cursor-not-allowed opacity-60"
            }`}
          >
            {saving && (
              <span className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
            )}
            ذخیره تغییرات {modifiedCount > 0 && `(${modifiedCount})`}
          </button>
        </div>
      </div>

      {/* پیام‌های وضعیت */}
      {error && (
        <div className="p-4 bg-red-50 border-r-4 border-red-500 text-red-700 rounded-lg text-sm">
          {error}
        </div>
      )}
      {successMessage && (
        <div className="p-4 bg-green-50 border-r-4 border-green-500 text-green-700 rounded-lg text-sm">
          {successMessage}
        </div>
      )}

      {/* نوار جستجو */}
      <div className="bg-white dark:bg-gray-800 p-4 rounded-xl shadow-sm border border-gray-100 dark:border-gray-700">
        <input
          type="text"
          value={globalSearch}
          onChange={(e) => setGlobalSearch(e.target.value)}
          placeholder="جستجو بر اساس عنوان واحد، شماره تلفن یا موبایل..."
          className="w-full px-4 py-2.5 rounded-lg border border-gray-300 dark:border-gray-600 dark:bg-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all text-sm"
        />
      </div>

      {/* جدول داده‌ها */}
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-100 dark:border-gray-700 overflow-hidden">
        {loading ? (
          <div className="p-12 text-center text-gray-500 dark:text-gray-400">
            <div className="inline-block w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin mb-3"></div>
            <p>در حال دریافت اطلاعات...</p>
          </div>
        ) : locationContacts.length === 0 ? (
          <div className="p-12 text-center text-gray-500 dark:text-gray-400">
            هیچ موردی یافت نشد.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-right text-sm">
              <thead className="bg-gray-50 dark:bg-gray-700/50 text-gray-600 dark:text-gray-300 border-b border-gray-200 dark:border-gray-700">
                <tr>
                  <th className="py-3.5 px-4 font-semibold w-12 text-center">#</th>
                  <th className="py-3.5 px-4 font-semibold min-w-[200px]">عنوان واحد</th>
                  <th className="py-3.5 px-4 font-semibold min-w-[280px]">شماره‌های تلفن ثابت</th>
                  <th className="py-3.5 px-4 font-semibold min-w-[280px]">شماره‌های همراه</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 dark:divide-gray-700/50">
                {locationContacts.map((loc, index) => (
                  <tr
                    key={loc.id}
                    className="hover:bg-gray-50/80 dark:hover:bg-gray-700/30 transition-colors"
                  >
                    <td className="py-3 px-4 text-center text-gray-400 font-medium">
                      {index + 1}
                    </td>
                    <td className="py-3 px-4 font-medium text-gray-800 dark:text-gray-200">
                      {loc.title}
                    </td>
                    <td className="py-3 px-4">
                      <TagInput
                        value={loc.orgPhone ?? []} // رفع خطای Type 'null' is not assignable to type 'string[] | undefined'
                        onChange={(vals) => handleFieldChange(loc.id, "orgPhone", vals)}
                        placeholder="افزودن شماره تلفن..."
                      />
                    </td>
                    <td className="py-3 px-4">
                      <TagInput
                        value={loc.orgMobile ?? []} // رفع خطای Type 'null' is not assignable to type 'string[] | undefined'
                        onChange={(vals) => handleFieldChange(loc.id, "orgMobile", vals)}
                        placeholder="افزودن همراه..."
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};