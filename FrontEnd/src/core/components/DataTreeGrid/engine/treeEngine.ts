//src/core/components/DataTreeGrid/engine/treeEngine.ts
import {
    MoveResult,
    TreeAdapter,
  TreeNodeBase,
} from "../contracts/tree";
import { TreeIndex } from "../types";



/**
 * ساخت Rows قابل نمایش برای Grid
 *
 * فقط Node هایی که در مسیر Expand شده هستند
 * برگردانده می‌شوند.
 *
 * Complexity:
 * O(n)
 */
export function flattenTree<T extends TreeNodeBase>(
  index: TreeIndex<T>,
  expandedIds: ReadonlySet<string>
) {

  const rows: {
    id: string;
    item: T;
    parentId: string | null;
    depth: number;
    hasChildren: boolean;
  }[] = [];


  function visit(
    id: string,
    depth: number,
    parentId: string | null
  ) {

    const item =
      index.nodeMap.get(id);


    if (!item) {
      return;
    }


    const children =
      index.childrenMap.get(id) ?? [];


    rows.push({

      id,

      item,

      parentId,

      depth,

      hasChildren:
        children.length > 0,

    });



    if (!expandedIds.has(id)) {
      return;
    }



    for (const childId of children) {

      visit(
        childId,
        depth + 1,
        id
      );

    }

  }



  for (const rootId of index.rootIds) {

    visit(
      rootId,
      0,
      null
    );

  }


  return rows;

}





// =====================================================
// Navigation
// =====================================================



export function findNode<T extends TreeNodeBase>(
  index: TreeIndex<T>,
  id:string
):T|undefined {

  return index.nodeMap.get(id);

}





export function findParent<T extends TreeNodeBase>(
  index:TreeIndex<T>,
  id:string
):T|undefined {


  const parentId =
    index.parentMap.get(id);


  if (!parentId) {
    return undefined;
  }


  return index.nodeMap.get(parentId);

}





export function findChildren<T extends TreeNodeBase>(
  index:TreeIndex<T>,
  id:string
):T[] {


  const childrenIds =
    index.childrenMap.get(id) ?? [];


  return childrenIds
    .map(x => index.nodeMap.get(x))
    .filter(
      (x): x is T => x !== undefined
    );

}







export function findDescendants<T extends TreeNodeBase>(
  index:TreeIndex<T>,
  id:string
):T[] {


  const result:T[]=[];



  function visit(nodeId:string){


    const children =
      index.childrenMap.get(nodeId) ?? [];



    for(const childId of children){


      const child =
        index.nodeMap.get(childId);


      if(child){

        result.push(child);

        visit(childId);

      }

    }

  }



  visit(id);


  return result;

}


export interface MoveValidationResult {

  allowed: boolean;

  reason?: string;

}




export function validateMove<T extends TreeNodeBase>(

  index: TreeIndex<T>,

  sourceId:string,

  targetParentId:string | null

): MoveValidationResult {



  /**
   * انتقال به Root همیشه مجاز است
   */
  if(targetParentId === null)
  {
    return {
      allowed:true
    };
  }





  /**
   * انتقال Node به خودش
   */
  if(sourceId === targetParentId)
  {
    return {

      allowed:false,

      reason:
        "یک Node نمی‌تواند زیرمجموعه خودش قرار بگیرد"

    };
  }





  /**
   * انتقال به داخل زیرمجموعه خودش
   */
  if(
    isDescendant(
      index,
      sourceId,
      targetParentId
    )
  )
  {

    return {

      allowed:false,

      reason:
        "نمی‌توان Node را زیرمجموعه یکی از فرزندان خودش قرار داد"

    };

  }





  return {

    allowed:true

  };

}

export function findAncestors<T extends TreeNodeBase>(
  index:TreeIndex<T>,
  id:string
):T[] {


  const result:T[]=[];


  let parentId =
    index.parentMap.get(id);



  while(parentId){


    const parent =
      index.nodeMap.get(parentId);


    if(!parent){
      break;
    }


    result.push(parent);


    parentId =
      index.parentMap.get(parentId);

  }



  return result;

}





// =====================================================
// Validation
// =====================================================



export function isRoot<T extends TreeNodeBase>(
  index:TreeIndex<T>,
  id:string
):boolean {

  return (
    index.parentMap.get(id)
    === null
  );

}





export function isLeaf<T extends TreeNodeBase>(
  index:TreeIndex<T>,
  id:string
):boolean {


  const children =
    index.childrenMap.get(id);


  return (
    !children ||
    children.length === 0
  );

}







/**
 * آیا target زیرمجموعه source است؟
 *
 * مثال:
 *
 * CEO
 *  |
 *  Finance
 *
 * isDescendant(
 *   CEO,
 *   Finance
 * )
 *
 * true
 */
export function isDescendant<T extends TreeNodeBase>(
  index: TreeIndex<T>,
  sourceId: string,
  targetId: string
): boolean {

  const children =
    index.childrenMap.get(sourceId) ?? [];


  for (const childId of children) {


    if (childId === targetId) {
      return true;
    }


    if (
      isDescendant(
        index,
        childId,
        targetId
      )
    ) {
      return true;
    }

  }


  return false;
}


/**
 * آیا انتقال مجاز است؟
 */
export function canMove<T extends TreeNodeBase>(
  index:TreeIndex<T>,
  sourceId:string,
  targetParentId:string|null
):boolean {


  // انتقال به خودش ممنوع
  if(sourceId === targetParentId){
    return false;
  }



  // انتقال به زیرمجموعه خودش ممنوع
  if(
    targetParentId &&
    isDescendant(
      index,
      sourceId,
      targetParentId
    )
  ){
    return false;
  }



  return true;

}






// =====================================================
// Mutation helpers
// =====================================================



/**
 * تغییر Parent یک Node
 *
 * فقط داده جدید برمی‌گرداند.
 */
export function moveNode<T extends TreeNodeBase>(
  items: readonly T[],
  nodeId: string,
  newParentId: string | null,
  adapter: TreeAdapter<T>
): T[] {


  return items.map(item => {

    if (
      adapter.getId(item) !== nodeId
    ) {
      return item;
    }


    return adapter.setParentId(
      item,
      newParentId
    );

  });

}
