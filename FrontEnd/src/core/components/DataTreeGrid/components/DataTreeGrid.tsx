//src/core/components/DataTreeGrid/components/DataTreeGrid.tsx
import type {
  DataTreeGridProps
}
from "../contracts";


import type {
  TreeNodeBase
}
from "../contracts";


import {
  useTanStackDataTreeGrid
}
from "../hooks/useTanStackDataTreeGrid";


import {
  TanStackTableRenderer
}
from "../renderers";




export default function DataTreeGrid<
  T extends TreeNodeBase
>(
  props:
    DataTreeGridProps<T>
){


  const tree =
    props.tree;



  const table =
    useTanStackDataTreeGrid(

      props.columns,

      tree

    );




  return (

    <div

      className={
        props.className
      }

    >

      <TanStackTableRenderer

        table={
          table
        }


        tree={
          tree
        }


        rowClassName={
          props.rowClassName
        }

      />


    </div>

  );

}