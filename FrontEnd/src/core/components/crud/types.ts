//src/core/components/crud/types.ts
import React from "react";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { ApiOptions } from "@/core/api/apiOptions";

export interface BaseEntity {
  id: string | number;
}

export type ColumnType =
  | "text"
  | "number"
  | "select"
  | "multi-select"
  | "taginput"
  | "date"
  | "boolean";

export interface GenericColumnDef<T> {
  key: keyof T | string;
  label: string;
  type?: ColumnType;
  selectionKey?: string;
  editable?: boolean;
  required?: boolean;
  dir?: "ltr" | "rtl";
  className?: string;
  render?: (value: any, item: T) => React.ReactNode;
  getFilterValue?: (entity: T) => string;
}

export interface GenericCrudApi<T extends BaseEntity, TCreateCmd, TUpdateCmd> {
  getList: (options?: ApiOptions) => Promise<T[]>;
  getSelectionList?: (options?: ApiOptions) => Promise<SelectionListDto[]>;
  create: (cmd: TCreateCmd, options?: ApiOptions) => Promise<any>;
  batchUpdate: (cmds: TUpdateCmd[], options?: ApiOptions) => Promise<any>;
  delete: (id: T["id"], options?: ApiOptions) => Promise<any>;
}

export interface TableFeatures {
  enableExcelImport?: boolean;
  enableExcelExport?: boolean;
  enableSearch?: boolean;
  enableColumnFilter?: boolean;
  enableDelete?: boolean;
}

export interface PageFeatures {
  enableAdd?: boolean;
}

export interface UseGenericCrudOptions<T extends BaseEntity, TCreateCmd, TUpdateCmd> {
  api: GenericCrudApi<T, TCreateCmd, TUpdateCmd>;
  apiOptions?: ApiOptions; // 👈 تنظیمات سراسری استراتژی درخواست‌ها (مثل queueOffline) برای این CRUD
  columns: GenericColumnDef<T>[];
  selectionApis?: Record<string, (options?: ApiOptions) => Promise<SelectionListDto[]>>;
  mapToUpdateCommand?: (entity: T) => TUpdateCmd;
  mapToCreateCommand?: (formData: Record<string, any>) => TCreateCmd;
  transformApiData?: (data: T[]) => T[];
  excelMatchKey?: keyof T;
  tableFeatures?: TableFeatures;
  pageFeatures?: PageFeatures;
}

export interface DeleteTarget<T> {
  item: T;
  isModified?: boolean;
}