import type {
  DataTreeGridCellRenderer,
  TreeNodeBase
}
from "../../../contracts";



export function createDefaultCellRenderer<
  T extends TreeNodeBase
>():
DataTreeGridCellRenderer<T>
{


  return {


    canRender(){

      return true;

    },



    render(context){


      return <>{context.value}</>;


    }


  };


}