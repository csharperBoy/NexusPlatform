import {
  useMemo
}
from "react";


import {
  useReactTable,
  getCoreRowModel,
}
from "@tanstack/react-table";
import {
 flexRender
}
from "@tanstack/react-table";

import type {
  DataTreeGridProps
}
from "../contracts";


import {
  useDataTreeGrid
}
from "../hooks/useDataTreeGrid";


import {
  toTanStackColumn
}
from "../adapters/tanstack";


import type {
  TreeNodeBase
}
from "../contracts";





export default function DataTreeGrid<
  T extends TreeNodeBase
>(
  props:
    DataTreeGridProps<T>
) {


  const tree =
    useDataTreeGrid({

      data:
        props.data,

      adapter:
        props.adapter,

      defaultExpandAll:
        props.defaultExpandAll

    });




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
      tree
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
        className="w-full border"
      >


        <thead>

          {
            table
              .getHeaderGroups()
              .map(
                headerGroup => (

                  <tr
                    key={
                      headerGroup.id
                    }
                  >

                    {
                      headerGroup.headers.map(
                        header => (

                          <th
                            key={
                              header.id
                            }
                            className="
                              border
                              p-2
                            "
                          >

                            {
 flexRender(

   header.column.columnDef.header,

   header.getContext()

 )
}

                          </th>

                        )
                      )
                    }

                  </tr>

                )
              )
          }

        </thead>



        <tbody>


          {
            table
              .getRowModel()
              .rows
              .map(
                row => (

                  <tr
                    key={
                      row.id
                    }
                  >

                    {
                      row
                        .getVisibleCells()
                        .map(
                          cell => (

                            <td
                              key={
                                cell.id
                              }
                              className="
                                border
                                p-2
                              "
                            >

                              {
                               flexRender(

  cell.column.columnDef.cell,

  cell.getContext()

)
                              }

                            </td>

                          )
                        )
                    }


                  </tr>

                )
              )

          }


        </tbody>


      </table>


    </div>

  );

}