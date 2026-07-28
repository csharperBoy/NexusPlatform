//src/core/components/DataTreeGrid/hooks/useDataTreeGrid.ts
import {
  useCallback,
  useMemo,
  useState,
} from "react";



import {
  buildTree
} from "../engine/treeBuilder";

import {
  useRowState
}
from "./useRowState";

import {
  flattenTree,
  findNode,
  findParent,
  findChildren,
  findDescendants,
  findAncestors,
  validateMove,
  moveNode,
  isDescendant,
} from "../engine/treeEngine";
import { DataTreeGridController, TreeNodeBase, UseDataTreeGridOptions } from "../contracts/tree";



export function useDataTreeGrid<
  T extends TreeNodeBase
>(
  options:UseDataTreeGridOptions<T>
):DataTreeGridController<T> {


  const {
    data,
    adapter,
    defaultExpandedIds = [],
    defaultExpandAll = false
  } = options;



  /**
   * ساخت Tree Index
   *
   * فقط وقتی Data تغییر کند
   */
  const buildResult =
    useMemo(
      ()=>buildTree(
        data,
        adapter
      ),
      [
        data,
        adapter
      ]
    );



  const {
    index
  } = buildResult;





  // -----------------------------
  // Expansion State
  // -----------------------------


  const [
    expandedIds,
    setExpandedIds
  ] = useState<Set<string>>(
    ()=>{


      if(defaultExpandAll){

        return new Set(
          index.nodeMap.keys()
        );

      }


      return new Set(
        defaultExpandedIds
      );

    }
  );

  const [
  selectedId,
  setSelectedId
] = useState<string | null>(null);


const rowState =
  useRowState();

  const isExpanded =
    useCallback(
      (id:string)=>
        expandedIds.has(id),
      [
        expandedIds
      ]
    );





  const expand =
    useCallback(
      (id:string)=>{

        setExpandedIds(
          previous=>{

            const next =
              new Set(previous);

            next.add(id);

            return next;

          }
        );

      },
      []
    );





  const collapse =
    useCallback(
      (id:string)=>{

        setExpandedIds(
          previous=>{

            const next =
              new Set(previous);

            next.delete(id);

            return next;

          }
        );

      },
      []
    );




const toggle =
  useCallback(
    (id:string)=>{

      console.time(
        "TREE TOGGLE TOTAL"
      );


      setExpandedIds(
        previous=>{


          console.time(
            "CREATE NEW EXPANDED IDS"
          );


          const next =
            new Set(previous);


          if(next.has(id)){
            next.delete(id);
          }
          else{
            next.add(id);
          }


          console.timeEnd(
            "CREATE NEW EXPANDED IDS"
          );


          return next;

        }
      );


      console.timeEnd(
        "TREE TOGGLE TOTAL"
      );

    },
    []
  );






  const expandAll =
    useCallback(()=>{

      setExpandedIds(
        new Set(
          index.nodeMap.keys()
        )
      );

    },[
      index
    ]);






  const collapseAll =
    useCallback(()=>{

      setExpandedIds(
        new Set()
      );

    },[]);





  // -----------------------------
  // Rows
  // -----------------------------


  const rows =
    useMemo(
      ()=>flattenTree(
        index,
        expandedIds
      ),
      [
        index,
        expandedIds
      ]
    );




const selection =
useMemo(

()=>({

  selectedId,


  isSelected(
    id:string
  ){

    return selectedId === id;

  },


  select(
    id:string
  ){

    setSelectedId(id);

  },


  clear(){

    setSelectedId(null);

  }


}),

[
 selectedId
]

);
  // -----------------------------
  // Controller
  // -----------------------------
console.log(
  "TREE CONTROLLER RENDER"
);

  return {


    rows,


    index,


    rowState,

    expansion:{


      expandedIds,


      isExpanded,


      expand,


      collapse,


      toggle,


      expandAll,


      collapseAll,


      expandToLevel(level:number){

        const ids =
          new Set<string>();


        function visit(
          id:string,
          depth:number
        ){

          if(depth >= level){
            return;
          }


          ids.add(id);


          const children =
            index.childrenMap.get(id) ?? [];


          children.forEach(
            child =>
              visit(
                child,
                depth+1
              )
          );

        }



        index.rootIds.forEach(
          root =>
            visit(
              root,
              0
            )
        );

        setExpandedIds(ids);

      }

    },




    navigation:{


      findNode:
        (id)=>
          findNode(
            index,
            id
          ),



      parent:
        (id)=>
          findParent(
            index,
            id
          ),



      children:
        (id)=>
          findChildren(
            index,
            id
          ),



      descendants:
        (id)=>
          findDescendants(
            index,
            id
          ),



      ancestors:
        (id)=>
          findAncestors(
            index,
            id
          )

    },



validation:{


  isRoot(id){

    return (
      index.parentMap.get(id)
      === null
    );

  },


  isLeaf(id){

    const children =
      index.childrenMap.get(id);

    return (
      !children ||
      children.length === 0
    );

  },


  isDescendant(
    sourceId,
    targetId
  ){

    return isDescendant(
      index,
      sourceId,
      targetId
    );

  },


   canMove:(

     sourceId:string,

     targetParentId:string|null

   )=>{


     return validateMove(

       index,

       sourceId,

       targetParentId

     );


   }

},




    manipulation:{


      moveNode(
        nodeId,
        newParentId
      ){

        return moveNode(
          data,
          nodeId,
          newParentId,
          adapter
        );

      }

    },

    selection

  };

}