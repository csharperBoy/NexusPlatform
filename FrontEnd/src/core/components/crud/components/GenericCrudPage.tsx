//src/core/components/crud/components/GenericCrudPage.tsx
import React from "react";
import { BaseEntity, GenericColumnDef, TableFeatures } from "../types";
import { useGenericCrud, UseGenericCrudOptions } from "../hooks/useGenericCrud";
import { GenericAddModal } from "./GenericAddModal";

interface GenericCrudPageProps<T extends BaseEntity, TCreateCmd, TUpdateCmd> {
  title: string;
  columns: GenericColumnDef<T>[];
  crudOptions: UseGenericCrudOptions<T, TCreateCmd, TUpdateCmd>;
}

export const GenericCrudPage = <T extends BaseEntity, TCreateCmd = any, TUpdateCmd = any>({
  title,
  columns,
  crudOptions,
}: GenericCrudPageProps<T, TCreateCmd, TUpdateCmd>) => {
  const crud = useGenericCrud(crudOptions);
  const features: TableFeatures<T> = crudOptions.features || {
    enableAdd: true,
    enableDelete: true,
    enableBatchSave: true,
    enableGlobalSearch: true,
  };

  if (crud.loading) {
    return (
      <div className="flex items-center justify-center min-h-[400px] text-gray-500 dir-rtl">
        در حال بارگذاری اطلاعات...
      </div>
    );
  }

  return (
    <div className="p-6 space-y-4 dir-rtl bg-gray-50/50 min-h-screen">
      {/* هدر صفحه و دکمه‌های عملیاتی */}
      <div className="flex flex-wrap justify-between items-center bg-white p-4 rounded-xl shadow-sm border border-gray-100 gap-4">
        <div>
          <h1 className="text-xl font-bold text-gray-800">{title}</h1>
          <p className="text-xs text-gray-400 mt-1">مدیریت و ویرایش اطلاعات جدول</p>
        </div>

        <div className="flex items-center gap-2 flex-wrap">
          {features.enableExcelImport && features.excelMapper && (
            <>
              <input
                type="file"
                ref={crud.fileInputRef}
                onChange={(crud as any).handleExcelImport}
                accept=".xlsx, .xls"
                className="hidden"
              />
              <button
                onClick={() => crud.fileInputRef.current?.click()}
                className="px-3.5 py-2 text-sm font-medium text-emerald-700 bg-emerald-50 rounded-lg border border-emerald-200 hover:bg-emerald-100"
              >
                📥 ورود از اکسل
              </button>
            </>
          )}

          {features.enableAdd && (
            <button
              onClick={() => crud.setIsAddModalOpen(true)}
              className="px-3.5 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 shadow-sm"
            >
              ➕ رکورد جدید
            </button>
          )}

          {features.enableBatchSave && crud.modifiedIds.size > 0 && (
            <>
              <button
                onClick={crud.handleResetChanges}
                className="px-3.5 py-2 text-sm font-medium text-gray-600 bg-gray-100 rounded-lg hover:bg-gray-200"
              >
                لغو تغییرات
              </button>
              <button
                onClick={crud.handleSaveChanges}
                disabled={crud.saving}
                className="px-3.5 py-2 text-sm font-medium text-white bg-emerald-600 rounded-lg hover:bg-emerald-700 shadow-sm disabled:opacity-50"
              >
                {crud.saving ? "در حال ذخیره..." : `💾 ذخیره (${crud.modifiedIds.size})`}
              </button>
            </>
          )}
        </div>
      </div>

      {/* پیام‌های سیستم */}
      {crud.error && (
        <div className="p-4 bg-rose-50 border border-rose-200 text-rose-700 text-sm rounded-xl flex justify-between items-center">
          <span>{crud.error}</span>
          <button onClick={() => crud.loadData()} className="underline text-xs font-semibold">
            تلاش مجدد
          </button>
        </div>
      )}
      {crud.successMessage && (
        <div className="p-4 bg-emerald-50 border border-emerald-200 text-emerald-700 text-sm rounded-xl">
          {crud.successMessage}
        </div>
      )}

      {/* نوار جستجوی سراسری */}
      {features.enableGlobalSearch !== false && (
        <div className="bg-white p-3 rounded-xl shadow-sm border border-gray-100">
          <input
            type="text"
            placeholder="🔍 جستجوی کلی در تمامی ستون‌ها..."
            value={crud.globalSearch}
            onChange={(e) => crud.setGlobalSearch(e.target.value)}
            className="w-full max-w-md px-3.5 py-2 border rounded-lg text-sm bg-gray-50/50 focus:bg-white focus:ring-2 focus:ring-blue-500 outline-none transition-all"
          />
        </div>
      )}

      {/* جدول داده‌ها */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-x-auto">
        <table className="w-full text-right border-collapse text-sm">
          <thead>
            <tr className="bg-gray-50/80 border-b border-gray-100 text-gray-600">
              {columns.map((col) => (
                <th key={String(col.key)} style={{ width: col.width }} className="p-3.5 font-semibold">
                  {col.title}
                </th>
              ))}
              {features.enableDelete && <th className="p-3.5 font-semibold w-20">عملیات</th>}
            </tr>

            {/* سطر فیلتر ستونی */}
            <tr className="bg-gray-50/30 border-b border-gray-100">
              {columns.map((col) => {
                const key = String(col.key);
                return (
                  <td key={key} className="p-2">
                    {col.searchable !== false && (
                      <input
                        type="text"
                        placeholder={`فیلتر ${col.title}...`}
                        value={crud.columnSearch[key] || ""}
                        onChange={(e) => crud.setColumnSearch(key, e.target.value)}
                        className="w-full px-2.5 py-1 text-xs border rounded-md bg-white focus:ring-1 focus:ring-blue-500 outline-none"
                      />
                    )}
                  </td>
                );
              })}
              {features.enableDelete && <td />}
            </tr>
          </thead>

          <tbody className="divide-y divide-gray-100">
            {crud.items.length === 0 ? (
              <tr>
                <td
                  colSpan={columns.length + (features.enableDelete ? 1 : 0)}
                  className="p-8 text-center text-gray-400"
                >
                  هیچ داده‌ای یافت نشد.
                </td>
              </tr>
            ) : (
              crud.items.map((item) => {
                const isModified = crud.modifiedIds.has(item.id);

                return (
                  <tr
                    key={item.id}
                    className={`transition-colors ${
                      isModified ? "bg-amber-50/60 hover:bg-amber-50" : "hover:bg-gray-50/80"
                    }`}
                  >
                    {columns.map((col) => {
                      const key = String(col.key);
                      const isEditable = col.editable !== false;
                      const options = crud.selections[col.optionsKey || key] || [];

                      return (
                        <td key={key} className="p-3.5">
                          {col.renderEditCell ? (
                            col.renderEditCell(item, (val) => crud.handleFieldChange(item.id, key as keyof T, val))
                          ) : isEditable ? (
                            col.type === "select" ? (
                              <select
                                value={item[key] || ""}
                                onChange={(e) => crud.handleFieldChange(item.id, key as keyof T, e.target.value)}
                                className="w-full px-2.5 py-1.5 border rounded-lg text-sm bg-white focus:ring-1 focus:ring-blue-500 outline-none"
                              >
                                <option value="">انتخاب کنید...</option>
                                {options.map((opt) => (
                                  <option key={opt.value} value={opt.value}>
                                    {opt.display || opt.label}
                                  </option>
                                ))}
                              </select>
                            ) : (
                              <input
                                type={col.type === "number" ? "number" : "text"}
                                value={item[key] ?? ""}
                                onChange={(e) =>
                                  crud.handleFieldChange(
                                    item.id,
                                    key as keyof T,
                                    col.type === "number" ? e.target.valueAsNumber || e.target.value : e.target.value
                                  )
                                }
                                className="w-full px-2.5 py-1.5 border rounded-lg text-sm bg-white focus:ring-1 focus:ring-blue-500 outline-none"
                              />
                            )
                          ) : col.render ? (
                            col.render(item)
                          ) : col.type === "select" ? (
                            crud.selectionMaps[col.optionsKey || key]?.get(String(item[key])) || item[key]
                          ) : (
                            String(item[key] ?? "")
                          )}
                        </td>
                      );
                    })}

                    {features.enableDelete && (
                      <td className="p-3.5 text-center">
                        <button
                          onClick={() =>
                            crud.setDeleteTarget({
                              id: item.id,
                              title: item.title || item.name || item.id,
                            })
                          }
                          className="text-rose-500 hover:text-rose-700 p-1.5 hover:bg-rose-50 rounded-lg transition-colors"
                          title="حذف"
                        >
                          🗑️
                        </button>
                      </td>
                    )}
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* مودال ایجاد */}
      {features.enableAdd && (
        <GenericAddModal
          isOpen={crud.isAddModalOpen}
          onClose={() => crud.setIsAddModalOpen(false)}
          onSubmit={crud.handleCreate}
          columns={columns}
          selections={crud.selections}
          saving={crud.saving}
        />
      )}

      {/* مودال تأیید حذف */}
      {crud.deleteTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4 dir-rtl">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-md p-6 space-y-4 border border-gray-100">
            <h3 className="text-lg font-bold text-gray-800">تأیید حذف</h3>
            <p className="text-sm text-gray-600">
              آیا از حذف رکورد <strong>«{crud.deleteTarget.title}»</strong> اطمینان دارید؟
            </p>
            <div className="flex justify-end gap-3 pt-2">
              <button
                onClick={() => crud.setDeleteTarget(null)}
                className="px-4 py-2 text-sm text-gray-600 bg-gray-100 rounded-lg hover:bg-gray-200"
              >
                انصراف
              </button>
              <button
                onClick={crud.handleConfirmDelete}
                disabled={crud.isDeleting}
                className="px-4 py-2 text-sm text-white bg-rose-600 rounded-lg hover:bg-rose-700 disabled:opacity-50"
              >
                {crud.isDeleting ? "در حال حذف..." : "حذف رکورد"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};