
// src/core/components/Table/Table.tsx

import React from 'react';
import { useTable } from './useTable';
import { TableProps } from './Table.type';

function Table<T>({
  rows,
  renderRow,
  onRowClick,
  getRowId,
  getRowLabel,
  selectable = false,
  selected,
  defaultSelected,
  onSelectionChange,
  className = '',
  dir = 'rtl',
}: TableProps<T>) {
  const {
    handleRowClick,
    getRowId: _getRowId,
    getRowLabel: _getRowLabel,
    selectedRows,
    toggleSelect,
    isSelected,
  } = useTable({
    rows,
    onRowClick,
    getRowId,
    getRowLabel,
    selectable,
    selected,
    defaultSelected,
    onSelectionChange,
  });

  const renderRowRecursive = (row: T, level: number = 0): React.ReactNode => {
    const rowId = _getRowId(row);
    const label = _getRowLabel(row);
    const selected = isSelected(rowId);

    const handleCheckboxChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      e.stopPropagation();
      toggleSelect(rowId, row);
    };

    // استایل padding بر اساس جهت صفحه
    const paddingStyle = dir === 'rtl' 
      ? { paddingRight: `${level * 1.5}rem` } 
      : { paddingLeft: `${level * 1.5}rem` };

    return (
      <div key={rowId} className="table-row">
        <div
          className="table-row-content flex items-center py-1 px-2 hover:bg-gray-100 cursor-pointer"
          style={paddingStyle}
          onClick={() => handleRowClick(row)}
        >
          {/* چک‌باکس */}
          {selectable && (
            <input
              type="checkbox"
              checked={selected}
              onChange={handleCheckboxChange}
              onClick={(e) => e.stopPropagation()}
              className={dir === 'rtl' ? 'ml-2' : 'mr-2'}
            />
          )}

          <div>{row.label} 2</div>
          {/* محتوای گره */}
          {renderRow ? (
            renderRow(row, level,  selected)
          ) : (
            <span className="table-label">{label}</span>
          )}
        </div>        
      </div>
    );
  };

  return (
    <div className={`table ${className}`} dir={dir}>
      {rows.map(row => renderRowRecursive(row))}
    </div>
  );
}

export default Table;