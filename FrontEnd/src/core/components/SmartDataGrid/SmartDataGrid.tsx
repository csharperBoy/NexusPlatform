// src/core/components/SmartDataGrid/SmartDataGrid.tsx
import React, { useEffect } from 'react';
import { SmartDataGridProps } from './SmartDataGrid.types';
import { useGridInstance } from './core/useGridInstance';
import { SmartDataGridProvider } from './core/SmartDataGridContext';
import { SmartDataGridCore } from './core/SmartDataGridCore';

import { useGridSorting } from './plugins/sorting/useGridSorting';
import { useGridPagination } from './plugins/pagination/useGridPagination';
import { useGridAction } from './plugins/action/useGridAction';
import { useGridExcel } from './plugins/excel/useGridExcel';
import { useGridTree } from './plugins/tree/useGridTree';

import { GridPaginationUI } from './plugins/pagination/GridPaginationUI';
import { ExcelToolbarUI } from './plugins/excel/ExcelToolbarUI';

export function SmartDataGrid<T>(props: SmartDataGridProps<T>) {
  const { data, columns, keyExtractor, pageSize, className, emptyMessage, actionConfig, excelConfig, treeConfig } = props;

  // ۱. راه‌اندازی هسته
  const instance = useGridInstance({ data, columns, keyExtractor });

  // ۲. تزریق هوشمند آبجکت‌های کانفیگ به پلاگین‌ها
  useGridSorting(instance);
  
  useGridPagination(instance, { pageSize });
  
  useGridExcel(instance, {
    allowImport: excelConfig?.allowImport,
    allowExport: excelConfig?.allowExport,
    columns,
    keyExtractor,
    emptyRowFactory: actionConfig?.emptyRowFactory // از آبجکت ادیت می‌خواند
  });
  
  useGridAction(instance, {
    allowAdd: actionConfig?.allowAdd,
    allowEdit: actionConfig?.allowEdit,
    allowDelete: actionConfig?.allowDelete,
    validateRow: actionConfig?.validateRow,
    emptyRowFactory: actionConfig?.emptyRowFactory,
    onSaveRow: actionConfig?.onSaveRow,
    onSaveBatch: actionConfig?.onSaveBatch
  });

  const currentPage = instance.pluginState.pagination?.currentPage || 1;

  // ۳. تزریق تمیز آبجکت treeConfig به پلاگین درخت
  const { paginatedTreeData, totalTreePages } = useGridTree(instance, {
    useTreeStructure: treeConfig?.enabled,
    parentKey: treeConfig?.parentKey as keyof T,
    keyExtractor,
    emptyRowFactory: actionConfig?.emptyRowFactory,
    currentPage,
    pageSize
  });

  // هماهنگی صفحات درخت با پیجینشن
  useEffect(() => {
    if (treeConfig?.enabled) {
      instance.setPluginState('pagination', (prev: any) => ({
        ...prev,
        totalPages: totalTreePages
      }));
    }
  }, [treeConfig?.enabled, totalTreePages, instance]);

  const ToolbarSlot = (
    <div className="flex flex-wrap justify-between items-center gap-2 bg-gray-50 p-3 rounded-lg border border-gray-200">
      <div className="flex gap-2">
        {instance.pluginState.actions?.allowAdd && (
          <button onClick={instance.actions.handleAddNewRow} className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded shadow transition">
            + افزودن رکورد اصلی
          </button>
        )}
        <ExcelToolbarUI />
      </div>
      {instance.pluginState.actions?.isDirty && (
        <button onClick={instance.actions.processSaveBatch} className="bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-bold px-5 py-2 rounded shadow transition">
          💾 ذخیره کل تغییرات
        </button>
      )}
    </div>
  );

  return (
    <SmartDataGridProvider value={instance}>
      <SmartDataGridCore
        className={className}
        emptyMessage={emptyMessage}
        toolbarSlot={ToolbarSlot}
        paginationSlot={<GridPaginationUI />}
        useTreeStructure={treeConfig?.enabled}
        treeData={paginatedTreeData}
      />
    </SmartDataGridProvider>
  );
}

export default SmartDataGrid;