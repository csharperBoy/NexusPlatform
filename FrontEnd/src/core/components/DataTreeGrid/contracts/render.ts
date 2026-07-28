import type {
  TreeRow
}
from "../types";


import type {
  TreeNodeBase
}
from "./tree";


import type {
  DataTreeGridColumn
}
from "./column";



export interface DataTreeGridCellContext<
  T extends TreeNodeBase
>{


  /**
   * Row کامل Tree
   */
  row:
    TreeRow<T>;



  /**
   * مقدار خام Cell
   */
  value:
    unknown;



  /**
   * تعریف Column
   *
   * شامل:
   * treeColumn
   * formatter
   * editable
   * editor
   * ...
   */
  column:
    DataTreeGridColumn<T>;

}