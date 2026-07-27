//SRC/core/components/DataTreeGrid/adapters/tanstack/columnAdapter.tsx
import type {
  ReactNode
} from "react";

import type {
  ColumnDef
}
from "@tanstack/react-table";


import type {
  DataTreeGridColumn
}
from "../../contracts/column";


import type {
  TreeNodeBase
}
from "../../contracts/tree";


import type {
  TreeRow
}
from "../../types";


import {
 renderTreeCell
}
from "../../renderers/tanstack";



export function toTanStackColumn<
  T extends TreeNodeBase
>(

  column:
    DataTreeGridColumn<T>,

  tree:any

):
ColumnDef<TreeRow<T>>
{


  return {


    id:
      column.id,



    header:
      column.header,



    accessorKey:
      column.accessorKey as string,



    cell:
      ({row})=>{


        const treeRow =
          row.original;



        const value =
          column.accessorKey
            ?
            treeRow.item[
              column.accessorKey
            ]
            :
            null;




        if(column.treeColumn){

          return renderTreeCell(
            treeRow,
            value as ReactNode,
            tree
          );

          }




        return (

          <>

            {
              value as ReactNode
            }

          </>

        );


      }


  };


}