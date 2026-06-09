// src/core/components/SmartDataGrid/SmartDataGrid.tsx
import React, { useMemo } from 'react';
import { SmartDataGridProps } from './SmartDataGrid.types';
import SmartDataGridHeader from './SmartDataGridHeader';
import SmartDataGridCell from './SmartDataGridCell';
import { useSmartDataGrid } from './useSmartDataGrid';

function SmartDataGrid<T>(props: SmartDataGridProps<T>) {
  const {
    allowAdd,
    allowEdit,
    allowDelete,
    allowExcelImport,
    onSaveRow,
    onSaveBatch,
    columns,
    keyExtractor,
    emptyMessage = 'داده‌ای وجود ندارد',
    className = '',
  } = props;

  const {
    paginatedData,
    currentPage,
    totalPages,
    setPage,
    sortColumn,
    sortDirection,
    toggleSort,
    editingKeys,
    addedKeys,
    rowErrors,
    isDirty,
    handleAddNewRow,
    handleDeleteRow,
    handleCellChange,
    startEditing,
    cancelEditing,
    handleExcelUpload,
    getBatchChanges,
  } = useSmartDataGrid(props);

  // ستون عملیات سفارشی سازمانی
  const hasActions = allowEdit || allowDelete;
  const finalColumns = useMemo(() => {
    if (!hasActions) return columns;
    const actionsCol = {
      id: '__actions',
      header: 'عملیات',
      width: '140px',
      type: 'custom' as const,
    };
    return [...columns, actionsCol];
  }, [columns, hasActions]);

  // هندل کردن ذخیره تکی یک سطر
  const processSaveRow = async (row: T, idx: number) => {
    const key = keyExtractor(row, idx);
    if (rowErrors.has(key)) {
      alert(`لطفا خطاهای سطر را ابتدا برطرف کنید:\n${rowErrors.get(key)?.join('\n')}`);
      return;
    }
    if (onSaveRow) {
      const type = addedKeys.has(key) ? 'add' : 'edit';
      await onSaveRow(row, type, idx);
    }
    cancelEditing(key, idx);
  };

  // هندل کردن ذخیره کلی سطرها
  const processSaveBatch = async () => {
    if (rowErrors.size > 0) {
      alert('جدول حاوی سطر‌های دارای خطا (قرمز رنگ) است. لطفا ابتدا آن‌ها را اصلاح کنید.');
      return;
    }
    if (onSaveBatch) {
      const changes = getBatchChanges();
      await onSaveBatch(changes);
    }
  };

  return (
    <div className={`w-full flex flex-col gap-4 ${className}`} dir="rtl">
      {/* --- نوار ابزار بالا (Toolbar) --- */}
      <div className="flex flex-wrap justify-between items-center gap-2 bg-gray-50 p-3 rounded-lg border border-gray-200">
        <div className="flex gap-2">
          {allowAdd && (
            <button
              onClick={handleAddNewRow}
              className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded shadow transition"
            >
              + افزودن سطر جدید
            </button>
          )}

          {allowExcelImport && (
            <label className="bg-green-600 hover:bg-green-700 text-white text-sm font-medium px-4 py-2 rounded shadow transition cursor-pointer">
              📥 بارگذاری از اکسل
              <input
                type="file"
                accept=".xlsx, .xls"
                onChange={handleExcelUpload}
                className="hidden"
              />
            </label>
          )}
        </div>

        {/* دکمه ذخیره انبوه کل تراکنش‌ها */}
        {isDirty && (onSaveBatch || onSaveRow) && (
          <button
            onClick={processSaveBatch}
            className="bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-bold px-5 py-2 rounded shadow transition"
          >
            💾 ذخیره کل تغییرات
          </button>
        )}
      </div>

      {/* --- جدول داده‌ها --- */}
      <div className="overflow-x-auto border border-gray-200 rounded-lg shadow bg-white">
        <table className="min-w-full border-collapse text-right text-sm">
          <SmartDataGridHeader
            columns={finalColumns}
            sortColumn={sortColumn}
            sortDirection={sortDirection}
            onSort={toggleSort}
          />
          <tbody>
            {paginatedData.length === 0 ? (
              <tr>
                <td colSpan={finalColumns.length} className="text-center py-10 text-gray-400">
                  {emptyMessage}
                </td>
              </tr>
            ) : (
              paginatedData.map((row, idx) => {
                const key = keyExtractor(row, idx);
                const isEditing = editingKeys.has(key);
                const hasError = rowErrors.has(key);

                // مدیریت داینامیک رنگ پس‌زمینه سطر بر اساس اعتبارسنجی
                let rowClassName = "border-b hover:bg-gray-50 transition-colors";
                if (hasError) {
                  rowClassName = "border-b bg-red-50 hover:bg-red-100 dark:bg-red-950/20";
                } else if (addedKeys.has(key)) {
                  rowClassName = "border-b bg-green-50/60 hover:bg-green-100/60";
                } else if (editingKeys.has(key)) {
                  rowClassName = "border-b bg-amber-50/40";
                }

                return (
                  <tr key={key} className={rowClassName}>
                    {columns.map(col => (
                      <td key={String(col.id)} className="p-3 border-b max-w-xs overflow-hidden text-ellipsis">
                        <SmartDataGridCell
                          row={row}
                          column={col}
                          index={idx}
                          isEditing={isEditing}
                          onValueChange={(field, val) => handleCellChange(key, field, val)}
                        />
                      </td>
                    ))}

                    {/* رندر ستون عملیات */}
                    {hasActions && (
                      <td className="p-3 border-b whitespace-nowrap text-left">
                        <div className="flex gap-3 justify-end">
                          {isEditing ? (
                            <>
                              <button
                                onClick={() => processSaveRow(row, idx)}
                                className="text-emerald-600 hover:text-emerald-800 font-medium text-xs"
                                title="ذخیره سطر جاری"
                              >
                                ✔ ذخیره
                              </button>
                              <button
                                onClick={() => cancelEditing(key, idx)}
                                className="text-gray-500 hover:text-gray-700 font-medium text-xs"
                                title="انصراف"
                              >
                                ✖ انصراف
                              </button>
                            </>
                          ) : (
                            <>
                              {allowEdit && (
                                <button
                                  onClick={() => startEditing(key)}
                                  className="text-blue-600 hover:text-blue-800 text-xs"
                                >
                                  ویرایش
                                </button>
                              )}
                              {allowDelete && (
                                <button
                                  onClick={() => handleDeleteRow(row, idx)}
                                  className="text-red-600 hover:text-red-800 text-xs"
                                >
                                  حذف
                                </button>
                              )}
                            </>
                          )}
                        </div>
                      </td>
                    )}
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* --- کامپوننت صفحه‌بندی (Pagination) --- */}
      {totalPages > 1 && (
        <div className="flex justify-center items-center gap-4 mt-2">
          <button
            onClick={() => setPage(currentPage - 1)}
            disabled={currentPage <= 1}
            className="px-4 py-1.5 border rounded-md text-sm bg-white hover:bg-gray-50 disabled:opacity-40 transition"
          >
            قبلی
          </button>
          <span className="text-sm text-gray-600">
            صفحه <span className="font-semibold text-gray-900">{currentPage}</span> از <span className="font-semibold text-gray-900">{totalPages}</span>
          </span>
          <button
            onClick={() => setPage(currentPage + 1)}
            disabled={currentPage >= totalPages}
            className="px-4 py-1.5 border rounded-md text-sm bg-white hover:bg-gray-50 disabled:opacity-40 transition"
          >
            بعدی
          </button>
        </div>
      )}
    </div>
  );
}

export default SmartDataGrid;