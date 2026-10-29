import React from "react";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { ApiOptions } from "@/core/api/apiOptions";
import { BaseEntity } from "@/core/models/BaseEntity";


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

  /** گزینه‌های ثابت (enumها) که از سرور نمیان */
  staticOptions?: SelectionListDto[];

  /**
   * نوع داده‌ای که این ستون باید در draft ذخیره کنه.
   * - "string"  (پیش‌فرض) → بدون تغییر
   * - "number"  → تبدیل به Number؛ خالی/null → null
   * - "boolean" → تبدیل به Boolean
   */
  valueType?: "string" | "number" | "boolean";

  editable?: boolean;

  /**
   * Override the default "includes in Add Modal" behavior.
   * - undefined: follow `editable !== false`
   * - true: always include in Add Modal
   * - false: never include in Add Modal
   */
  editableInAdd?: boolean;

  required?: boolean;
  dir?: "ltr" | "rtl";
  className?: string;
  render?: (value: any, item: T) => React.ReactNode;
  getFilterValue?: (entity: T) => string;

  /** حداقل عرض ستون به پیکسل (اختیاری). */
  minWidth?: number;

  /** اگر true، محتوای سلول در یک خط می‌ماند (بدون wrap). */
  noWrap?: boolean;

  /** چسبیدن ستون به لبه‌ی visual (left/right) در صورت اسکرول افقی. */
  sticky?: "left" | "right";

  /** عرض ستون (px). الزامی اگر sticky set باشد. */
  stickyWidth?: number;
}

export interface GenericCrudApi<T extends BaseEntity, TCreateCmd, TUpdateCmd> {
  getList: (options?: ApiOptions) => Promise<T[]>;
  getSelectionList?: (options?: ApiOptions) => Promise<SelectionListDto[]>;
  create: (cmd: TCreateCmd, options?: ApiOptions) => Promise<any>;
  batchUpdate: (cmds: TUpdateCmd[], options?: ApiOptions) => Promise<any>;
  delete: (id: T["id"], options?: ApiOptions) => Promise<any>;
}

/**
 * کلاس‌های CSS قابل تنظیم برای عناصر ساختاری جدول.
 * همه اختیاری هستند — اگر تنظیم نشوند، مقادیر پیش‌فرض (که نمایش فعلی را حفظ می‌کنند) استفاده می‌شوند.
 */
export interface TableClassNames {
  /** wrapper که overflow-x-auto دارد */
  wrapper?: string;
  /** کلاس‌های پایه‌ی <table> (بدون w-full/min-w-max که خودکار اضافه می‌شوند) */
  table?: string;

  /** کلاس‌های <thead> */
  thead?: string;
  /** کلاس‌های <th> عادی */
  th?: string;
  /** کلاس‌های <th> مخصوص ستون Action */
  thAction?: string;
  /** کلاس‌های <input> فیلتر ستون */
  filterInput?: string;

  /** کلاس‌های <tbody> */
  tbody?: string;
  /** کلاس‌های <tr> — باید شامل bg + hover باشد اگر sticky دارید */
  tr?: string;
  /** کلاس‌های <td> عادی */
  td?: string;
  /** کلاس‌های <td> مخصوص ستون Action */
  tdAction?: string;
  /** کلاس‌های <td> برای حالت loading/empty (colSpan) */
  tdEmpty?: string;

  /**
   * کلاس‌های اضافه برای <th> و <td> sticky.
   * پیش‌فرض: "bg-inherit" — یعنی رنگ از <tr> یا <thead> ارث می‌برد.
   * اگر <tr> bg صریح ندارد، اینجا override کنید (مثلاً "bg-white dark:bg-gray-900").
   */
  stickyTh?: string;
  stickyTd?: string;

  /** کلاس‌های دکمه‌ی حذف */
  deleteButton?: string;
}

export interface TableFeatures {
  enableExcelImport?: boolean;
  enableExcelExport?: boolean;
  enableSearch?: boolean;
  enableColumnFilter?: boolean;
  enableDelete?: boolean;

  horizontalScroll?: boolean;

  /** چسبیدن ستون Action (دکمه‌ی حذف) به لبه. */
  stickyAction?: "left" | "right";
  /** عرض ستون Action (px). پیش‌فرض 100. */
  stickyActionWidth?: number;

  /** کلاس‌های دلخواه برای عناصر ساختاری جدول */
  classNames?: TableClassNames;
}

export interface PageFeatures {
  enableAdd?: boolean;
}

export interface UseGenericCrudOptions<T extends BaseEntity, TCreateCmd, TUpdateCmd> {
  api: GenericCrudApi<T, TCreateCmd, TUpdateCmd>;
  apiOptions?: ApiOptions;
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