/**
 * DataTreeGrid Public Contracts
 *
 * این فایل API عمومی DataTreeGrid است.
 *
 * این فایل نباید هیچ وابستگی به:
 * React
 * UI Framework
 * Backend
 * Business Module
 * داشته باشد.
 */

import {   TreeIndex,TreeRow } from "./types";


// =====================================================
// Base Node
// =====================================================


export interface TreeNodeBase {

  id:string;

}



// =====================================================
// Adapter
// =====================================================

export interface TreeAdapter<
  T extends TreeNodeBase
> {


  /**
   * شناسه یکتا
   */
  getId(
    item:T
  ):string;



  /**
   * Parent فعلی
   */
  getParentId(
    item:T
  ):string|null;



  /**
   * تغییر Parent به صورت Immutable
   */
  setParentId(
    item:T,
    parentId:string|null
  ):T;




  /**
   * عنوان نمایشی Node
   *
   * برای Tree Column
   */
  getLabel?(
    item:T
  ):string;



  /**
   * آیا Node قابلیت جابه‌جایی دارد؟
   *
   * برای Drag & Drop آینده
   */
  canMove?(
    item:T
  ):boolean;



  /**
   * آیا Node قابل ویرایش است؟
   */
  canEdit?(
    item:T
  ):boolean;


}





// =====================================================
// Hook Options
// =====================================================


export interface UseDataTreeGridOptions<
  T extends TreeNodeBase
>{


  data:readonly T[];



  adapter:TreeAdapter<T>;



  defaultExpandedIds?:
    readonly string[];



  defaultExpandAll?:
    boolean;

}



// =====================================================
// Validation
// =====================================================


export interface MoveResult {


  allowed:boolean;



  reason?:string;

}



// =====================================================
// Public Controllers
// =====================================================


export interface TreeExpansionController {


  expandedIds:
    ReadonlySet<string>;



  isExpanded(
    id:string
  ):boolean;



  expand(
    id:string
  ):void;



  collapse(
    id:string
  ):void;



  toggle(
    id:string
  ):void;



  expandAll():void;



  collapseAll():void;



  expandToLevel(
    level:number
  ):void;

}





export interface TreeNavigationController<
  T extends TreeNodeBase
>{


  findNode(
    id:string
  ):T|undefined;



  parent(
    id:string
  ):T|undefined;



  children(
    id:string
  ):T[];



  descendants(
    id:string
  ):T[];



  ancestors(
    id:string
  ):T[];

}




export interface TreeValidationController {


  isRoot(
    id:string
  ):boolean;



  isLeaf(
    id:string
  ):boolean;



  isDescendant(
    sourceId:string,
    targetId:string
  ):boolean;



  canMove(
    sourceId:string,
    targetId:string|null
  ):MoveResult;


}




export interface TreeManipulationController<
  T extends TreeNodeBase
>{


  moveNode(
    nodeId:string,
    newParentId:string|null
  ):T[];


}




// =====================================================
// Main Controller
// =====================================================


export interface DataTreeGridController<
  T extends TreeNodeBase
>{


  rows:
    TreeRow<T>[];


  /**
   * Internal tree index
   *
   * برای عملیات پیشرفته و Debug
   */
  index:
    TreeIndex<T>;


  expansion:
    TreeExpansionController;



  navigation:
    TreeNavigationController<T>;



  validation:
    TreeValidationController;



  manipulation:
    TreeManipulationController<T>;

}