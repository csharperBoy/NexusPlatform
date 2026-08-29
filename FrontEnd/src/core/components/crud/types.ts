//src/core/components/crud/types.ts
import React from "react";
import { SelectionListDto } from "@/core/models/SelectionListDto";

export interface BaseEntity {
  id: string | number;
}

export type ColumnType = "text" | "number" | "select" | "multi-select" | "taginput" | "date" | "boolean";

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
}
// بخشی از types.ts
export interface GenericCrudApi<T extends BaseEntity, TCreateCmd, TUpdateCmd> {
  getList: () => Promise<T[]>;
  getSelectionList?: () => Promise<SelectionListDto[]>;
  create: (cmd: TCreateCmd) => Promise<any>;
  batchUpdate: (cmds: TUpdateCmd[]) => Promise<any>;
  delete: (id: T["id"]) => Promise<any>; // <--- تغییر از (string | number) به T["id"]
}

export interface TableFeatures {
  enableExcelImport?: boolean;
  enableExcelExport?: boolean;
  enableSearch?: boolean;
  enableColumnFilter?: boolean;
}

export interface UseGenericCrudOptions<T extends BaseEntity, TCreateCmd, TUpdateCmd> {
  api: GenericCrudApi<T, TCreateCmd, TUpdateCmd>;
  columns: GenericColumnDef<T>[];
  selectionApis?: Record<string, () => Promise<SelectionListDto[]>>;
  mapToUpdateCommand?: (entity: T) => TUpdateCmd;
  mapToCreateCommand?: (formData: Record<string, any>) => TCreateCmd;
  transformApiData?: (data: T[]) => T[];
  excelMatchKey?: keyof T;
  features?: TableFeatures;
}

export interface DeleteTarget<T> {
  item: T;
  isModified?: boolean;
}