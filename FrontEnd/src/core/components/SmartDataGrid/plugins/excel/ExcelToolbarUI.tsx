// src/core/components/SmartDataGrid/plugins/excel/ExcelToolbarUI.tsx
import React, { useRef } from 'react';
import { useGridContext } from '../../core/SmartDataGridContext';

export function ExcelToolbarUI() {
  const instance = useGridContext();
  const excel = instance.pluginState.excel;
  const fileInputRef = useRef<HTMLInputElement>(null);

  if (!excel) return null;

  return (
    <div className="flex gap-2">
      {excel.allowImport && (
        <>
          <input 
            type="file" 
            ref={fileInputRef}
            className="hidden" 
            accept=".xlsx, .xls" 
            onChange={(e) => {
              const file = e.target.files?.[0];
              if (file) {
                // صدا زدن متد ایمپورت جدید با ارسال مستقیم آبجکت فایل
                instance.actions.excelImport?.(file);
                e.target.value = ''; // ریسیت آدرس فایل
              }
            }}
          />
          <button 
            onClick={() => fileInputRef.current?.click()} 
            className="bg-sky-600 hover:bg-sky-700 text-white text-sm font-medium px-4 py-2 rounded shadow transition flex items-center gap-1"
          >
            📥 ورود از اکسل
          </button>
        </>
      )}

      {excel.allowExport && (
        <button 
          onClick={instance.actions.excelExport} 
          className="bg-amber-600 hover:bg-amber-700 text-white text-sm font-medium px-4 py-2 rounded shadow transition flex items-center gap-1"
        >
          📤 خروجی اکسل
        </button>
      )}
    </div>
  );
}