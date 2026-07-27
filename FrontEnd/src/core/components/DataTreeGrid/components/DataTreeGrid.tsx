// src/core/components/DataTreeGrid/components/DataTreeGrid.tsx

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
  DataTreeGridProps
}
from "../contracts";


import type {
  TreeNodeBase
}
from "../contracts";


import {
  toTanStackColumn
}
from "../adapters/tanstack";


import {
  DefaultTableBody,
  DefaultTableHeader
}
from "../renderers";




export default function DataTreeGrid<
  T extends TreeNodeBase
>(
  props:
    DataTreeGridProps<T>
){


  const tree =
    props.tree;



  const columns =
    useMemo(

      () =>

        props.columns.map(

          column =>

            toTanStackColumn(
              column,
              tree
            )

        ),


      [
        props.columns,
        tree.expansion
      ]

    );




  const table =
    useReactTable({

      data:
        tree.rows,


      columns,


      getCoreRowModel:
        getCoreRowModel()

    });





  return (

    <div

      className={
        props.className
      }

    >

      <table

        className="
          w-full
          border
        "

      >


        <DefaultTableHeader

          table={
            table
          }

        />



        <DefaultTableBody

          table={
            table
          }


          tree={
            tree
          }


          rowClassName={
            props.rowClassName
          }

        />


      </table>


    </div>

  );

}