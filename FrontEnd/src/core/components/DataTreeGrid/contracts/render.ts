//src/core/components/DataTreeGrid/contracts/render.ts

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