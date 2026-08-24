import React from "react";
import { BaseEntity, GenericColumnDef, UseGenericCrudOptions } from "./types";
import { useGenericCrud } from "./useGenericCrud";
import { GenericAddModal } from "./GenericAddModal";
import { SearchableMultiSelect } from "@/core/components/SearchableMultiSelect";

interface GenericCrudPageProps<T extends BaseEntity, TCreateCmd, TUpdateCmd> {
  title: string;
  columns: GenericColumnDef<T>[];
  crudOptions: UseGenericCrudOptions<T, TCreateCmd, TUpdateCmd>;
}

export function GenericCrudPage<T extends BaseEntity, TCreateCmd, TUpdateCmd>({
  title,
  columns,
  crudOptions,
}: GenericCrudPageProps<T, TCreateCmd, TUpdateCmd>) {
  const crud = useGenericCrud<T, TCreateCmd, TUpdateCmd>({
    ...crudOptions,
    columns,
  });

  const { features } = crudOptions;
  const showSearch = features?.enableSearch !== false;
  const showColumnFilter = features?.enableColumnFilter !== false;

  return (
    <div className="space-y-4 p-6">
      {/* Header & Action Bar */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <h1 className="text-xl font-bold text-gray-900 dark:text-white">{title}</h1>

        <div className="flex flex-wrap items-center gap-2">
          {crud.hasChanges && (
            <span className="rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold text-amber-800 dark:bg-amber-900/30 dark:text-amber-400">
              {crud.modifiedCount} رکورد تغییر یافته
            </span>
          )}

          <button
            onClick={crud.handleSaveAll}
            disabled={!crud.hasChanges || crud.saving}
            className="rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
          >
            {crud.saving ? "در حال ذخیره..." : "ذخیره تغییرات"}
          </button>

          <button
            onClick={() => crud.setIsAddModalOpen(true)}
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
          >
            افزودن جدید
          </button>
        </div>
      </div>

      {/* Global Search Input */}
      {showSearch && (
        <div className="max-w-md">
          <input
            type="text"
            placeholder="جستجوی کلی..."
            value={crud.globalSearch}
            onChange={(e) => crud.setGlobalSearch(e.target.value)}
            className="w-full rounded-lg border border-gray-300 p-2 text-sm focus:border-blue-500 dark:border-gray-600 dark:bg-gray-700 dark:text-white"
          />
        </div>
      )}

      {/* Data Table */}
      <div className="overflow-x-auto rounded-lg border border-gray-200 shadow-sm dark:border-gray-700">
        <table className="w-full text-right text-sm text-gray-700 dark:text-gray-300">
          <thead className="bg-gray-50 text-xs text-gray-700 dark:bg-gray-700 dark:text-gray-300">
            <tr>
              {columns.map((col) => (
                <th key={String(col.key)} className="p-3">
                  <div className="flex flex-col gap-2">
                    <span>{col.label}</span>
                    {showColumnFilter && (
                      <input
                        type="text"
                        placeholder="فیلتر..."
                        value={crud.columnFilters[String(col.key)] || ""}
                        onChange={(e) =>
                          crud.setColumnFilters((prev) => ({
                            ...prev,
                            [String(col.key)]: e.target.value,
                          }))
                        }
                        className="w-full rounded border border-gray-300 p-1 text-xs font-normal dark:border-gray-600 dark:bg-gray-800 dark:text-white"
                      />
                    )}
                  </div>
                </th>
              ))}
              <th className="p-3 text-center">عملیات</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {crud.loading ? (
              <tr>
                <td colSpan={columns.length + 1} className="p-4 text-center">
                  در حال بارگذاری داده‌ها...
                </td>
              </tr>
            ) : crud.items.length === 0 ? (
              <tr>
                <td colSpan={columns.length + 1} className="p-4 text-center">
                  رکوردی یافت نشد.
                </td>
              </tr>
            ) : (
              crud.items.map((item) => (
                <tr key={item.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
                  {columns.map((col) => {
                    const key = col.key as keyof T;
                    const value = item[key];
                    const options = col.selectionKey
                      ? crud.selectionLists[col.selectionKey] || []
                      : [];

                    return (
                      <td key={String(key)} className="p-2">
                        {col.editable !== false ? (
                          col.type === "select" ? (
                            <select
                              value={(value as string) || ""}
                              onChange={(e) =>
                                crud.handleFieldChange(item.id, key, e.target.value)
                              }
                              className="w-full rounded border border-gray-300 p-1.5 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                            >
                              <option value="">انتخاب کنید...</option>
                              {options.map((opt) => (
                                <option key={opt.value} value={opt.value}>
                                  {opt.display || opt.label}
                                </option>
                              ))}
                            </select>
                          ) : col.type === "multi-select" ? (
                            <SearchableMultiSelect
                              options={options}
                              value={(value as string[]) || []}
                              onChange={(selected) =>
                                crud.handleFieldChange(item.id, key, selected)
                              }
                            />
                          ) : col.type === "boolean" ? (
                            <input
                              type="checkbox"
                              checked={!!value}
                              onChange={(e) =>
                                crud.handleFieldChange(item.id, key, e.target.checked)
                              }
                              className="h-4 w-4 rounded border-gray-300 text-blue-600"
                            />
                          ) : (
                            <input
                              type={col.type === "number" ? "number" : "text"}
                              value={(value as string) || ""}
                              dir={col.dir || "rtl"}
                              onChange={(e) =>
                                crud.handleFieldChange(item.id, key, e.target.value)
                              }
                              className={`w-full rounded border border-gray-300 p-1.5 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white ${
                                col.className || ""
                              }`}
                            />
                          )
                        ) : col.render ? (
                          col.render(value, item)
                        ) : (
                          <span dir={col.dir || "rtl"} className={col.className}>
                            {String(value ?? "")}
                          </span>
                        )}
                      </td>
                    );
                  })}
                  <td className="p-2 text-center">
                    <button
                      onClick={() => crud.prepareDelete(item)}
                      className="rounded p-1 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/30"
                    >
                      حذف
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Add Modal */}
      <GenericAddModal
        isOpen={crud.isAddModalOpen}
        onClose={() => crud.setIsAddModalOpen(false)}
        onSubmit={crud.handleCreate}
        columns={columns}
        selectionLists={crud.selectionLists}
        saving={crud.saving}
      />

      {/* Delete Confirmation Modal */}
      {crud.deleteTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl dark:bg-gray-800">
            <h3 className="mb-2 text-lg font-bold text-gray-900 dark:text-white">
              تایید حذف
            </h3>
            <p className="mb-4 text-sm text-gray-600 dark:text-gray-300">
              آیا از حذف این رکورد اطمینان دارید؟
            </p>

            {crud.deleteTarget.isModified && (
              <div className="mb-4 rounded-lg bg-amber-50 p-3 text-xs text-amber-800 dark:bg-amber-900/30 dark:text-amber-300">
                ⚠️ این رکورد دارای تغییرات ذخیره‌نشده است. با حذف آن، تغییرات نیز از دست خواهند رفت.
              </div>
            )}

            <div className="flex justify-end gap-2">
              <button
                onClick={() => crud.setDeleteTarget(null)}
                className="rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300"
              >
                انصراف
              </button>
              <button
                onClick={crud.confirmDelete}
                disabled={crud.saving}
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
              >
                {crud.saving ? "در حال حذف..." : "حذف قطعی"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}