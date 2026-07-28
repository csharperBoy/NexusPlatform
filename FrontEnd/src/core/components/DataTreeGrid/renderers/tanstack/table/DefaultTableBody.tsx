//src/core/components/DataTreeGrid/renderers/tanstack/table/DefaultTableBody.tsx
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
  const renderStart =
 performance.now();
console.time(
 "TABLE BODY RENDER"
);


console.log(
 "ROWS COUNT",
 props.table.getRowModel().rows.length
);


console.timeEnd(
 "TABLE BODY RENDER"
);
console.log(
 "TABLE BODY COMPLETE",
 performance.now() - renderStart,
 "ms"
);
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


              tree={
                    props.tree
                    }



                onClick={()=>{

                    console.time(
                      "ROW CLICK TO UI"
                    );


                    props.tree.expansion.toggle(
                      row.original.id
                    );


                    requestAnimationFrame(()=>{

                      console.timeEnd(
                        "ROW CLICK TO UI"
                      );

                    });

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