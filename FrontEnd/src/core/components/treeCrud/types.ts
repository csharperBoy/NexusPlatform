//src/core/components/treeCrud/types.ts
import React from "react";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { TreeSelectionListDto } from "@/core/models/TreeSelectionListDto";
import { ApiOptions } from "@/core/api/apiOptions";
import { BaseEntity, GenericColumnDef } from "../crud/types";

// ─── قابلیت‌های جدول درختی ───
export interface TreeTableFeatures {
  enableExcelImport?: boolean;
  enableExcelExport?: boolean;
  enableSearch?: boolean;
  enableColumnFilter?: boolean;
  enableDelete?: boolean;
  enableDragDrop?: boolean;         // پیش‌فرض: true
  enableMultiSelect?: boolean;      // پیش‌فرض: true
  enableExpandCollapseAll?: boolean;// پیش‌فرض: true
  enableStatusColumn?: boolean;     // ستون "تغییر یافته"  
  enableInlineAddChild?: boolean;
}

export interface TreePageFeatures {
  enableAdd?: boolean;              // پیش‌فرض: false (چون درخت معمولاً افزودن فرزندیه)
  enableAddAsRoot?: boolean;        // افزودن گره ریشه
  enableResetAll?: boolean;         // پیش‌فرض: true
}

// ─── ستون مخصوص درخت (ارث‌بری از GenericColumnDef) ───
export interface TreeColumnDef<T> extends GenericColumnDef<T> {
  /** عناوین احتمالی این ستون در فایل اکسل (اگر خالی باشد از label استفاده می‌شود) */
  excelHeaders?: string[];
  /** برای ستون‌های select/multi-select در ایمپورت اکسل */
  excelMatchByDisplay?: boolean;    // پیش‌فرض: true
  /** جداکننده برای multi-select در اکسل */
  excelSeparator?: string;          // پیش‌فرض: /[،,;؛]/
}

// ─── API مخصوص ساختار درختی ───
export interface GenericTreeCrudApi<T extends BaseEntity, TCreateCmd, TUpdateCmd> {
  getList: (options?: ApiOptions) => Promise<T[]>;
  getSelectionList?: (options?: ApiOptions) => Promise<TreeSelectionListDto[]>;
  create: (cmd: TCreateCmd, options?: ApiOptions) => Promise<any>;
  batchUpdate: (cmds: TUpdateCmd[], options?: ApiOptions) => Promise<any>;
  delete: (id: T["id"], options?: ApiOptions) => Promise<any>;
}

// ─── تنظیمات هوک ───
export interface UseGenericTreeCrudOptions<
  T extends BaseEntity,
  TCreateCmd,
  TUpdateCmd
> {
  api: GenericTreeCrudApi<T, TCreateCmd, TUpdateCmd>;
  apiOptions?: ApiOptions;
  columns: TreeColumnDef<T>[];

  parentIdField?: keyof T | string;   // پیش‌فرض: "parentId"
  idField?: keyof T | string;         // پیش‌فرض: "id"

  selectionApis?: Record<
    string,
    (options?: ApiOptions) => Promise<(SelectionListDto | TreeSelectionListDto)[]>
  >;

  mapToUpdateCommand?: (entity: T) => TUpdateCmd;
  mapToCreateCommand?: (
    formData: Record<string, any>,
    parentId: string | null
  ) => TCreateCmd;
  transformApiData?: (data: T[]) => T[];
  excelMatchKey?: keyof T;

  /** عنوان نمایشی برای مودال حذف - پیش‌فرض: String(id) */
  getDisplayTitle?: (item: T) => string;

  /** تشخیص تغییرات - پیش‌فرض: JSON.stringify */
  isItemModified?: (current: T, initial: T) => boolean;

  /** ترتیب فرزندان - پیش‌فرض: همان ترتیب ورودی */
  sortChildren?: (a: T, b: T) => number;

  tableFeatures?: TreeTableFeatures;
  pageFeatures?: TreePageFeatures;
}

// ─── آیتم flatten شده ───
export interface FlattenedTreeNode<T> {
  node: T;
  depth: number;
  hasChildren: boolean;
  isExpanded: boolean;
  isModified: boolean;
  isSelected: boolean;
  isDragging: boolean;
  isDragOver: boolean;
  matchesSearch: boolean;   // آیا خودش مچ شده (برای bold کردن در سرچ)
}

export interface TreeDeleteTarget<T> {
  item: T;
  title: string;
  isModified?: boolean;
}