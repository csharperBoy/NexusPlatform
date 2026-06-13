// src/core/components/SmartDataGrid/plugins/actions/RowActionsUI.tsx
import React from 'react';
import { useGridContext } from '../../core/SmartDataGridContext';

export function RowActionsUI({ rowKey, rowIndex, row }: { rowKey: string | number, rowIndex: number, row: any }) {
  const instance = useGridContext();
  
  // بررسی استیت‌های پلاگین‌ها
  const actionsKeys = instance.pluginState.actions?.actionsKeys || new Set();
  const isEditing = actionsKeys.has(rowKey);
  
  // بررسی اینکه آیا پلاگین درختی فعال است یا نه
  const isTreeEnabled = !!instance.pluginState.tree; 

  if (isEditing) {
    return (
      <div className="flex gap-3 justify-end items-center">
        <button onClick={() => instance.actions.processSaveRow?.(row, rowIndex)} className="text-emerald-600 hover:text-emerald-800 font-medium text-xs flex items-center gap-1">✔ تایید</button>
        <button onClick={() => instance.actions.cancelEditing(rowKey, rowIndex)} className="text-gray-500 hover:text-gray-700 font-medium text-xs">✖ انصراف</button>
      </div>
    );
  }

  return (
    <div className="flex gap-3 justify-end items-center">
      {/* دکمه افزودن فرزند (فقط اگر حالت درختی روشن باشد) */}
      {isTreeEnabled && (
        <button 
          onClick={() => instance.actions.addChildRow?.(rowKey)} 
          className="text-emerald-600 hover:text-emerald-800 text-xs font-bold"
          title="افزودن زیرمجموعه"
        >
          ➕ زیرمجموعه
        </button>
      )}
      
      <button onClick={() => instance.actions.startEditing(rowKey)} className="text-blue-600 hover:text-blue-800 text-xs">ویرایش</button>
      <button onClick={() => instance.actions.handleDeleteRow(rowKey)} className="text-red-600 hover:text-red-800 text-xs">حذف</button>
    </div>
  );
}