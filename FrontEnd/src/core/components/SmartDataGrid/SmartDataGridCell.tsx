// src/core/components/SmartDataGrid/SmartDataGridCell.tsx
import React from 'react';
import { ColumnDef } from './SmartDataGrid.types';

interface CellProps<T> {
  row: T;
  column: ColumnDef<T>;
  index: number;
  isEditing: boolean;
  onValueChange: (field: string, value: any) => void;
}

function SmartDataGridCell<T>({ row, column, index, isEditing, onValueChange }: CellProps<T>) {
  const rawValue = column.accessor ? column.accessor(row) : (row as any)[column.id];
  const type = column.type || 'text';

  if (!isEditing || column.editable === false || type === 'custom') {
    if (type === 'custom' && column.render) return <>{column.render(row, index)}</>;
    if (type === 'checkbox') return <input type="checkbox" checked={!!rawValue} disabled className="accent-blue-500" />;
    if (type === 'date' && rawValue) return <span>{new Date(rawValue).toLocaleDateString('fa-IR')}</span>;
    if (type === 'select' && column.options) {
      const option = column.options.find(o => String(o.value) === String(rawValue));
      return <span>{option ? (option.display || option.label) : String(rawValue || '-')}</span>;
    }
    return <span>{rawValue != null ? String(rawValue) : '-'}</span>;
  }

  switch (type) {
    case 'select':
      return (
        <select
          value={rawValue != null ? String(rawValue) : ''}
          onChange={(e) => {
            let val: any = e.target.value;
            if (val !== '' && !isNaN(Number(val))) val = Number(val);
            onValueChange(String(column.id), val);
          }}
          className="w-full p-1 border rounded bg-white text-sm focus:outline-blue-500"
        >
          <option value="">انتخاب کنید...</option>
          {column.options?.map(opt => (
            <option key={opt.value} value={String(opt.value)}>{opt.display || opt.label}</option>
          ))}
        </select>
      );

    case 'checkbox':
      return (
        <input
          type="checkbox"
          checked={!!rawValue}
          onChange={(e) => onValueChange(String(column.id), e.target.checked)}
          className="accent-blue-500 h-4 w-4"
        />
      );

    case 'number':
      return (
        <input
          type="number"
          value={rawValue ?? ''}
          onChange={(e) => onValueChange(String(column.id), e.target.value === '' ? null : Number(e.target.value))}
          className="w-full p-1 border rounded text-sm focus:outline-blue-500"
        />
      );

    case 'date':
      return (
        <input
          type="date"
          value={rawValue ? new Date(rawValue).toISOString().split('T')[0] : ''}
          onChange={(e) => onValueChange(String(column.id), e.target.value)}
          className="w-full p-1 border rounded text-sm focus:outline-blue-500"
        />
      );

    default:
      return (
        <input
          type="text"
          value={rawValue ?? ''}
          onChange={(e) => onValueChange(String(column.id), e.target.value)}
          className="w-full p-1 border rounded text-sm focus:outline-blue-500"
        />
      );
  }
}

export default React.memo(SmartDataGridCell) as <T>(props: CellProps<T>) => React.JSX.Element;