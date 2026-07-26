/**
 * DataTreeGrid Column Contracts
 *
 * قرارداد ستون‌ها مستقل از Grid Engine است.
 *
 * این فایل نباید هیچ وابستگی به:
 * - TanStack Table
 * - React
 * - UI Library
 *
 * داشته باشد.
 */

import type {
  TreeNodeBase
} from "./tree";

import type {
  CellFormatter
} from "./formatter";

import type {
  DataTreeGridCellContext
}
from "./render";


// =====================================================
// Editor Types
// =====================================================


export type ColumnEditorType =

  | "text"

  | "number"

  | "boolean"

  | "date"

  | "select"

  | "custom";




// =====================================================
// Select Options
// =====================================================


export interface ColumnOption {


  label:string;


  value:
    string | number;


}




// =====================================================
// Editor Configuration
// =====================================================


export interface ColumnEditorConfig {


  type:
    ColumnEditorType;



  /**
   * برای Select / ComboBox
   */
  options?:
    ColumnOption[];



  /**
   * Placeholder برای Inputها
   */
  placeholder?:
    string;


}




// =====================================================
// Main Column Contract
// =====================================================


export interface DataTreeGridColumn<
  T extends TreeNodeBase
>{


  /**
   * شناسه یکتا ستون
   */
  id:string;



  /**
   * عنوان ستون
   */
  header:string;



  /**
   * فیلدی از Entity
   *
   * مثال:
   * jobTitleName
   */
  accessorKey?:
    keyof T;



  /**
   * آیا این ستون ساختار Tree را نمایش می‌دهد؟
   *
   * فقط یک ستون باید true باشد.
   */
  treeColumn?:
    boolean;



  /**
   * قابلیت ویرایش
   */
  editable?:
    boolean;



  /**
   * تنظیمات Editor
   */
  editor?:
    ColumnEditorConfig;



  /**
   * فقط خواندنی
   */
  readonly?:
    boolean;



  /**
   * اندازه پیشنهادی
   *
   * Renderer تصمیم نهایی را می‌گیرد.
   */
  size?:
    number;



  /**
   * تبدیل مقدار برای نمایش
   */
  formatter?:
    CellFormatter<T>;

  /**
   * Renderer اختصاصی سلول
   *
   * برای UI Layer
   */
  cell?(
    context:
      DataTreeGridCellContext<T>
  ):
    unknown;

}