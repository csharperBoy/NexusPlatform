import type {
  ReactNode
} from "react";


import type {
  DataTreeGridController,
  TreeNodeBase
}
from "./tree";



export interface DataTreeGridRendererProps<
  T extends TreeNodeBase
>{

  tree:
    DataTreeGridController<T>;


  children:
    ReactNode;

}



export interface DataTreeGridRenderer<
  T extends TreeNodeBase
>{

  render(
    props:
      DataTreeGridRendererProps<T>
  ):
    ReactNode;

}