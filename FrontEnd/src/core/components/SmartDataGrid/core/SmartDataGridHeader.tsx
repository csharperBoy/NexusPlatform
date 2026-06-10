import React from 'react';
import { ColumnDef } from '../SmartDataGrid.types';
import { useGridContext } from './SmartDataGridContext';
import { SortIndicatorUI } from '../plugins/sorting/SortIndicatorUI';

interface HeaderProps<T> { columns: ColumnDef<T>[]; }

function SmartDataGridHeader<T>({ columns }: HeaderProps<T>) {
  const instance = useGridContext<T>();
  const toggleSort = instance.actions.toggleSort;

  return (
    <thead className="bg-gray-100 border-b-2 border-gray-200">
      <tr>
        {columns.map(col => (
          <th
            key={String(col.id)}
            className={`p-3 text-right text-sm font-semibold text-gray-700 ${col.sortable && typeof toggleSort === 'function' ? 'cursor-pointer select-none hover:bg-gray-200' : ''}`}        
            style={{ width: col.width }}
            onClick={() => col.sortable && toggleSort?.(String(col.id))}
          >
            <div className="flex items-center justify-start gap-1">
              <span>{col.header}</span>
              {col.sortable && <SortIndicatorUI columnId={String(col.id)} />}
            </div>
          </th>
        ))}
      </tr>
    </thead>
  );
}
export default React.memo(SmartDataGridHeader) as <T>(props: HeaderProps<T>) => React.JSX.Element;