import {
  flexRender
}
from "@tanstack/react-table";


import type {
  Row
}
from "@tanstack/react-table";


import type {
  TreeRow as TreeRowType
}
from "../types";


import type {
  TreeNodeBase,
  DataTreeGridController
}
from "../contracts";




interface TreeRowProps<
  T extends TreeNodeBase
>{


  row:
    Row<TreeRowType<T>>;


  tree:
    DataTreeGridController<T>;


  rowClassName?:
    (
      row: TreeRowType<T>
    )=>string;

}




export default function TreeRow<
  T extends TreeNodeBase
>(
  props:
    TreeRowProps<T>
){


  const {
    row,
    tree,
    rowClassName
  } = props;



  const original =
    row.original;



  return (


    <tr


      key={
        row.id
      }



      onClick={() => {


        console.log(
          "ROW CLICK",
          {
            rowId:
              original.id,

            original
          }
        );



        tree.selection.select(
          original.id
        );


      }}



      className={

        rowClassName
          ?
          rowClassName(original)
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


                style={{

                  background:

                    tree.selection.isSelected(
                      original.id
                    )

                    ?

                    "red"

                    :

                    "transparent"

                }}


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


  );

}