/**
 * DataTreeGrid Render Contracts
 *
 * مخصوص لایه UI
 *
 * این فایل می‌تواند با React ارتباط داشته باشد.
 */


import type {
  TreeRow
}
from "../types";



import type {
  TreeNodeBase
}
from "./tree";




export interface DataTreeGridCellContext<
  T extends TreeNodeBase
>{


  row:
    TreeRow<T>;



  value:
    unknown;



  columnId:
    string;

}