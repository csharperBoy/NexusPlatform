import type {
  ReactNode
}
from "react";


import type {
  DataTreeGridCellContext
}
from "./render";


import type {
  TreeNodeBase
}
from "./tree";



export interface DataTreeGridCellRenderer<
  T extends TreeNodeBase
>{


  canRender(
    context:
      DataTreeGridCellContext<T>
  ):
    boolean;



  render(
    context:
      DataTreeGridCellContext<T>
  ):
    ReactNode;


}