import React from 'react';
import { useGridContext } from '../../core/SmartDataGridContext';

export function RowActionsUI({ rowKey, rowIndex, row }: { rowKey: string | number, rowIndex: number, row: any }) {
  const instance = useGridContext();
  const editingKeys = instance.pluginState.editing?.editingKeys || new Set();
  const isEditing = editingKeys.has(rowKey);

  if (isEditing) {
    return (
      <div className="flex gap-3 justify-end">
        <button 
          onClick={() => instance.actions.saveRow?.(rowKey, rowIndex, row)} 
          className="text-emerald-600 hover:text-emerald-800 font-medium text-xs flex items-center gap-1"
        >
          ✔ تایید
        </button>
      
      <button 
          onClick={() => instance.actions.cancelEditing(rowKey, rowIndex)} 
          className="text-gray-500 hover:text-gray-700 font-medium text-xs"
        >
          ✖ انصراف
        </button>
        </div>
    );
  }

  return (
    <div className="flex gap-3 justify-end">
      <button onClick={() => instance.actions.startEditing(rowKey)} className="text-blue-600 hover:text-blue-800 text-xs">ویرایش</button>
      <button onClick={() => instance.actions.handleDeleteRow(rowKey)} className="text-red-600 hover:text-red-800 text-xs">حذف</button>
    </div>
  );
}