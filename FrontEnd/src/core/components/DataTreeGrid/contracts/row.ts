import type {
  ReactNode
} from "react";


import type {
  TreeRow
} from "../types";


import type {
  TreeNodeBase,
  DataTreeGridController
} from "./tree";



export interface DataTreeGridRowContext<
  T extends TreeNodeBase
>{


  row:
    TreeRow<T>;



  tree:
    DataTreeGridController<T>;



  selected:
    boolean;



  onClick():
    void;



  children:
    ReactNode;

}