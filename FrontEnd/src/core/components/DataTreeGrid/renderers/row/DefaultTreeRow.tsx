import type {
  ReactNode
} from "react";


import type {
  TreeNodeBase
} from "../../contracts";

import type {
  TreeRow
} from "../../types";



interface DefaultTreeRowProps<
  T extends TreeNodeBase
>{

  row:
    TreeRow<T>;


  selected:
    boolean;


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
          props.selected
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