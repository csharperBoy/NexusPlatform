import React from "react";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { TreeSelectionListDto } from "@/core/models/TreeSelectionListDto";
import { ApiOptions } from "@/core/api/apiOptions";
import {  GenericColumnDef } from "../crud/types";
import { HierarchicalEntity } from "@/core/models/HierarchicalEntity";



// ─── قابلیت‌های جدول درختی ───
export interface TreeTableFeatures {
  enableExcelImport?: boolean;
  enableExcelExport?: boolean;
  enableSearch?: boolean;
  enableColumnFilter?: boolean;
  enableDelete?: boolean;
  enableDragDrop?: boolean;          // پیش‌فرض: true
  enableMultiSelect?: boolean;       // پیش‌فرض: true
  enableExpandCollapseAll?: boolean; // پیش‌فرض: true
  enableStatusColumn?: boolean;      // پیش‌فرض: true
  enableInlineAddChild?: boolean;    // پیش‌فرض: false
}

export interface TreePageFeatures {
  enableAdd?: boolean;
  enableAddAsRoot?: boolean;         // پیش‌فرض: false
  enableResetAll?: boolean;
}

// ─── ستون مخصوص درخت ───
export interface TreeColumnDef<T> extends GenericColumnDef<T> {
  /** عناوین احتمالی این ستون در فایل اکسل (اگر خالی باشد از label استفاده می‌شود) */
  excelHeaders?: string[];
  /** برای ستون‌های select/multi-select در ایمپورت اکسل */
  excelMatchByDisplay?: boolean;     // پیش‌فرض: true
  /** جداکننده برای multi-select/taginput در اکسل */
  excelSeparator?: RegExp;           // پیش‌فرض: /[،,;؛]/
}

// ─── API مخصوص ساختار درختی ───
export interface GenericTreeCrudApi<
  T extends HierarchicalEntity,
  TCreateCmd,
  TUpdateCmd
> {
  getList: (options?: ApiOptions) => Promise<T[]>;
  getSelectionList?: (options?: ApiOptions) => Promise<TreeSelectionListDto[]>;
  create: (cmd: TCreateCmd, options?: ApiOptions) => Promise<any>;
  batchUpdate: (cmds: TUpdateCmd[], options?: ApiOptions) => Promise<any>;
  delete: (id: T["id"], options?: ApiOptions) => Promise<any>;
}

// ─── تنظیمات هوک ───
export interface UseGenericTreeCrudOptions<
  T extends HierarchicalEntity,
  TCreateCmd,
  TUpdateCmd
> {
  api: GenericTreeCrudApi<T, TCreateCmd, TUpdateCmd>;
  apiOptions?: ApiOptions;
  columns: TreeColumnDef<T>[];

  selectionApis?: Record<
    string,
    (options?: ApiOptions) => Promise<(SelectionListDto | TreeSelectionListDto)[]>
  >;

  getCreateDefaults?: () => Record<string, any>;

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
  isNew: boolean;
  matchesSearch: boolean;
}

export interface TreeDeleteTarget<T> {
  item: T;
  title: string;
  isModified?: boolean;
}