// src/modules/HR/pages/PostContact/PostContactManagementPage.tsx
import React from "react";
import { usePostContactManagement } from "../../hooks/PostContact/usePostContactManagement";
import { TagInput } from "@/core/components/Input/TagInput";
// تابع کمکی تبدیل رشته متصل با ویرگول به آرایه تگ‌ها
// const parseTags = (value: string | undefined | null): string[] => {
//   if (!value) return [];
//   return value
//     .split(",")
//     .map((s) => s.trim())
//     .filter(Boolean);
// };

export const PostContactManagementPage: React.FC = () => {
  const {
    postContacts,
    flattenedTree,
    loading,
    saving,
    error,
    successMessage,
    globalSearch,
    setGlobalSearch,
    columnSearch,
    handleColumnSearch,
    selectedIds,
    modifiedIds,
    draggedIds,
    dragOverId,
    fileInputRef,
    handleExcelImport,
    handleFieldChange,
    toggleExpand,
    expandAll,
    collapseAll,
    handleResetChanges,
    handleSaveChanges,
  } = usePostContactManagement();

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[400px] text-gray-500 font-sans">
        در حال دریافت اطلاعات چارت سازمانی...
      </div>
    );
  }

  return (
    <div className="p-6 dir-rtl text-right font-sans bg-gray-50/50 min-h-screen select-none">
      {/* هدر اصلی */}
      <div className="bg-white p-5 rounded-xl border border-gray-200 shadow-sm mb-5 select-text">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-gray-800 mb-1">مدیریت ساختار چارت سازمانی</h1>
            <p className="text-sm text-gray-500">
              کل پست‌ها: <span className="font-semibold text-gray-700">{postContacts.length}</span>
              {selectedIds.size > 0 && (
                <span className="mr-3 text-blue-600 bg-blue-50 px-2 py-0.5 rounded border border-blue-200 text-xs font-medium">
                  {selectedIds.size} رکورد انتخاب شده
                </span>
              )}
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
              title="بارگذاری اکسل جهت به‌روزرسانی تلفن داخلی و موبایل سازمانی"
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

        {/* سرچ اصلی و کنترل‌های درخت */}
        <div className="flex flex-wrap items-center justify-between gap-4 mt-5 pt-4 border-t border-gray-100">
          <div className="w-72">
            <input
              type="text"
              placeholder="جستجوی کلی در تمام فیلدها..."
              value={globalSearch}
              onChange={(e) => setGlobalSearch(e.target.value)}
              className="w-full px-3 py-1.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 outline-none"
            />
          </div>

          <div className="flex items-center gap-2 text-xs text-gray-500">
            <button
              onClick={expandAll}
              className="px-3 py-1.5 text-xs text-gray-600 bg-gray-100 hover:bg-gray-200 rounded border border-gray-300 mr-2"
            >
              گسترش همه
            </button>
            <button
              onClick={collapseAll}
              className="px-3 py-1.5 text-xs text-gray-600 bg-gray-100 hover:bg-gray-200 rounded border border-gray-300"
            >
              جمع‌کردن همه
            </button>
          </div>
        </div>
      </div>

      {/* جدول چارت */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-x-auto">
        <table className="w-full text-right border-collapse min-w-[1000px]">
          <thead>
            {/* ردیف اول: عناوین ستون‌ها */}
            <tr className="border-b border-gray-200 text-gray-700 text-xs font-semibold">
              <th className="sticky top-0 z-20 bg-gray-100 py-2.5 px-4 border-b border-gray-200 shadow-sm">
                عنوان شغل
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2.5 px-4 border-b border-gray-200 shadow-sm">
                واحد سازمانی
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2.5 px-4 border-b border-gray-200 shadow-sm">
                شاغل فعلی
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2.5 px-4 border-b border-gray-200 shadow-sm">
                سطح شغلی
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2.5 px-4 w-60 border-b border-gray-200 shadow-sm">
                تلفن‌های داخلی
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2.5 px-4 w-64 border-b border-gray-200 shadow-sm">
                موبایل‌های سازمانی
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2.5 px-4 text-center w-24 border-b border-gray-200 shadow-sm">
                وضعیت
              </th>
            </tr>

            {/* ردیف دوم: اینپوت‌های سرچ */}
            <tr className="border-b border-gray-200">
              <th className="sticky top-[34px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ شغل ..."
                  value={columnSearch["jobTitle"] || ""}
                  onChange={(e) => handleColumnSearch("jobTitle", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ واحد..."
                  value={columnSearch["unit"] || ""}
                  onChange={(e) => handleColumnSearch("unit", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ شاغل..."
                  value={columnSearch["occupant"] || ""}
                  onChange={(e) => handleColumnSearch("occupant", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ سطح شغلی..."
                  value={columnSearch["levelGrade"] || ""}
                  onChange={(e) => handleColumnSearch("levelGrade", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ داخلی..."
                  value={columnSearch["officePhone"] || ""}
                  onChange={(e) => handleColumnSearch("officePhone", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ موبایل..."
                  value={columnSearch["orgMobile"] || ""}
                  onChange={(e) => handleColumnSearch("orgMobile", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-50 py-1.5 px-2 border-b border-gray-200 shadow-sm"></th>
            </tr>
          </thead>

          <tbody className="divide-y divide-gray-100 text-sm">
            {flattenedTree.length === 0 ? (
              <tr>
                <td colSpan={7} className="text-center py-12 text-gray-400">
                  هیچ پستی یافت نشد.
                </td>
              </tr>
            ) : (
              flattenedTree.map(({ node, depth, hasChildren, isExpanded, isModified }) => {
                const isSelected = selectedIds.has(node.id);
                const isBeingDragged = draggedIds.includes(node.id);
                const isTarget = dragOverId === node.id;
                const occupantName =
                  node.firstName || node.lastName
                    ? `${node.firstName || ""} ${node.lastName || ""}`
                    : "-";

                return (
                  <tr
                    key={node.id}
                    className={`transition-colors cursor-pointer ${
                      isSelected ? "bg-blue-100/70 border-blue-300 font-medium" : ""
                    } ${isBeingDragged ? "opacity-30 bg-gray-200" : ""} ${
                      isTarget ? "bg-blue-200 border-y-2 border-blue-600" : "hover:bg-gray-50/80"
                    } ${isModified && !isSelected ? "bg-amber-50/40" : ""}`}
                  >
                    <td className="py-3 px-4 font-medium text-gray-800">
                      <div
                        className="flex items-center gap-2"
                        style={{ paddingRight: `${depth * 24}px` }}
                      >
                        {hasChildren ? (
                          <button
                            type="button"
                            onClick={(e) => {
                              e.stopPropagation();
                              toggleExpand(node.id);
                            }}
                            className="w-5 h-5 flex items-center justify-center rounded text-gray-500 hover:bg-gray-200 text-xs"
                          >
                            {isExpanded ? "▼" : "◀"}
                          </button>
                        ) : (
                          <span className="w-5 text-center text-gray-300">•</span>
                        )}
                        <span>
                          {node.jobTitleName || "بدون عنوان شغل"}{" "}
                          {node.postCode && (
                            <span className="text-gray-500 text-xs font-mono font-normal">
                              ({node.postCode})
                            </span>
                          )}
                        </span>
                      </div>
                    </td>

                    <td className="py-3 px-4 text-gray-600 text-xs">
                      {node.organizationUnitsName || "-"}
                    </td>

                    <td className="py-3 px-4 text-gray-700 text-xs">
                      <div className="flex flex-col">
                        <span className="font-medium">{occupantName}</span>
                        {node.employmentCode && (
                          <span className="text-[10px] text-gray-400 font-mono">
                            کد: {node.employmentCode}
                          </span>
                        )}
                      </div>
                    </td>

                    <td className="py-3 px-4 text-gray-500 text-xs">
                      {node.jobLevelTitle || node.gradeTitle ? (
                        <span>
                          {node.jobLevelTitle || ""} {node.gradeTitle ? `(${node.gradeTitle})` : ""}
                        </span>
                      ) : (
                        "-"
                      )}
                    </td>

                    <td className="py-3 px-4">
                          <TagInput
                            value={node.officePhone ?? []} // رفع خطای Type 'null' is not assignable to type 'string[] | undefined'
                            onChange={(vals) => handleFieldChange(node.id, "officePhone", vals)}
                            placeholder="افزودن شماره تلفن..."
                          />
                        </td>
                        <td className="py-3 px-4">
                          <TagInput
                            value={node.orgMobile ?? []} // رفع خطای Type 'null' is not assignable to type 'string[] | undefined'
                            onChange={(vals) => handleFieldChange(node.id, "orgMobile", vals)}
                            placeholder="افزودن همراه..."
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

export default PostContactManagementPage;