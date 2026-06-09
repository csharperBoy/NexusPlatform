// src/core/components/Table/TableCell.tsx
import React, { useState } from 'react';
import { ColumnDef } from './Table.types';
import { SelectionListDto } from '@/core/models/SelectionListDto';

interface TableCellProps<T> {
  row: T;
  column: ColumnDef<T>;
  index: number;
}

function TableCell<T>({ row, column, index }: TableCellProps<T>) {
  const rawValue = column.accessor ? column.accessor(row) : (row as any)[column.id];
  const type = column.type || 'text';

  // select – نمایش dropdown تعاملی
  if (type === 'select' && column.options) {
    const options = column.options as SelectionListDto[];
    return (
      <select
        value={rawValue != null ? String(rawValue) : ''}
        onChange={(e) => {
          let newValue: any = e.target.value;
          // تبدیل به عدد اگر لازم است (فرض ساده)
          if (!isNaN(Number(newValue)) && newValue !== '') newValue = Number(newValue);
          if (column.onCellChange) column.onCellChange(row, newValue, index);
        }}
        className="select select-bordered w-full"
      >
        <option value="">انتخاب کنید...</option>
        {options.map(opt => (
          <option key={opt.value} value={String(opt.value)}>
            {opt.label}
          </option>
        ))}
      </select>
    );
  }

  // date
  if (type === 'date') {
    if (!rawValue) return <span>-</span>;
    const date = new Date(rawValue);
    return <span>{date.toLocaleDateString('fa-IR')}</span>;
  }

  // checkbox
  if (type === 'checkbox') {
    return <input type="checkbox" checked={!!rawValue} readOnly disabled className="accent-blue-500" />;
  }

  // custom
  if (type === 'custom' && column.render) {
    return <>{column.render(row, index)}</>;
  }

  // text (پیش‌فرض)
  return <span>{rawValue != null ? String(rawValue) : '-'}</span>;
}

export default React.memo(TableCell);