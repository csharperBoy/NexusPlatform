import TreeCell from "../components/TreeCell";

import type {
  TreeRow
}
from "../types";


import type {
  TreeNodeBase,
  DataTreeGridController
}
from "../contracts";



export function renderTreeCell<
  T extends TreeNodeBase
>(
  row: TreeRow<T>,
  value: unknown,
  tree: DataTreeGridController<T>
){

  return (

    <TreeCell

      row={row}

      expansion={
        tree.expansion
      }

      value={
        value
      }

    />

  );

}