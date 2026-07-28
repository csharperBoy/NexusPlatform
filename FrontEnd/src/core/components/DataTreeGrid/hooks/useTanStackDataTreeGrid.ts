//src/core/components/DataTreeGrid/hooks/useTanStackDataTreeGrid.ts
import {
  useMemo
}
from "react";


import {
  useReactTable,
  getCoreRowModel,
}
from "@tanstack/react-table";


import type {
  DataTreeGridColumn,
  DataTreeGridController,
  TreeNodeBase
}
from "../contracts";


import type {
  TreeRow
}
from "../types";


import {
  toTanStackColumn
}
from "../adapters/tanstack";



export function useTanStackDataTreeGrid<
  T extends TreeNodeBase
>(

  columns:
    DataTreeGridColumn<T>[],

  tree:
    DataTreeGridController<T>

){


  const tanStackColumns =
useMemo(

()=>{

 console.time(
   "CREATE TANSTACK COLUMNS"
 );


 const result =
 columns.map(

   column =>
     toTanStackColumn(
       column,
       tree
     )

 );


 console.timeEnd(
   "CREATE TANSTACK COLUMNS"
 );


 return result;


},


      [
        columns,
        tree.expansion
      ]

    );



console.time(
 "CREATE TANSTACK TABLE"
);
  const table =
    useReactTable({

      data:
        tree.rows,


      columns:
        tanStackColumns,


      getCoreRowModel:
        getCoreRowModel()

    });

console.timeEnd(
 "CREATE TANSTACK TABLE"
);

  return table;

}