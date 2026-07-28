import type {
  ReactNode
}
from "react";


import type {
  DataTreeGridCellRenderer,
  TreeNodeBase
}
from "../../../contracts";


import {
  renderTreeCell
}
from "./treeCellRenderer";



export function createTanStackTreeCellRenderer<
  T extends TreeNodeBase
>(

 tree:any

):
DataTreeGridCellRenderer<T>
{


 return {


  canRender(context){


    return (
      context.column.treeColumn === true
    );


  },



  render(context){


    return renderTreeCell(

      context.row,

      context.value as ReactNode,

      tree

    );


  }


 };


}