import React from "react";
import { SearchableSelect } from "@/core/components/Selection/SearchableSelect";
import { SearchableMultiSelect } from "@/core/components/Selection/SearchableMultiSelect";
import { BaseEntity } from "@/core/models/BaseEntity";
import { TreeColumnDef } from "../types";
import { useGenericTreeCrud } from "../hooks/useGenericTreeCrud";
import type {  UseGenericTreeCrudOptions } from "../types";
import { TagInput } from "@/core/components/Input/TagInput";
import { HierarchicalEntity } from "@/core/models/HierarchicalEntity";

interface Props<T extends HierarchicalEntity, TCreateCmd, TUpdateCmd> {
  title: string;
  columns: TreeColumnDef<T>[];
  crudOptions: UseGenericTreeCrudOptions<T, TCreateCmd, TUpdateCmd>;
}

export function GenericTreeCrudPage<
   T extends HierarchicalEntity,  
  TCreateCmd,
  TUpdateCmd
>({ title, columns, crudOptions }: Props<T, TCreateCmd, TUpdateCmd>) {
  const crud = useGenericTreeCrud<T, TCreateCmd, TUpdateCmd>({
    ...crudOptions,
    columns,
  });

  const {
    enableExcelImport = false,
    enableSearch = true,
    enableColumnFilter = true,
    enableDelete = true,
    enableDragDrop = true,
    enableExpandCollapseAll = true,
    enableStatusColumn = true,
    enableInlineAddChild = false, // ← NEW
  } = crudOptions.tableFeatures || {};

  const {
    enableAddAsRoot = false, // ← NEW (مستقل از enableAdd)
  } = crudOptions.pageFeatures || {};

  const showActionColumn = enableDelete || enableInlineAddChild;
  const colCount =
    columns.length +
    (enableDragDrop ? 1 : 0) +
    (enableStatusColumn ? 1 : 0) +
    (showActionColumn ? 1 : 0);

  
const renderCell = (
  col: TreeColumnDef<T>,
  node: T,
  rowId: string
) => {
  const raw = node[col.key as keyof T];
  const options = col.selectionKey
    ? crud.selectionLists[col.selectionKey] || []
    : [];

  // ستون‌های فقط‌خواندنی: render سفارشی یا نمایش متن
  if (col.editable === false) {
    if (col.render) return col.render(raw, node);
    return (
      <span dir={col.dir || "rtl"} className={col.className}>
        {String(raw ?? "")}
      </span>
    );
  }

  // اگر editable صریحاً true نبود ولی render داشت، render رو ترجیح بده
  if (col.render && col.type == null) {
    return col.render(raw, node);
  }

  if (col.type === "select") {
    return (
      <SearchableSelect
        options={options}
        value={(raw as string) || ""}
        onChange={(sel) =>
          crud.handleFieldChange(rowId, col.key, sel?.value ?? null)
        }
        placeholder={col.label}
      />
    );
  }

  if (col.type === "multi-select") {
    const ids = Array.isArray(raw)
      ? (raw as any[]).map((x) =>
          typeof x === "object" && x != null ? String(x.id) : String(x)
        )
      : [];
    return (
      <SearchableMultiSelect
        options={options}
        value={ids}
        onChange={(ids) => crud.handleFieldChange(rowId, col.key, ids)}
        placeholder={col.label}
      />
    );
  }

  if (col.type === "taginput") {
    return (
      <TagInput
        value={(raw as string[]) || []}
        onChange={(vals) => crud.handleFieldChange(rowId, col.key, vals)}
        placeholder={col.label}
      />
    );
  }

  if (col.type === "boolean") {
    return (
      <input
        type="checkbox"
        checked={!!raw}
        onChange={(e) =>
          crud.handleFieldChange(rowId, col.key, e.target.checked)
        }
        className="h-4 w-4 rounded border-gray-300 text-blue-600"
      />
    );
  }

  return (
    <input
      type={col.type === "number" ? "number" : "text"}
      value={(raw as string) || ""}
      dir={col.dir || "rtl"}
      onChange={(e) =>
        crud.handleFieldChange(rowId, col.key, e.target.value)
      }
      className={`w-full rounded border border-gray-300 p-1.5 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white ${
        col.className || ""
      }`}
    />
  );
};

  return (
    <div className="p-6 dir-rtl text-right bg-gray-50/50 min-h-screen">
      {/* ─── هدر ─── */}
      <div className="bg-white p-5 rounded-xl border border-gray-200 shadow-sm mb-5">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-gray-800 mb-1">{title}</h1>
            <p className="text-sm text-gray-500">
              کل:{" "}
              <span className="font-semibold text-gray-700">
                {crud.items.length}
              </span>
              {crud.selectedIds.size > 0 && (
                <span className="mr-3 text-blue-600 bg-blue-50 px-2 py-0.5 rounded border border-blue-200 text-xs">
                  {crud.selectedIds.size} انتخاب‌شده
                </span>
              )}
              {crud.hasChanges && (
                <span className="mr-3 text-amber-600 bg-amber-50 px-2 py-0.5 rounded border border-amber-200 text-xs">
                  {crud.newCount > 0 && `${crud.newCount} جدید`}
                  {crud.newCount > 0 && crud.modifiedCount > 0 && "، "}
                  {crud.modifiedCount > 0 &&
                    `${crud.modifiedCount} تغییر`}
                </span>
              )}
            </p>
          </div>

          <div className="flex items-center gap-3">
            {enableExcelImport && (
              <>
                <input
                  type="file"
                  ref={crud.fileInputRef}
                  onChange={crud.handleExcelImport}
                  accept=".xlsx,.xls"
                  className="hidden"
                />
                <button
                  onClick={() => crud.fileInputRef.current?.click()}
                  disabled={crud.saving}
                  className="px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-sm font-medium disabled:opacity-50"
                >
                  📊 بارگذاری از اکسل
                </button>
              </>
            )}

            {/* ─── دکمه افزودن رکورد ریشه ─── */}
            {enableAddAsRoot && (
              <button
                onClick={() => crud.handleAddChild(null)}
                disabled={crud.saving}
                className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-medium disabled:opacity-50 flex items-center gap-1.5"
              >
                <span className="text-base leading-none">＋</span>
                افزودن ریشه
              </button>
            )}

            {crud.hasChanges && (
              <button
                onClick={crud.handleResetChanges}
                disabled={crud.saving}
                className="px-4 py-2 border border-gray-300 rounded-lg text-sm hover:bg-gray-100"
              >
                انصراف و بازنشانی
              </button>
            )}
            <button
              onClick={() => crud.handleSaveChanges()}
              disabled={!crud.hasChanges || crud.saving}
              className={`px-5 py-2 rounded-lg text-sm font-medium shadow-sm ${
                crud.hasChanges
                  ? "bg-blue-600 hover:bg-blue-700 text-white"
                  : "bg-gray-200 text-gray-400 cursor-not-allowed"
              }`}
            >
              {crud.saving ? "در حال ذخیره..." : "ذخیره تغییرات"}
            </button>
          </div>
        </div>

        {crud.error && (
          <div className="mt-4 p-3 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg">
            {crud.error}
          </div>
        )}
        {crud.successMessage && (
          <div className="mt-4 p-3 bg-green-50 border border-green-200 text-green-700 text-sm rounded-lg">
            {crud.successMessage}
          </div>
        )}

        {(enableSearch || enableExpandCollapseAll) && (
          <div className="flex flex-wrap items-center justify-between gap-4 mt-5 pt-4 border-t border-gray-100">
            {enableSearch && (
              <div className="w-72">
                <input
                  type="text"
                  placeholder="جستجوی کلی..."
                  value={crud.globalSearch}
                  onChange={(e) => crud.setGlobalSearch(e.target.value)}
                  className="w-full px-3 py-1.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 outline-none"
                />
              </div>
            )}
            {enableExpandCollapseAll && (
              <div className="flex items-center gap-2 text-xs text-gray-500">
                <button
                  onClick={crud.expandAll}
                  className="px-3 py-1.5 text-xs bg-gray-100 hover:bg-gray-200 rounded border border-gray-300"
                >
                  گسترش همه
                </button>
                <button
                  onClick={crud.collapseAll}
                  className="px-3 py-1.5 text-xs bg-gray-100 hover:bg-gray-200 rounded border border-gray-300"
                >
                  جمع‌کردن همه
                </button>
              </div>
            )}
          </div>
        )}
      </div>

      {/* ─── منطقه رهاسازی ریشه (فقط اگر درگ فعال باشد) ─── */}
      {enableDragDrop && (
        <div
          onDragOver={(e) => {
            e.preventDefault();
            if (crud.draggedIds.length) crud.handleDragStartOverRootZone(true);
          }}
          onDragLeave={() => crud.handleDragStartOverRootZone(false)}
          onDrop={crud.handleDropOnRoot}
          className={`sticky top-0 z-30 mb-2 h-[34px] px-3 border border-dashed rounded-lg text-center text-xs flex items-center justify-center transition-all ${
            crud.isOverRootZone
              ? "border-blue-500 bg-blue-100/95 text-blue-800 font-bold"
              : "border-gray-300 bg-white/95 text-gray-600 hover:border-gray-400"
          }`}
        >
          📌 جهت انتقال موارد انتخاب‌شده به بالاترین سطح، اینجا رها کنید.
        </div>
      )}

      {/* ─── جدول ─── */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-visible">
        <table className="w-full text-right border-collapse">
          <thead>
            <tr className="border-b border-gray-200 text-gray-700 text-xs font-semibold">
              {enableDragDrop && (
                <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-3 w-10 text-center">
                  جابه‌جایی
                </th>
              )}
              {columns.map((col, idx) => (
                <th
                  key={String(col.key)}
                  className={`sticky top-[34px] z-20 bg-gray-100 py-2 h-[38px] px-4 border-b border-gray-200 ${
                    idx === 0 ? "min-w-[220px]" : "min-w-[140px]"
                  }`}
                >
                  {col.label}
                </th>
              ))}
              {enableStatusColumn && (
                <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-4 w-24 text-center">
                  وضعیت
                </th>
              )}
              {showActionColumn && (
                <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-3 w-20 text-center">
                  عملیات
                </th>
              )}
            </tr>

            {enableColumnFilter && (
              <tr className="border-b border-gray-200">
                {enableDragDrop && (
                  <th className="top-[38px] z-20 bg-gray-50 py-1.5 px-2" />
                )}
                {columns.map((col) => (
                  <th
                    key={String(col.key)}
                    className="top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top"
                  >
                    <input
                      type="text"
                      placeholder={`سرچ ${col.label}...`}
                      value={crud.columnFilters[String(col.key)] || ""}
                      onChange={(e) =>
                        crud.handleColumnFilterChange(
                          String(col.key),
                          e.target.value
                        )
                      }
                      className="w-full px-2 py-1 text-xs font-normal border border-gray-300 rounded focus:border-blue-500 outline-none"
                    />
                  </th>
                ))}
                {enableStatusColumn && (
                  <th className="top-[38px] z-20 bg-gray-50 py-1.5 px-2" />
                )}
                {showActionColumn && (
                  <th className="top-[38px] z-20 bg-gray-50 py-1.5 px-2" />
                )}
              </tr>
            )}
          </thead>

          <tbody className="divide-y divide-gray-100 text-sm">
            {crud.loading ? (
              <tr>
                <td colSpan={colCount} className="text-center py-12 text-gray-400">
                  در حال بارگذاری...
                </td>
              </tr>
            ) : crud.flattenedTree.length === 0 ? (
              <tr>
                <td colSpan={colCount} className="text-center py-12 text-gray-400">
                  رکوردی یافت نشد.
                </td>
              </tr>
            ) : (
              crud.flattenedTree.map((row) => {
                const rowId = String(row.node.id);
                const isNewRow = crud.newItemIds.has(rowId); // ← NEW

                return (
                  <tr
                    key={rowId}
                    onClick={(e) => crud.handleRowClick(e, rowId)}
                    onDragOver={(e) =>
                      enableDragDrop && crud.handleDragOverRow(e, rowId)
                    }
                    onDragLeave={() =>
                      crud.dragOverId === rowId && crud.handleDragOverId(null)
                    }
                    onDrop={(e) =>
                      enableDragDrop && crud.handleDropOnRow(e, rowId)
                    }
                    className={`transition-colors ${
                      isNewRow ? "bg-emerald-50/60" : "cursor-pointer"
                    } ${row.isSelected ? "bg-blue-100/70 font-medium" : ""} ${
                      row.isDragging ? "opacity-30 bg-gray-200" : ""
                    } ${
                      row.isDragOver
                        ? "bg-blue-200 border-y-2 border-blue-600"
                        : !isNewRow
                        ? "hover:bg-gray-50/80"
                        : ""
                    } ${
                      row.isModified && !row.isSelected ? "bg-amber-50/40" : ""
                    } ${!row.matchesSearch ? "opacity-60" : ""}`}
                  >
                    {enableDragDrop && (
                      <td className="py-2 px-2 text-center">
                        {!isNewRow && (
                          <div
                            draggable
                            onDragStart={(e) => crud.handleDragStart(e, rowId)}
                            className="cursor-grab active:cursor-grabbing text-gray-400 hover:text-gray-700 text-lg inline-block p-1"
                          >
                            ☰
                          </div>
                        )}
                      </td>
                    )}

                    {columns.map((col, idx) => (
                      <td
                        key={String(col.key)}
                        className="py-2 px-4 align-middle"
                      >
                        {idx === 0 ? (
                          <div
                            className="flex items-center gap-1"
                            style={{ paddingRight: `${row.depth * 24}px` }}
                          >
                            {row.hasChildren ? (
                              <button
                                type="button"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  crud.toggleExpand(rowId);
                                }}
                                className="w-5 h-5 flex items-center justify-center rounded text-gray-500 hover:bg-gray-200 text-xs"
                              >
                                {row.isExpanded ? "▼" : "◀"}
                              </button>
                            ) : (
                              <span className="w-5 text-center text-gray-300">
                                •
                              </span>
                            )}

                            {/* دکمه افزودن فرزند inline */}
                            {enableInlineAddChild && !isNewRow && (
                              <button
                                type="button"
                                title={row.canAddChild ? "افزودن فرزند" : "حداکثر سطح مجاز پر شده است"}
                                disabled={!row.canAddChild}
                                onClick={(e) => {
                                  e.stopPropagation();
                                  if (row.canAddChild) crud.handleAddChild(rowId);
                                }}
                                className={`w-5 h-5 flex items-center justify-center rounded font-bold text-sm leading-none transition-colors ${
                                  row.canAddChild
                                    ? "text-emerald-600 hover:bg-emerald-100 cursor-pointer"
                                    : "text-gray-300 cursor-not-allowed"
                                }`}
                              >
                                ＋
                              </button>
                            )}
                            {isNewRow && (
                              <button
                                type="button"
                                title="حذف پیش‌نویس"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  crud.handleDiscardNew(rowId);
                                }}
                                className="w-5 h-5 flex items-center justify-center rounded text-red-600 hover:bg-red-100 font-bold text-sm leading-none"
                              >
                                ×
                              </button>
                            )}

                            {renderCell(col, row.node, rowId)}
                          </div>
                        ) : (
                          renderCell(col, row.node, rowId)
                        )}
                      </td>
                    ))}

                    {enableStatusColumn && (
                      <td className="py-2 px-4 text-center">
                        {isNewRow ? (
                          <span className="inline-block text-[10px] bg-emerald-100 text-emerald-800 border border-emerald-300 px-2 py-0.5 rounded-full">
                            جدید
                          </span>
                        ) : row.isModified ? (
                          <span className="inline-block text-[10px] bg-amber-100 text-amber-800 border border-amber-300 px-2 py-0.5 rounded-full">
                            تغییر یافته
                          </span>
                        ) : (
                          <span className="text-gray-300 text-xs">-</span>
                        )}
                      </td>
                    )}

                    {showActionColumn && (
                      <td className="py-2 px-3 text-center">
                        {enableDelete && !isNewRow && (
                          <button
                            onClick={() => crud.handleOpenDeleteModal(row.node)}
                            title="حذف"
                            className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg"
                          >
                            <svg
                              className="w-4 h-4"
                              fill="none"
                              stroke="currentColor"
                              viewBox="0 0 24 24"
                            >
                              <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth={1.8}
                                d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                              />
                            </svg>
                          </button>
                        )}
                      </td>
                    )}
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* ─── مودال حذف ─── */}
      {crud.deleteTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm overflow-hidden">
            <div className="p-6 text-center">
              <div className="w-12 h-12 rounded-full bg-red-100 text-red-600 mx-auto flex items-center justify-center mb-4">
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                  />
                </svg>
              </div>
              <h3 className="font-bold text-gray-800 text-lg mb-2">تأیید حذف</h3>
              <p className="text-sm text-gray-600 mb-3">
                آیا از حذف «
                <span className="font-semibold text-gray-900">
                  {crud.deleteTarget.title}
                </span>
                » اطمینان دارید؟
              </p>
              {crud.deleteTarget.isModified && (
                <div className="mb-4 p-2.5 bg-amber-50 border border-amber-200 rounded-lg text-amber-800 text-xs">
                  این سطر دارای تغییرات ذخیره‌نشده است.
                </div>
              )}
              <p className="text-xs text-gray-400">این عملیات قابل بازگشت نیست.</p>
            </div>
            <div className="px-5 py-3.5 bg-gray-50 border-t flex gap-3">
              <button
                onClick={crud.handleCloseDeleteModal}
                disabled={crud.isDeleting}
                className="w-full py-2 border border-gray-300 rounded-lg text-xs hover:bg-gray-100"
              >
                انصراف
              </button>
              <button
                onClick={() => crud.handleConfirmDelete()}
                disabled={crud.isDeleting}
                className="w-full py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg text-xs disabled:opacity-50"
              >
                {crud.isDeleting ? "در حال حذف..." : "حذف شود"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}