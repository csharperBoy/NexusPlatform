//src/core/components/DataTreeGrid/contracts/rowStateController.ts
import type {
  TreeRowState
}
from "./rowState";



export interface RowStateController {


  get(
    id:string
  ):
    TreeRowState;



  set(
    id:string,
    state:
      Partial<TreeRowState>
  ):
    void;



  clear(
    id:string
  ):
    void;



  clearAll():
    void;


}