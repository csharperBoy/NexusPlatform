import React, { useState } from "react";
import { GenericColumnDef } from "../types";
import { SelectionListDto } from "@/core/models/SelectionListDto";

interface GenericAddModalProps<T> {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: any) => Promise<void>;
  columns: GenericColumnDef<T>[];
  selections: Record<string, SelectionListDto[]>;
  saving: boolean;
}

export const GenericAddModal = <T,>({
  isOpen,
  onClose,
  onSubmit,
  columns,
  selections,
  saving,
}: GenericAddModalProps<T>) => {
  const editableColumns = columns.filter((col) => col.editable !== false);
  const [formData, setFormData] = useState<Record<string, any>>({});

  if (!isOpen) return null;

  const handleChange = (key: string, value: any) => {
    setFormData((prev) => ({ ...prev, [key]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await onSubmit(formData);
    setFormData({});
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4 dir-rtl">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-lg overflow-hidden border border-gray-100">
        <div className="flex justify-between items-center px-6 py-4 bg-gray-50 border-b border-gray-100">
          <h3 className="font-bold text-gray-800 text-lg">افزودن رکورد جدید</h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            ✕
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4 max-h-[70vh] overflow-y-auto">
          {editableColumns.map((col) => {
            const key = String(col.key);
            const options = selections[col.optionsKey || key] || [];

            return (
              <div key={key} className="flex flex-col gap-1.5">
                <label className="text-sm font-medium text-gray-700">{col.title}</label>

                {col.type === "select" ? (
                  <select
                    value={formData[key] || ""}
                    onChange={(e) => handleChange(key, e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg text-sm bg-white focus:ring-2 focus:ring-blue-500 outline-none"
                    required
                  >
                    <option value="">انتخاب کنید...</option>
                    {options.map((opt) => (
                      <option key={opt.value} value={opt.value}>
                        {opt.display || opt.label}
                      </option>
                    ))}
                  </select>
                ) : col.type === "number" ? (
                  <input
                    type="number"
                    value={formData[key] || ""}
                    onChange={(e) => handleChange(key, e.target.valueAsNumber || e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-blue-500 outline-none"
                    required
                  />
                ) : (
                  <input
                    type="text"
                    value={formData[key] || ""}
                    onChange={(e) => handleChange(key, e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-blue-500 outline-none"
                    required
                  />
                )}
              </div>
            );
          })}

          <div className="flex justify-end gap-3 pt-4 border-t border-gray-100">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm text-gray-600 bg-gray-100 rounded-lg hover:bg-gray-200"
            >
              انصراف
            </button>
            <button
              type="submit"
              disabled={saving}
              className="px-4 py-2 text-sm text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50"
            >
              {saving ? "در حال ثبت..." : "ثبت اطلاعات"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};