// src/core/components/SmartDataGrid/SmartDataGridHeader.tsx
import React from 'react';
import { ColumnDef } from './SmartDataGrid.types';

interface HeaderProps<T> {
  columns: ColumnDef<T>[];
  sortColumn?: string;
  sortDirection?: 'asc' | 'desc';
  onSort: (columnId: string) => void;
}

function SmartDataGridHeader<T>({ columns, sortColumn, sortDirection, onSort }: HeaderProps<T>) {
  return (
    <thead className="bg-gray-100 border-b-2 border-gray-200">
      <tr>
        {columns.map(col => (
          <th
            key={String(col.id)}
            className={`p-3 text-right text-sm font-semibold text-gray-700 ${col.sortable ? 'cursor-pointer select-none hover:bg-gray-200' : ''}`}
            style={{ width: col.width }}
            onClick={() => col.sortable && onSort(String(col.id))}
          >
            <div className="flex items-center justify-start gap-1">
              <span>{col.header}</span>
              {col.sortable && sortColumn === col.id && (
                <span className="text-xs text-blue-600">{sortDirection === 'asc' ? '▲' : '▼'}</span>
              )}
            </div>
          </th>
        ))}
      </tr>
    </thead>
  );
}

export default React.memo(SmartDataGridHeader) as <T>(props: HeaderProps<T>) => React.JSX.Element;