// src/modules/HR/pages/employment/EmploymentManagementPage.tsx

import React, { useEffect, useState, useMemo, useRef } from "react";
import * as XLSX from "xlsx";
import { useEmploymentManagement } from '../../hooks/Employment/useEmploymentManagement';
import { SearchableSelect } from "@/core/components/Selection/SearchableSelect";
import { SearchableMultiSelect } from "@/core/components/Selection/SearchableMultiSelect";

export const EmploymentManagementPage: React.FC = () => {
  const {
    columnSearch,
    employments,
    error,
    fileInputRef,
    filteredEmployments,
    globalSearch,
    handleGlobalSearch,
    handleColumnSearch,
    handleExcelImport,
    handleFieldChange,  
    handleResetChanges, 
    handleSaveChanges,
    initialEmployments,
    initialEmploymentsMap,
    loadData,
    loading,
    locationMap,
    locations,
    modifiedIds,
    saving,
    successMessage,
  } = useEmploymentManagement();

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[400px] text-gray-500 font-sans">
        در حال دریافت اطلاعات لیست کارمندان و مکان‌ها...
      </div>
    );
  }
  return (
    <div className="p-6 dir-rtl text-right font-sans bg-gray-50/50 min-h-screen">
      {/* هدر اصلی */}
      <div className="bg-white p-5 rounded-xl border border-gray-200 shadow-sm mb-5">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-gray-800 mb-1">مدیریت و ویرایش اطلاعات کارمندان</h1>
            <p className="text-sm text-gray-500">
              کل کارمندان: <span className="font-semibold text-gray-700">{employments.length}</span>
              {modifiedIds.size > 0 && (
                <span className="mr-3 text-amber-600 bg-amber-50 px-2 py-0.5 rounded border border-amber-200 text-xs font-medium">
                  {modifiedIds.size} تغییر ذخیره‌نشده
                </span>
              )}
            </p>
          </div>

          <div className="flex items-center gap-3">
            <input
              type="file"
              ref={fileInputRef}
              onChange={handleExcelImport}
              accept=".xlsx, .xls"
              className="hidden"
            />

            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              disabled={saving}
              className="px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-sm font-medium transition-colors disabled:opacity-50 flex items-center gap-2 cursor-pointer shadow-sm"
              title="بارگذاری اکسل جهت به‌روزرسانی اطلاعات کارمندان"
            >
              📊 بارگذاری از اکسل
            </button>

            {modifiedIds.size > 0 && (
              <button
                onClick={handleResetChanges}
                disabled={saving}
                className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg text-sm hover:bg-gray-100 transition-colors disabled:opacity-50"
              >
                انصراف و بازنشانی
              </button>
            )}

            <button
              onClick={handleSaveChanges}
              disabled={modifiedIds.size === 0 || saving}
              className={`px-5 py-2 rounded-lg text-sm font-medium shadow-sm transition-all flex items-center gap-2 ${
                modifiedIds.size > 0
                  ? "bg-blue-600 hover:bg-blue-700 text-white cursor-pointer"
                  : "bg-gray-200 text-gray-400 cursor-not-allowed"
              }`}
            >
              {saving ? "در حال ذخیره..." : "ذخیره تغییرات"}
            </button>
          </div>
        </div>

        {error && (
          <div className="mt-4 p-3 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg">
            {error}
          </div>
        )}
        {successMessage && (
          <div className="mt-4 p-3 bg-green-50 border border-green-200 text-green-700 text-sm rounded-lg">
            {successMessage}
          </div>
        )}

        {/* سرچ اصلی */}
        <div className="flex items-center justify-between gap-4 mt-5 pt-4 border-t border-gray-100">
          <div className="w-72">
            <input
              type="text"
              placeholder="جستجوی کلی در تمام فیلدها..."
              value={globalSearch}
              onChange={(e) => handleGlobalSearch(e.target.value)}
              className="w-full px-3 py-1.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 outline-none"
            />
          </div>
        </div>
      </div>

      {/* جدول کارمندان */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
        <table className="w-full text-right border-collapse">
          <thead>
            {/* ردیف اول: عناوین ستون‌ها */}
            <tr className="border-b border-gray-200 text-gray-700 text-xs font-semibold">
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-3 w-12 text-center h-[38px] border-b border-gray-200 shadow-sm">
                ردیف
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 w-36 h-[38px] border-b border-gray-200 shadow-sm">
                کد پرسنلی
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 h-[38px] border-b border-gray-200 shadow-sm">
                نام
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 h-[38px] border-b border-gray-200 shadow-sm">
                نام خانوادگی
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 w-44 h-[38px] border-b border-gray-200 shadow-sm">
                کد ملی
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 w-52 h-[38px] border-b border-gray-200 shadow-sm">
                محل استقرار
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 text-center w-28 h-[38px] border-b border-gray-200 shadow-sm">
                وضعیت
              </th>
            </tr>

            {/* ردیف دوم: اینپوت‌های سرچ ستونی */}
            <tr className="border-b border-gray-200">
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 border-b border-gray-200 shadow-sm"></th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ کد پرسنلی..."
                  value={columnSearch["employmentCode"] || ""}
                  onChange={(e) => handleColumnSearch("employmentCode", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500 font-mono"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ نام..."
                  value={columnSearch["firstName"] || ""}
                  onChange={(e) => handleColumnSearch("firstName", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ نام خانوادگی..."
                  value={columnSearch["lastName"] || ""}
                  onChange={(e) => handleColumnSearch("lastName", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ کد ملی..."
                  value={columnSearch["nationalCode"] || ""}
                  onChange={(e) => handleColumnSearch("nationalCode", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500 font-mono"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ محل استقرار..."
                  value={columnSearch["locationId"] || ""}
                  onChange={(e) => handleColumnSearch("locationId", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 border-b border-gray-200 shadow-sm"></th>
            </tr>
          </thead>

          <tbody className="divide-y divide-gray-100 text-sm">
            {filteredEmployments.length === 0 ? (
              <tr>
                <td colSpan={7} className="text-center py-12 text-gray-400">
                  هیچ کارمندی یافت نشد.
                </td>
              </tr>
            ) : (
              filteredEmployments.map((emp: any, index) => {
                const isModified = modifiedIds.has(emp.id);

                return (
                  <tr
                    key={emp.id}
                    className={`transition-colors hover:bg-gray-50/80 ${
                      isModified ? "bg-amber-50/40" : ""
                    }`}
                  >
                    <td className="py-3 px-3 text-center text-xs text-gray-400 font-mono">
                      {index + 1}
                    </td>

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={emp.employmentCode || ""}
                        onChange={(e) => handleFieldChange(emp.id, "employmentCode", e.target.value)}
                        placeholder="کد پرسنلی..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 font-mono text-center dir-ltr outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={emp.firstName || ""}
                        onChange={(e) => handleFieldChange(emp.id, "firstName", e.target.value)}
                        placeholder="نام..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 text-right outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={emp.lastName || ""}
                        onChange={(e) => handleFieldChange(emp.id, "lastName", e.target.value)}
                        placeholder="نام خانوادگی..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 text-right outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={emp.nationalCode || ""}
                        onChange={(e) => handleFieldChange(emp.id, "nationalCode", e.target.value)}
                        placeholder="کد ملی..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 font-mono text-center dir-ltr outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    {/* ستون انتخاب محل استقرار */}
                    <td className="py-2 px-3">
                      {/* <select
                        value={emp.locationId || ""}
                        onChange={(e) => handleFieldChange(emp.id, "locationId", e.target.value)}
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 text-right outline-none bg-white hover:border-gray-400 transition-colors cursor-pointer"
                      >
                        <option value="">-- انتخاب محل استقرار --</option>
                        {locations.map((loc) => (
                          <option key={loc.value} value={loc.value}>
                            {loc.display || loc.label}
                          </option>
                        ))}
                      </select> */}
                      <SearchableMultiSelect
                        options={locations} // لیست مکان‌ها
                        value={emp.locationsId || []} // آرایه‌ای از آی‌دی‌های انتخاب‌شده
                        onChange={(selectedValues) =>
                          handleFieldChange(emp.id, "locationsId", selectedValues)
                        }
                      />

                    </td>

                    <td className="py-3 px-4 text-center">
                      {isModified ? (
                        <span className="inline-block text-[10px] bg-amber-100 text-amber-800 border border-amber-300 px-2 py-0.5 rounded-full font-medium">
                          تغییر یافته
                        </span>
                      ) : (
                        <span className="text-gray-300 text-xs">-</span>
                      )}
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default EmploymentManagementPage;