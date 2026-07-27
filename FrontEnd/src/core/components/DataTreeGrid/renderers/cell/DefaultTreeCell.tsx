//src/core/components/DataTreeGrid/renderers/default/DefaultTreeCell.tsx
import type {
  ReactNode
} from "react";


import type {
  TreeNodeBase
} from "../../contracts";


import type {
  TreeRow
} from "../../types";




interface DefaultTreeCellProps<
  T extends TreeNodeBase
>{


  row:
    TreeRow<T>;


  expanded:
    boolean;


  onToggle:
    (
      id:string
    )=>void;


  children:
    ReactNode;

}





export default function DefaultTreeCell<
  T extends TreeNodeBase
>(
  props:
    DefaultTreeCellProps<T>
){


  const {
    row,
    expanded,
    onToggle,
    children
  } = props;



  return (

    <div

      className="
        flex
        items-center
        gap-2
      "

      style={{
        paddingRight:
          row.depth * 24
      }}

    >


      {
        row.hasChildren && (

          <button

            type="button"

            onClick={(event)=>{

              event.stopPropagation();

              onToggle(
                row.id
              );

            }}

          >

            {
              expanded
                ?
                "-"
                :
                "+"
            }


          </button>

        )
      }



      {children}



    </div>

  );

}