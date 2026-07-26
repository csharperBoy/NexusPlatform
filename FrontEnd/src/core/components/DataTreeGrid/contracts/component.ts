/**
 * DataTreeGrid Component Contracts
 *
 * قرارداد ورودی Component اصلی
 *
 * مستقل از:
 * - TanStack
 * - React UI Library
 * - CSS Framework
 */


import type {
  TreeAdapter,
  TreeNodeBase
}
from "./tree";


import type {
  DataTreeGridColumn
}
from "./column";





export interface DataTreeGridProps<
  T extends TreeNodeBase
>{


  /**
   * داده خام
   */
  data:
    readonly T[];



  /**
   * تبدیل Entity به Tree
   */
  adapter:
    TreeAdapter<T>;



  /**
   * ستون‌ها
   */
  columns:
    DataTreeGridColumn<T>[];



  /**
   * وضعیت اولیه Expansion
   */
  defaultExpandAll?:
    boolean;



  /**
   * کلاس Container
   */
  className?:
    string;


}