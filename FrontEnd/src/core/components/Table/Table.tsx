// src/core/components/Table/Table.tsx
// src/core/components/Table/Table.tsx
import React from 'react';
import { TableProps } from './Table.types';
import TableHeader from './TableHeader';
import TableCell from './TableCell';
import { useTable } from './useTable';

function Table<T>({
  data,
  columns,
  keyExtractor = (row, idx) => idx,
  onEdit,
  onDelete,
  selectable = false,
  selectedIds,
  onSelectionChange,
  pageSize,
  currentPage: externalCurrentPage,
  onPageChange,
  sortColumn: externalSortColumn,
  sortDirection: externalSortDirection,
  onSortChange,
  className = '',
  emptyMessage = 'داده‌ای وجود ندارد',
}: TableProps<T>) {
  const {
    paginatedData,
    currentPage,
    totalPages,
    setPage,
    sortColumn,
    sortDirection,
    toggleSort,
    selectedIds: internalSelectedIds,
    toggleSelect,
    toggleSelectAll,
    hasPagination,
  } = useTable({
    data,
    columns,
    keyExtractor,
    selectable,
    selectedIds,
    onSelectionChange,
    pageSize,
    currentPage: externalCurrentPage,
    onPageChange,
    sortColumn: externalSortColumn,
    sortDirection: externalSortDirection,
    onSortChange,
  });

  const allSelected = paginatedData.length > 0 && paginatedData.every((row, idx) => internalSelectedIds.has(keyExtractor(row, idx)));
  const indeterminate = !allSelected && paginatedData.some((row, idx) => internalSelectedIds.has(keyExtractor(row, idx)));

  // ستون عملیات (اگر onEdit یا onDelete وجود داشته باشد)
  const hasActions = !!(onEdit || onDelete);
  const actionColumn = hasActions
    ? {
        id: '__actions',
        header: 'عملیات',
        type: 'action' as const,
        width: '100px',
      }
    : null;
  const allColumns = actionColumn ? [...columns, actionColumn] : columns;

  return (
    <div className={`overflow-x-auto ${className}`}>
      <table className="min-w-full border-collapse">
        <TableHeader
          columns={allColumns}
          sortColumn={sortColumn}
          sortDirection={sortDirection}
          onSort={toggleSort}
          selectable={selectable}
          onSelectAll={toggleSelectAll}
          allSelected={allSelected}
          indeterminate={indeterminate}
        />
        <tbody>
          {paginatedData.length === 0 ? (
            <tr>
              <td colSpan={allColumns.length + (selectable ? 1 : 0)} className="text-center py-8 text-gray-500">
                {emptyMessage}
              </td>
            </tr>
          ) : (
            paginatedData.map((row, idx) => {
              const rowId = keyExtractor(row, idx);
              const isSelected = internalSelectedIds.has(rowId);
              return (
                <tr key={rowId} className="border-b hover:bg-gray-50">
                  {selectable && (
                    <td className="w-8 p-2 text-center">
                      <input
                        type="checkbox"
                        checked={isSelected}
                        onChange={() => toggleSelect(row, idx)}
                        className="accent-blue-500"
                      />
                    </td>
                  )}
                  {columns.map(col => (
                    <td key={String(col.id)} className="p-2 border-b">
                      <TableCell row={row} column={col as any} index={idx} />
                    </td>
                  ))}
                  {hasActions && (
                    <td className="p-2 border-b whitespace-nowrap">
                      <div className="flex gap-2">
                        
                        {onEdit && (
                          <button onClick={() => onEdit(row, idx)}   className="text-blue-600 hover:text-blue-800 text-sm">
                            ویرایش
                          </button>
                        )}
                        {onDelete && (
                          <button onClick={() => onDelete(row, idx)}  className="text-red-600 hover:text-red-800 text-sm">
                            حذف
                          </button>
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

      {hasPagination && totalPages > 1 && (
        <div className="flex justify-center gap-2 mt-4">
          <button
            onClick={() => setPage(currentPage - 1)}
            disabled={currentPage <= 1}
            className="px-3 py-1 border rounded disabled:opacity-50"
          >
            قبلی
          </button>
          <span className="px-3 py-1">
            صفحه {currentPage} از {totalPages}
          </span>
          <button
            onClick={() => setPage(currentPage + 1)}
            disabled={currentPage >= totalPages}
            className="px-3 py-1 border rounded disabled:opacity-50"
          >
            بعدی
          </button>
        </div>
      )}
    </div>
  );
}

export default Table;