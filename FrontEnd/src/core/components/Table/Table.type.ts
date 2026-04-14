// src/core/components/Table/Table.types.ts

export interface TableColumnsBase {
  id: string;
  label: string;
}

export interface TableProps<T = TableColumnsBase> extends UseTableProps<T> {
  renderRow?: (row: T, level: number,  isSelected: boolean) => React.ReactNode;
  className?: string;
  dir?: 'ltr' | 'rtl';
}

export interface UseTableProps<T> {
  rows: T[];
  onRowClick?: (row: T) => void;
  getRowId?: (row: T) => string;
  getRowLabel?: (row: T) => string;
  // قابلیت انتخاب با چک‌باکس
  selectable?: boolean;
  selected?: string[];            // برای کنترل خارجی
  defaultSelected?: string[];     // برای کنترل داخلی
  onSelectionChange?: (selectedIds: string[], selectedRows: T[]) => void;
}

export interface UseTableReturn<T> {
  rows: T[];
  
  handleRowClick: (row: T) => void;
  getRowId: (row: T) => string;
  getRowLabel: (row: T) => string;
  // قابلیت انتخاب
  selectedRows: Set<string>;
  toggleSelect: (rowId: string, row: T) => void;
  isSelected: (rowId: string) => boolean;
  selectAll: () => void;
  clearSelection: () => void;
}