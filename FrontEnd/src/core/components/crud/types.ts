//src/core/components/crud/types.ts
import React from "react";
import { SelectionListDto } from "@/core/models/SelectionListDto"; // مسیر ایمپورت بر اساس ساختار شما

// مدل پایه برای تمام موجودیت‌ها
export interface BaseEntity {
  id: string;
  [key: string]: any;
}

// ساختار استاندارد API برای تمام سرویس‌ها
export interface GenericCrudApi<TData, TCreateCmd = any, TUpdateCmd = any> {
  GetSelectionList: () => Promise<SelectionListDto[]>;
  GetList: () => Promise<TData[]>;
  create: (command: TCreateCmd) => Promise<any>;
  update: (id: string, command: TUpdateCmd) => Promise<any>;
  batchUpdate: (commands: TUpdateCmd[]) => Promise<any>;
  delete: (id: string) => Promise<any>;
}

// تنظیمات هر ستون از جدول
export interface GenericColumnDef<T> {
  key: keyof T | string;
  title: string;
  width?: string;
  searchable?: boolean;
  editable?: boolean;
  type?: "text" | "number" | "select" | "multi-select";
  optionsKey?: string; // کلید مربوط به لیست‌های انتخابی (در صورت type === 'select')
  render?: (item: T) => React.ReactNode;
  renderEditCell?: (item: T, onChange: (val: any) => void) => React.ReactNode;
}

// تنظیمات قابلیت‌های فعال در صفحه
export interface TableFeatures<T> {
  enableAdd?: boolean;
  enableDelete?: boolean;
  enableBatchSave?: boolean;
  enableExcelImport?: boolean;
  enableGlobalSearch?: boolean;
  excelMapper?: (excelRow: Record<string, any>, item: T) => Partial<T> | null;
}