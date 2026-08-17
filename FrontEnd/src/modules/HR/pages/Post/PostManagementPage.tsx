// src/modules/HR/pages/Post/PostManagementPage.tsx

import React, { useEffect, useState, useMemo, useRef } from "react";
import * as XLSX from "xlsx";
import { postApi } from "../../api/PostApi";
import { PostInfoView } from "../../models/postInfoView";
import { UpdatePostCommand } from "../../models/postCommand";
import { usePostManagement } from "../../hooks/Post/usePostManagement";
import { SearchableSelect } from "@/core/components/Selection/SearchableSelect";


export const PostManagementPage: React.FC = () => {
  const {
    columnSearch,
    collapseAll,
    dragOverId,draggedIds,draggedIdsRef,
    employmentMap,employments,error,
    expandAll,expandedIds,fileInputRef,
    flattenedTree,globalSearch,handleColumnSearch,handleDragOverRow,
    handleDragStart,handleDragOverId,handleIsOverRootZone,
    handleDropOnRoot,handleDropOnRow,handleExcelImport,
    handleFieldChange,handleGlobalSearch,handleResetChanges,
    handleRowClick,handleSaveChanges,initialPosts,
    initialPostsMap,isDescendant,isOverRootZone,
    lastSelectedId,loadData,loading,
    locationMap,locations,
    modifiedIds,posts,postsMap,saving,selectedIds,successMessage,toggleExpand,updateNodesParent,
  } = usePostManagement();


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
              کل پست‌ها: <span className="font-semibold text-gray-700">{posts.length}</span>
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
              onChange={(e) => handleGlobalSearch(e.target.value)}
              className="w-full px-3 py-1.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 outline-none"
            />
          </div>

          <div className="flex items-center gap-2 text-xs text-gray-500">
            <span>💡 راهنما: برای انتخاب چندگانه از کلیدهای Ctrl و Shift استفاده کنید.</span>
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

      {/* منطقه رهاسازی ریشه */}
      <div
        onDragOver={(e) => {
          e.preventDefault();
          if (draggedIds.length > 0) handleIsOverRootZone(true);
        }}
        onDragLeave={() => handleIsOverRootZone(false)}
        onDrop={handleDropOnRoot}
        className={`sticky top-0 z-30 mb-2 h-[34px] px-3 border border-dashed rounded-lg text-center text-xs transition-all shadow-sm backdrop-blur-md flex items-center justify-center ${
          isOverRootZone
            ? "border-blue-500 bg-blue-100/95 text-blue-800 font-bold scale-[1.005]"
            : "border-gray-300 bg-white/95 text-gray-600 hover:border-gray-400"
        }`}
      >
        📌 جهت انتقال موارد انتخاب‌شده به بالاترین سطح چارت (بدون والد)، آن‌ها را اینجا رها کنید.
      </div>

      {/* جدول چارت */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
        <table className="w-full text-right border-collapse">
          <thead>
            {/* ردیف اول: عناوین ستون‌ها */}
            <tr className="border-b border-gray-200 text-gray-700 text-xs font-semibold">
              <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-3 w-10 text-center border-b border-gray-200 shadow-sm">
                جابه‌جایی
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-4 border-b border-gray-200 shadow-sm">
                عنوان شغل (کد پست)
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-4 border-b border-gray-200 shadow-sm">
                واحد سازمانی
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-4 border-b border-gray-200 shadow-sm">
                شاغل فعلی
              </th>
             
              <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-4 border-b border-gray-200 shadow-sm">
                رده / سطح شغلی
              </th>
               <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-4 border-b border-gray-200 shadow-sm">
                محل استقرار
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-4 text-center w-24 border-b border-gray-200 shadow-sm">
                وضعیت
              </th>
            </tr>

            {/* ردیف دوم: اینپوت‌های سرچ */}
            <tr className="border-b border-gray-200">
              <th className="sticky top-[70px] z-20 bg-gray-50 py-1.5 px-2 border-b border-gray-200 shadow-sm"></th>
              <th className="sticky top-[70px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ شغل / کد..."
                  value={columnSearch["jobTitle"] || ""}
                  onChange={(e) => handleColumnSearch("jobTitle", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[70px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ واحد..."
                  value={columnSearch["unit"] || ""}
                  onChange={(e) => handleColumnSearch("unit", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[70px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ شاغل..."
                  value={columnSearch["occupant"] || ""}
                  onChange={(e) => handleColumnSearch("occupant", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              
              <th className="sticky top-[70px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ رده..."
                  value={columnSearch["levelGrade"] || ""}
                  onChange={(e) => handleColumnSearch("levelGrade", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[70px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ محل استقرار..."
                  value={columnSearch["locationId"] || ""}
                  onChange={(e) => handleColumnSearch("locationId", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[70px] z-20 bg-gray-50 py-1.5 px-2 border-b border-gray-200 shadow-sm"></th>
            </tr>
          </thead>

          <tbody className="divide-y divide-gray-100 text-sm">
            {flattenedTree.length === 0 ? (
              <tr>
                <td colSpan={8} className="text-center py-12 text-gray-400">
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
                    onClick={(e) => handleRowClick(e, node.id)}
                    onDragOver={(e) => handleDragOverRow(e, node.id)}
                    onDragLeave={() => dragOverId === node.id && handleDragOverId(null)}
                    onDrop={(e) => handleDropOnRow(e, node.id)}
                    className={`transition-colors cursor-pointer ${
                      isSelected ? "bg-blue-100/70 border-blue-300 font-medium" : ""
                    } ${isBeingDragged ? "opacity-30 bg-gray-200" : ""} ${
                      isTarget ? "bg-blue-200 border-y-2 border-blue-600" : "hover:bg-gray-50/80"
                    } ${isModified && !isSelected ? "bg-amber-50/40" : ""}`}
                  >
                    <td className="py-3 px-2 text-center align-middle">
                      <div
                        draggable
                        onDragStart={(e) => handleDragStart(e, node.id)}
                        className="cursor-grab active:cursor-grabbing text-gray-400 hover:text-gray-700 text-lg leading-none inline-block p-1"
                        title="جهت تغییر والد، کشیده و رها کنید (امکان انتخاب گروهی)"
                      >
                        ☰
                      </div>
                    </td>

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

                    {/* <td className="py-3 px-4 text-gray-700 text-xs">
                      <div className="flex flex-col">
                        <span className="font-medium">{occupantName}</span>
                        {node.employmentCode && (
                          <span className="text-[10px] text-gray-400 font-mono">
                            کد: {node.employmentCode}
                          </span>
                        )}
                      </div>
                    </td> */}
                     <td className="py-3 px-4 text-gray-500 text-xs">
                      <SearchableSelect
                        options={employments}
                        value={node.employmentId || ""}
                        onChange={(selected) => handleFieldChange(node.id, "employmentId", selected?.value || "")}
                        placeholder="انتخاب شاغل..."
                      />
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

 
                    <td className="py-3 px-4 text-gray-500 text-xs">
                      <SearchableSelect
                        options={locations}
                        value={node.locationId || ""}
                        onChange={(selected) => handleFieldChange(node.id, "locationId", selected?.value || "")}
                        placeholder="انتخاب محل استقرار..."
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

export default PostManagementPage;