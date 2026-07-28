//src/core/components/DataTreeGrid/renderers/tanstack/row/DefaultTreeRow.tsx
import type {
  ReactNode
} from "react";


import type {
  TreeNodeBase
} from "../../../contracts";

import type {
  TreeRow
} from "../../../types";


import type {
  DataTreeGridController
}
from "../../../contracts";

interface DefaultTreeRowProps<
  T extends TreeNodeBase
>{

  row:
    TreeRow<T>;


  tree:
    DataTreeGridController<T>;


  onClick:
    ()=>void;


  children:
    ReactNode;


  className?:
    string;

}



export default function DefaultTreeRow<
  T extends TreeNodeBase
>(
  props:
    DefaultTreeRowProps<T>
){

  return (

    <tr

      onClick={
        props.onClick
      }


      className={

        props.className
        ??
        (
          props.tree.rowState
          .get(props.row.id)
          .selected

          ?

          "bg-blue-100"

          :

          undefined

          )

      }

    >

      {
        props.children
      }

    </tr>

  );

}