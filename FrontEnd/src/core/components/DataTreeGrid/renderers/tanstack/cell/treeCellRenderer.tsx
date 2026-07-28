import type {
  ReactNode
}
from "react";


import type {
  TreeRow
}
from "../../../types";


import type {
  TreeNodeBase,
  DataTreeGridController
}
from "../../../contracts";
import DefaultTreeCell from "./DefaultTreeCell";




export function renderTreeCell<
  T extends TreeNodeBase
>(

  row:
    TreeRow<T>,


  value:
    ReactNode,


  tree:
    DataTreeGridController<T>

){


  return (

    <DefaultTreeCell

      row={
        row
      }


      expanded={
        tree.expansion.isExpanded(
          row.id
        )
      }


      onToggle={
        tree.expansion.toggle
      }


    >

      {
        value
      }

    </DefaultTreeCell>

  );

}