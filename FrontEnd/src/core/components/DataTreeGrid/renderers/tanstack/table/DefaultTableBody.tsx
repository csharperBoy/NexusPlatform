import type {
  Table
}
from "@tanstack/react-table";


import type {
  TreeRow
}
from "../../../types";


import type {
  TreeNodeBase,
  DataTreeGridController
}
from "../../../contracts";


import {
  DefaultTreeRow
}
from "../row";


import {
  flexRender
}
from "@tanstack/react-table";




interface DefaultTableBodyProps<
  T extends TreeNodeBase
>{

  table:
    Table<TreeRow<T>>;


  tree:
    DataTreeGridController<T>;


  rowClassName?:
    (
      row:TreeRow<T>
    )=>string;

}





export default function DefaultTableBody<
  T extends TreeNodeBase
>(
  props:
    DefaultTableBodyProps<T>
){

  return (

    <tbody>


      {
        props.table
          .getRowModel()
          .rows
          .map(
            row => (

              <DefaultTreeRow

                key={
                  row.id
                }


                row={
                  row.original
                }


                selected={

                  props.tree.selection.isSelected(
                    row.original.id
                  )

                }



                onClick={()=>{


                  console.log(
                    "ROW CLICK",
                    {
                      rowId:
                        row.original.id,

                      original:
                        row.original
                    }
                  );


                  props.tree.selection.select(
                    row.original.id
                  );


                }}


                className={

                  props.rowClassName
                  ?
                  props.rowClassName(
                    row.original
                  )
                  :
                  undefined

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


              </DefaultTreeRow>

            )
          )

      }


    </tbody>

  );

}