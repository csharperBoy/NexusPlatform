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
  DataTreeGridController,
  TreeAdapter,
  TreeNodeBase
}
from "./tree";


import type {
  DataTreeGridColumn
}
from "./column";
import { TreeRow } from "../types";





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


    tree:
  DataTreeGridController<T>;

  /**
   * کلاس Container
   */
  className?:
    string;

    rowClassName?:
    (
      row: TreeRow<T>
    )=>
    string;


}