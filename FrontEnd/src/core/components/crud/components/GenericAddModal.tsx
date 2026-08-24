import React, { useState, useEffect } from "react";
import { GenericColumnDef } from "../types";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { SearchableMultiSelect } from "../../Selection/SearchableMultiSelect";

interface GenericAddModalProps<T> {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (formData: Record<string, any>) => void;
  columns: GenericColumnDef<T>[];
  selectionLists: Record<string, SelectionListDto[]>;
  saving: boolean;
}

export function GenericAddModal<T>({
  isOpen,
  onClose,
  onSubmit,
  columns,
  selectionLists,
  saving,
}: GenericAddModalProps<T>) {
  const [formData, setFormData] = useState<Record<string, any>>({});

  useEffect(() => {
    if (isOpen) {
      const initial: Record<string, any> = {};
      columns.forEach((col) => {
        if (col.type === "multi-select") {
          initial[col.key as string] = [];
        } else if (col.type === "boolean") {
          initial[col.key as string] = false;
        } else {
          initial[col.key as string] = "";
        }
      });
      setFormData(initial);
    }
  }, [isOpen, columns]);

  if (!isOpen) return null;

  const handleChange = (key: string, value: any) => {
    setFormData((prev) => ({ ...prev, [key]: value }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  const editableColumns = columns.filter((col) => col.editable !== false);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-lg rounded-xl bg-white p-6 shadow-xl dark:bg-gray-800">
        <h3 className="mb-4 text-lg font-bold text-gray-900 dark:text-white">
          افزودن رکورد جدید
        </h3>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="max-h-[60vh] overflow-y-auto space-y-3 px-1">
            {editableColumns.map((col) => {
              const key = col.key as string;
              const options = col.selectionKey ? selectionLists[col.selectionKey] || [] : [];

              return (
                <div key={key} className="flex flex-col gap-1">
                  <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                    {col.label} {col.required && <span className="text-red-500">*</span>}
                  </label>

                  {col.type === "select" ? (
                    <select
                      value={formData[key] || ""}
                      required={col.required}
                      onChange={(e) => handleChange(key, e.target.value)}
                      className="w-full rounded-lg border border-gray-300 p-2 text-sm focus:border-blue-500 dark:border-gray-600 dark:bg-gray-700 dark:text-white"
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
                      value={formData[key] || []}
                      onChange={(selected) => handleChange(key, selected)}
                    />
                  ) : col.type === "boolean" ? (
                    <input
                      type="checkbox"
                      checked={!!formData[key]}
                      onChange={(e) => handleChange(key, e.target.checked)}
                      className="h-5 w-5 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                    />
                  ) : (
                    <input
                      type={col.type === "number" ? "number" : col.type === "date" ? "date" : "text"}
                      value={formData[key] || ""}
                      required={col.required}
                      dir={col.dir || "rtl"}
                      onChange={(e) => handleChange(key, e.target.value)}
                      className={`w-full rounded-lg border border-gray-300 p-2 text-sm focus:border-blue-500 dark:border-gray-600 dark:bg-gray-700 dark:text-white ${
                        col.className || ""
                      }`}
                    />
                  )}
                </div>
              );
            })}
          </div>

          <div className="flex justify-end gap-2 pt-4 border-t dark:border-gray-700">
            <button
              type="button"
              onClick={onClose}
              className="rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
            >
              انصراف
            </button>
            <button
              type="submit"
              disabled={saving}
              className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {saving ? "در حال ثبت..." : "ثبت رکورد"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}