import type {
  Table
}
from "@tanstack/react-table";


import {
  DefaultTableHeader,
  DefaultTableBody
}
from ".";


import type {
  TreeRow
}
from "../../types";


import type {
  TreeNodeBase,
  DataTreeGridController
}
from "../../contracts";



interface Props<
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





export default function TanStackTableRenderer<
  T extends TreeNodeBase
>(
  props:
    Props<T>
){

  return (

    <table
      className="
        w-full
        border
      "
    >

      <DefaultTableHeader

        table={
          props.table
        }

      />


      <DefaultTableBody

        table={
          props.table
        }


        tree={
          props.tree
        }


        rowClassName={
          props.rowClassName
        }

      />


    </table>

  );

}