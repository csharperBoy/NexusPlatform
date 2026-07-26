//src/core/components/DataTreeGrid/types/index.ts


import {
  TreeNodeBase
} from "../contracts/tree";




// =====================================================
// Tree Index
// =====================================================


export interface TreeIndex<
  T extends TreeNodeBase
>{


  items:
    readonly T[];



  nodeMap:
    ReadonlyMap<string,T>;



  parentMap:
    ReadonlyMap<string,string|null>;



  childrenMap:
    ReadonlyMap<string|null,readonly string[]>;



  rootIds:
    readonly string[];

}





// =====================================================
// Diagnostics
// =====================================================


export interface MissingParentDiagnostic {


  nodeId:string;


  parentId:string;

}



export interface TreeDiagnostics {


  duplicateIds:
    readonly string[];



  missingParents:
    readonly MissingParentDiagnostic[];

}





// =====================================================
// Build Result
// =====================================================


export interface TreeBuildResult<
  T extends TreeNodeBase
>{


  index:
    TreeIndex<T>;



  diagnostics:
    TreeDiagnostics;

}




// =====================================================
// Render Row
// =====================================================


export interface TreeRow<
  T extends TreeNodeBase
>{


  id:string;



  item:T;



  parentId:string|null;



  depth:number;



  hasChildren:boolean;

}