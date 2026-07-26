import type {
  TreeRow
} from "../types";


import type {
  TreeNodeBase
}
from "../contracts";


import type {
  TreeExpansionController
}
from "../contracts";




interface TreeCellProps<
  T extends TreeNodeBase
>{

  row:
    TreeRow<T>;


  expansion:
    TreeExpansionController;


  value:
    unknown;

}




export default function TreeCell<
  T extends TreeNodeBase
>(
  {
    row,
    expansion,
    value
  }:
  TreeCellProps<T>
){


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

            className="
              w-6
              h-6
              border
              rounded
            "


            onClick={
              (event)=>{

                event.stopPropagation();


                expansion.toggle(
                  row.id
                );

              }
            }

          >

            {
              expansion.isExpanded(
                row.id
              )
                ?
                "-"
                :
                "+"
            }

          </button>

        )
      }



      <span>

        {
          String(
            value ?? ""
          )
        }

      </span>



    </div>

  );

}