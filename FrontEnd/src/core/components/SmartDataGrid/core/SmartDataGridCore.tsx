// src/core/components/SmartDataGrid/core/SmartDataGridCore.tsx
import React, { ReactNode } from 'react';
import { useGridContext } from './SmartDataGridContext';
import SmartDataGridHeader from './SmartDataGridHeader';
import SmartDataGridCell from './SmartDataGridCell';
import { TreeToggleUI } from '../plugins/tree/TreeToggleUI'; // 🌲 اضافه شدن کامپوننت دکمه‌های درخت

interface CoreProps<T> {
  toolbarSlot?: ReactNode;
  paginationSlot?: ReactNode;
  emptyMessage?: string;
  className?: string;
  useTreeStructure?: boolean; 
  treeData?: Array<{ row: T, depth: number, hasChildren: boolean }>; 
}

export function SmartDataGridCore<T>({ 
  toolbarSlot, 
  paginationSlot, 
  emptyMessage = 'داده‌ای وجود ندارد', 
  className = '', 
  useTreeStructure, 
  treeData 
}: CoreProps<T>) {
  const instance = useGridContext<T>();
  const finalColumns = instance.getFinalColumns();
  const processedData = instance.getProcessedData();
  
  const actionsKeys = instance.pluginState.actions?.actionsKeys || new Set();
  const addedKeys = instance.pluginState.actions?.addedKeys || new Set();
  const rowErrors = instance.pluginState.actions?.rowErrors || new Map();

  // 🛠️ یکپارچه‌سازی منبع داده برای رندر: اگر درختی بود از treeData وگرنه از processedData استفاده مینویسیم
  const displayRows = useTreeStructure && treeData && treeData.length > 0
    ? treeData
    : processedData.map(row => ({ row, depth: 0, hasChildren: false }));

  return (
    <div className={`w-full flex flex-col gap-4 ${className}`} dir="rtl">
      {toolbarSlot && <div className="w-full">{toolbarSlot}</div>}
      
      <div className="overflow-x-auto border border-gray-200 rounded-lg shadow bg-white">
        <table className="min-w-full border-collapse text-right text-sm">
          <SmartDataGridHeader columns={finalColumns} />
          <tbody>
            {displayRows.length === 0 ? (
              <tr>
                <td colSpan={finalColumns.length} className="text-center py-10 text-gray-400">
                  {emptyMessage}
                </td>
              </tr>
            ) : (
              displayRows.map((item, idx) => {
                const { row, depth, hasChildren } = item; // 🌲 استخراج اطلاعات درخت برای هر ردیف
                const key = instance.keyExtractor(row, idx);
                const isEditing = actionsKeys.has(key);
                const hasError = rowErrors.has(key);

                let rowClassName = "border-b hover:bg-gray-50 transition-colors";
                if (hasError) rowClassName = "border-b bg-red-50 hover:bg-red-100";
                else if (addedKeys.has(key)) rowClassName = "border-b bg-green-50/60 hover:bg-green-100/60";
                else if (isEditing) rowClassName = "border-b bg-amber-50/40";

                return (
                  <tr key={key} className={rowClassName}>
                    {finalColumns.map((col, colIdx) => (
                      <td key={String(col.id)} className="p-3 border-b max-w-xs overflow-hidden text-ellipsis">
                        {/* 🌲 اگر حالت درختی فعال است و به ستون اول (ایندکس 0) رسیدیم، سلول را درون دکمه‌های درخت کپسوله کن */}
                        {useTreeStructure && colIdx === 0 ? (
                          <TreeToggleUI rowKey={key} depth={depth} hasChildren={hasChildren}>
                            <SmartDataGridCell row={row} column={col} index={idx} isEditing={isEditing} rowKey={key} />
                          </TreeToggleUI>
                        ) : (
                          <SmartDataGridCell row={row} column={col} index={idx} isEditing={isEditing} rowKey={key} />
                        )}
                      </td>
                    ))}
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {paginationSlot && <div className="w-full">{paginationSlot}</div>}
    </div>
  );
}