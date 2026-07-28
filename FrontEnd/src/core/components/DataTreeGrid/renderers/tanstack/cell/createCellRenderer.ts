import type {
  DataTreeGridCellRenderer,
  TreeNodeBase
}
from "../../../contracts";



export function resolveCellRenderer<
 T extends TreeNodeBase
>(

 renderers:
 DataTreeGridCellRenderer<T>[],

 context:any

)
{

 return (

  renderers.find(

    renderer =>
      renderer.canRender(context)

  )

 );

}