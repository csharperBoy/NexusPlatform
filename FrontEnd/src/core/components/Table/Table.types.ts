// src/core/components/Table/Table.types.ts
import { SelectionListDto } from "@/core/models/SelectionListDto";

import { ReactNode } from "react";

export type ColumnType = 'text' | 'select' | 'date' | 'checkbox' | 'action' | 'custom';


export interface ColumnDef<T> {
  id: keyof T | string;
  header: string;
  type?: ColumnType;                // نوع ستون
  width?: string | number;
  sortable?: boolean;
  options?: SelectionListDto[];         // برای type='select'
  render?: (row: T, index: number) => ReactNode; // برای type='custom'
  accessor?: (row: T) => any;       // دسترسی به مقدار (اختیاری)
  onCellChange?: (row: T, newValue: any, index: number) => void;   // جدید
  
}
export interface TableProps<T> {
  data: T[];
  columns: ColumnDef<T>[];
  keyExtractor: (row: T, index: number) => string | number;
  onEdit?: (row: T, index: number) => void;   // اضافه کردن index
  onDelete?: (row: T, index: number) => void; // اضافه کردن index
  
  selectable?: boolean;
  selectedIds?: Set<string | number>;
  onSelectionChange?: (selectedIds: Set<string | number>, selectedRows: T[]) => void;
  pageSize?: number;                // اگر undefined باشد صفحه‌بندی غیرفعال می‌شود
  currentPage?: number;
  onPageChange?: (page: number) => void;
  sortColumn?: string;
  sortDirection?: 'asc' | 'desc';
  onSortChange?: (columnId: string, direction: 'asc' | 'desc') => void;
  className?: string;
  emptyMessage?: string;
}