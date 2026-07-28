//src/core/components/DataTreeGrid/adapters/tanstack/columnAdapter.tsx

import type {
  ColumnDef
}
from "@tanstack/react-table";


import type {
  ReactNode
}
from "react";


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
  createTanStackTreeCellRenderer,
  createDefaultCellRenderer,
  resolveCellRenderer
}
from "../../renderers";




export function toTanStackColumn<
  T extends TreeNodeBase
>(

  column:
    DataTreeGridColumn<T>,


  tree:any

):
ColumnDef<TreeRow<T>>
{


  const renderers = [

        createDefaultCellRenderer<T>()

      ];


      if(column.treeColumn){

        renderers.unshift(

          createTanStackTreeCellRenderer<T>(
            tree
          )

        );

      }




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



        const rawValue =

          column.accessorKey

            ?

            treeRow.item[
              column.accessorKey
            ]

            :

            null;



        let value:
          ReactNode =
            rawValue as ReactNode;



        if(column.formatter){


          value =

            column.formatter({

              value:
                rawValue,


              row:
                treeRow.item

            });

        }




        const context = {


          row:
            treeRow,


          rawValue,


          value,


          column


        };




        /**
         * اگر cell اختصاصی تعریف شده باشد
         * اولویت اول دارد
         */
        if(column.cell){


          return column.cell(
            context
          );

        }




        const renderer =

          resolveCellRenderer(

            renderers,

            context

          );




        return renderer

          ?

          renderer.render(
            context
          )

          :

          null;


      }


  };


}