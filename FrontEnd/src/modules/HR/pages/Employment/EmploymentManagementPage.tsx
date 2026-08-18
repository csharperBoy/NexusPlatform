// src/modules/HR/pages/employment/EmploymentManagementPage.tsx

import React from "react";
import { EmploymentItem, useEmploymentManagement } from "../../hooks/Employment/useEmploymentManagement";
import { SearchableMultiSelect } from "@/core/components/Selection/SearchableMultiSelect";


export const EmploymentManagementPage: React.FC = () => {
  const {
    columnSearch,
    employments,
    error,
    fileInputRef,
    filteredEmployments,
    deleteTarget,isDeleting,
    globalSearch,
    handleGlobalSearch,
    handleColumnSearch,
    handleExcelImport,
    handleFieldChange,
    handleResetChanges,
    handleSaveChanges,
    loading,
    locations,
    modifiedIds,
    saving,
    successMessage,
    handleOpenDeleteModal,handleCloseDeleteModal,handleConfirmDelete
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
            <h1 className="text-2xl font-bold text-gray-800 mb-1">
              مدیریت و ویرایش اطلاعات کارمندان
            </h1>
            <p className="text-sm text-gray-500">
              کل کارمندان:{" "}
              <span className="font-semibold text-gray-700">
                {employments.length}
              </span>
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
                type="button"
                onClick={handleResetChanges}
                disabled={saving}
                className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg text-sm hover:bg-gray-100 transition-colors disabled:opacity-50 cursor-pointer"
              >
                انصراف و بازنشانی
              </button>
            )}

            <button
              type="button"
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

        {/* پیام‌های خطا و موفقیت */}
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

        {/* جستجوی کلی */}
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
                  onChange={(e) =>
                    handleColumnSearch("employmentCode", e.target.value)
                  }
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500 font-mono"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ نام..."
                  value={columnSearch["firstName"] || ""}
                  onChange={(e) =>
                    handleColumnSearch("firstName", e.target.value)
                  }
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ نام خانوادگی..."
                  value={columnSearch["lastName"] || ""}
                  onChange={(e) =>
                    handleColumnSearch("lastName", e.target.value)
                  }
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ کد ملی..."
                  value={columnSearch["nationalCode"] || ""}
                  onChange={(e) =>
                    handleColumnSearch("nationalCode", e.target.value)
                  }
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500 font-mono"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ محل استقرار..."
                  value={columnSearch["locationId"] || ""}
                  onChange={(e) =>
                    handleColumnSearch("locationId", e.target.value)
                  }
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
              filteredEmployments.map((emp: EmploymentItem, index: number) => {
                const empId = String(emp.id);
                const isModified = modifiedIds.has(empId);

                return (
                  <tr
                    key={empId}
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
                        onChange={(e) =>
                          handleFieldChange(
                            empId,
                            "employmentCode",
                            e.target.value
                          )
                        }
                        placeholder="کد پرسنلی..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 font-mono text-center dir-ltr outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={emp.firstName || ""}
                        onChange={(e) =>
                          handleFieldChange(empId, "firstName", e.target.value)
                        }
                        placeholder="نام..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 text-right outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={emp.lastName || ""}
                        onChange={(e) =>
                          handleFieldChange(empId, "lastName", e.target.value)
                        }
                        placeholder="نام خانوادگی..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 text-right outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={emp.nationalCode || ""}
                        onChange={(e) =>
                          handleFieldChange(
                            empId,
                            "nationalCode",
                            e.target.value
                          )
                        }
                        placeholder="کد ملی..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 font-mono text-center dir-ltr outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    {/* ستون انتخاب محل استقرار */}
                    <td className="py-2 px-3">
                      <SearchableMultiSelect
                        options={locations}
                        value={(emp.locationsId || []).map(String)}
                        onChange={(selectedValues) =>
                          handleFieldChange(
                            empId,
                            "locationsId",
                            selectedValues
                          )
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
                    <td className="py-2 px-3 text-center">
                      <button
                        type="button"
                        onClick={() => handleOpenDeleteModal(emp)}
                        title="حذف مکان"
                        className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer"
                      >
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.8} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                        </svg>
                      </button>
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>
      {/* --- مودال تأیید حذف --- */}
      {deleteTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
          <div 
            className="bg-white rounded-xl shadow-xl w-full max-w-sm overflow-hidden animate-in fade-in zoom-in duration-150"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="p-6 text-center">
              {/* آیکون اخطار حذف */}
              <div className="w-12 h-12 rounded-full bg-red-100 text-red-600 mx-auto flex items-center justify-center mb-4">
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
              </div>

              <h3 className="font-bold text-gray-800 text-lg mb-2">تأیید حذف مکان</h3>
              
              <p className="text-sm text-gray-600 leading-relaxed mb-3">
                آیا از حذف مکان <span className="font-semibold text-gray-900">«{deleteTarget.title}»</span> اطمینان دارید؟
              </p>

              {deleteTarget.isModified && (
                <div className="mb-4 p-2.5 bg-amber-50 border border-amber-200 rounded-lg text-amber-800 text-xs">
                  این سطر دارای تغییرات ذخیره‌نشده است. با حذف آن، تغییرات نیز از بین خواهند رفت.
                </div>
              )}

              <p className="text-xs text-gray-400">این عملیات قابل بازگشت نیست.</p>
            </div>

            {/* دکمه‌های مودال حذف */}
            <div className="px-5 py-3.5 bg-gray-50 border-t border-gray-100 flex items-center justify-center gap-3">
              <button
                type="button"
                onClick={handleCloseDeleteModal}
                disabled={isDeleting}
                className="w-full py-2 border border-gray-300 text-gray-700 rounded-lg text-xs font-medium hover:bg-gray-100 transition-colors cursor-pointer"
              >
                انصراف
              </button>
              <button
                type="button"
                onClick={handleConfirmDelete}
                disabled={isDeleting}
                className="w-full py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg text-xs font-medium shadow-sm transition-all disabled:opacity-50 cursor-pointer flex items-center justify-center gap-1.5"
              >
                {isDeleting ? "در حال حذف..." : "حذف شود"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default EmploymentManagementPage;