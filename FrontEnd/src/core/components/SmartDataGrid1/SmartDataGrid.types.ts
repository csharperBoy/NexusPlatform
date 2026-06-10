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
  options?: SelectionListDto[]; // منطبق با DTO اختصاصی پروژه شما
  render?: (row: T, index: number) => ReactNode;
  accessor?: (row: T) => any;
  editable?: boolean;
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
  allowAdd?: boolean;
  allowEdit?: boolean;
  allowDelete?: boolean;
  allowExcelImport?: boolean;
  allowExcelExport?: boolean;
  onSaveRow?: (row: T, actionType: 'add' | 'edit' | 'delete', index: number) => Promise<void> | void;
  onSaveBatch?: (changes: BatchChanges<T>) => Promise<void> | void;
  validateRow?: (row: T) => string[] | null;
  emptyRowFactory?: () => T;
  pageSize?: number;
  className?: string;
  emptyMessage?: string;
}