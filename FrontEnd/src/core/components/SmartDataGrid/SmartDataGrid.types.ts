// src/core/components/SmartDataGrid/SmartDataGrid.types.ts
import { ReactNode } from "react";

export type ColumnType = 'text' | 'number' | 'select' | 'date' | 'checkbox' | 'custom';

export interface SelectionOption {
  value: string | number;
  label: string;
}

export interface ColumnDef<T> {
  id: keyof T | string;
  header: string;
  type?: ColumnType;
  width?: string | number;
  sortable?: boolean;
  options?: SelectionOption[]; // برای نوع select
  render?: (row: T, index: number) => ReactNode; // برای نوع custom
  accessor?: (row: T) => any;
  editable?: boolean; // آیا این ستون قابل ویرایش است؟
}

export interface BatchChanges<T> {
  added: T[];
  modified: T[];
  deletedIds: (string | number)[];
}

export interface SmartDataGridProps<T> {
  data: T[];
  columns: ColumnDef<T>[];
  keyExtractor: (row: T, index?: number) => string | number;
  
  // قابلیت‌ها (Feature Toggles)
  allowAdd?: boolean;
  allowEdit?: boolean;
  allowDelete?: boolean;
  allowExcelImport?: boolean;
  
  // متدهای ذخیره‌سازی
  onSaveRow?: (row: T, actionType: 'add' | 'edit' | 'delete', index: number) => Promise<void> | void;
  onSaveBatch?: (changes: BatchChanges<T>) => Promise<void> | void;
  
  // اعتبارسنجی (خروجی null یعنی بدون خطا، آرایه یعنی رشته خطاها)
  validateRow?: (row: T) => string[] | null;
  
  // سورس پیش‌فرض برای ایجاد سطر خالی جدید
  emptyRowFactory?: () => T;

  // مدیریت ابعاد و استایل
  pageSize?: number;
  className?: string;
  emptyMessage?: string;
}