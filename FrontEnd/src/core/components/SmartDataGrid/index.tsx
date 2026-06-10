import React from 'react';
import { SmartDataGridProps } from './SmartDataGrid.types';
import { useGridInstance } from './core/useGridInstance';
import { SmartDataGridProvider } from './core/SmartDataGridContext';
import { SmartDataGridCore } from './core/SmartDataGridCore';

// ایمپورت هوک‌های منطقی پلاگین‌ها
import { useGridSorting } from './plugins/sorting/useGridSorting';
import { useGridPagination } from './plugins/pagination/useGridPagination';
import { useGridEditing } from './plugins/editing/useGridEditing';
import { useGridExcel } from './plugins/excel/useGridExcel';

// ایمپورت کامپوننت‌های ظاهری خالص پلاگین‌ها
import { GridPaginationUI } from './plugins/pagination/GridPaginationUI';
import { ExcelToolbarUI } from './plugins/excel/ExcelToolbarUI';

export function SmartDataGrid<T>(props: SmartDataGridProps<T>) {
  // ۱. راه‌اندازی هسته ساده
  const instance = useGridInstance({
    data: props.data,
    columns: props.columns,
    keyExtractor: props.keyExtractor
  });

  // ۲. تزریق منطق مستقل پلاگین‌ها به هسته کانتکست (بدون تداخل با UI)
  useGridSorting(instance);
  useGridPagination(instance, { pageSize: props.pageSize });
  useGridExcel(instance, {
  allowImport: props.allowExcelImport,
  allowExport: props.allowExcelExport,
  columns: props.columns,
  keyExtractor: props.keyExtractor,
  emptyRowFactory: props.emptyRowFactory
});
  useGridEditing(instance, {
    allowEdit: props.allowEdit,
    allowDelete: props.allowDelete,
    validateRow: props.validateRow,
    emptyRowFactory: props.emptyRowFactory,
    onSaveRow: props.onSaveRow,
    onSaveBatch: props.onSaveBatch
  });

  // ۳. ایجاد لایوت ترکیبی کامپوننت‌های لوکال لایه ویو (فقط رندر دکمه‌های متناظر)
  const ToolbarSlot = (
    <div className="flex flex-wrap justify-between items-center gap-2 bg-gray-50 p-3 rounded-lg border border-gray-200">
      <div className="flex gap-2">
        {instance.pluginState.editing?.allowAdd && (
          <button onClick={instance.actions.handleAddNewRow} className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded shadow transition">
            + افزودن سطر جدید
          </button>
        )}
        <ExcelToolbarUI />
      </div>
      {instance.pluginState.editing?.isDirty && (
        <button onClick={instance.actions.processSaveBatch} className="bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-bold px-5 py-2 rounded shadow transition">
          💾 ذخیره کل تغییرات
        </button>
      )}
    </div>
  );

  return (
    <SmartDataGridProvider value={instance}>
      <SmartDataGridCore
        className={props.className}
        emptyMessage={props.emptyMessage}
        toolbarSlot={ToolbarSlot}
        paginationSlot={<GridPaginationUI />}
      />
    </SmartDataGridProvider>
  );
}

export default SmartDataGrid;