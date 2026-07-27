//src/core/components/DataTreeGrid/renderers/treeCellRenderer.tsx
import type {
  ReactNode
}
from "react";


import type {
  TreeRow
}
from "../types";


import type {
  TreeNodeBase,
  DataTreeGridController
}
from "../contracts";


import {
  DefaultTreeCell
}
from "./cell";




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

  row={row}

  expanded={
    tree.expansion.isExpanded(row.id)
  }

  onToggle={
    tree.expansion.toggle
  }

>
  {value}
</DefaultTreeCell>

  );

}