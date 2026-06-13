// src/core/components/SmartDataGrid/SmartDataGrid.types.ts
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { ReactNode } from "react";

export type ColumnType = 'text' | 'number' | 'select' | 'date' | 'checkbox' | 'custom';

export interface ColumnDef<T> {
  id: keyof T | string;
  header: string;
  type?: ColumnType;
  width?: string | number;
  sortable?: boolean;
  options?: SelectionListDto[];
  render?: (row: T, index: number) => ReactNode;
  accessor?: (row: T) => any;
  editable?: boolean;
}

// 📦 ۱. آبجکت تنظیمات ویرایش و اکشن‌ها
export interface ActionConfig<T> {
  allowAdd?: boolean;
  allowEdit?: boolean;
  allowDelete?: boolean;
  onSaveRow?: (row: T, actionType: 'add' | 'edit' | 'delete', index: number) => Promise<void> | void;
  onSaveBatch?: (changes: BatchChanges<T>) => Promise<void> | void;
  validateRow?: (row: T) => string[] | null;
  emptyRowFactory?: () => T;
}

// 📦 ۲. آبجکت تنظیمات اکسل
export interface ExcelConfig<T> {
  allowImport?: boolean;
  allowExport?: boolean;
}

// 📦 ۳. آبجکت تنظیمات ساختار درختی (دقیقاً مثل نمونه‌ای که فرستادی)
export interface TreeConfig<T> {
  enabled: boolean;
  parentKey: keyof T | string;
  indentSize?: number; // اضافه کردن آپشن دلخواه برای فاصله تورفتگی
}

export interface BatchChanges<T> {
  added: T[];
  modified: T[];
  deletedIds: (string | number)[];
}

// 🚀 پراپ‌های نهایی و فوق‌العاده تمیزِ گرید
export interface SmartDataGridProps<T> {
  data: T[];
  columns: ColumnDef<T>[];
  keyExtractor: (row: T, index?: number) => string | number;
  pageSize?: number;
  className?: string;
  emptyMessage?: string;

  // کانفیگ‌های مجتمع پلاگین‌ها
  actionConfig?: ActionConfig<T>;
  excelConfig?: ExcelConfig<T>;
  treeConfig?: TreeConfig<T>;
}