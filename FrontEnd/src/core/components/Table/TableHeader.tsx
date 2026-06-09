// src/core/components/Table/TableHeader.tsx
import React from 'react';
import { ColumnDef } from './Table.types';

interface TableHeaderProps<T> {
  columns: ColumnDef<T>[];
  sortColumn?: string;
  sortDirection?: 'asc' | 'desc';
  onSort: (columnId: string) => void;
  selectable: boolean;
  onSelectAll: () => void;
  allSelected: boolean;
  indeterminate: boolean;
}

function TableHeader<T>({
  columns,
  sortColumn,
  sortDirection,
  onSort,
  selectable,
  onSelectAll,
  allSelected,
  indeterminate,
}: TableHeaderProps<T>) {
  return (
    <thead className="bg-gray-100">
      <tr>
        {selectable && (
          <th className="w-8 p-2 border-b text-center">
              <input
                type="checkbox"
                checked={allSelected}
                ref={el => { if (el) { el.indeterminate = indeterminate; } }}
                onChange={() => onSelectAll()}
                className="accent-blue-500"
              />
          </th>
        )}
        {columns.map(col => (
          <th
            key={String(col.id)}
            className={`p-2 border-b text-right ${col.sortable ? 'cursor-pointer select-none hover:bg-gray-200' : ''}`}
            style={{ width: col.width }}
            onClick={() => col.sortable && onSort(String(col.id))}
          >
            <div className="flex items-center justify-between gap-1">
              {col.header}
              {col.sortable && sortColumn === col.id && (
                <span className="text-xs">{sortDirection === 'asc' ? '▲' : '▼'}</span>
              )}
            </div>
          </th>
        ))}
        {/* ستون عملیات اگر در columns نباشد، در جای دیگر اضافه می‌شود */}
      </tr>
    </thead>
  );
}

export default TableHeader;