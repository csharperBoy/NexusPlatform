import type {
  ColumnDef
}
from "@tanstack/react-table";


import type {
  DataTreeGridColumn,
  TreeNodeBase,
  DataTreeGridController
}
from "../../contracts";


import type {
  TreeRow
}
from "../../types";

import {
  renderTreeCell
}
from "../../renderers/treeCellRenderer";





export function toTanStackColumn<
  T extends TreeNodeBase
>(

  column:
    DataTreeGridColumn<T>,


  tree:
    DataTreeGridController<T>

):
ColumnDef<TreeRow<T>>
{


  return {


    id:
      column.id,



    header:
      column.header,



    accessorFn:
      row => {

        if(!column.accessorKey)
          return undefined;


        return row.item[
          column.accessorKey
        ];

      },



    cell:
      context => {


        const row =
          context.row.original;



        if(
  column.treeColumn
)
{

  return renderTreeCell(

    row,

    context.getValue(),

    tree

  );

}




        if(column.cell)
        {

          return column.cell({

            row,

            value:
              context.getValue(),

            columnId:
              column.id

          });

        }




        return context.getValue();


      }


  };

}