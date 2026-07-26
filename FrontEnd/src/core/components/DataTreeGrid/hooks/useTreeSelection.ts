// //src/core/components/DataTreeGrid/hooks/useTreeSelection.ts
// import {
//   useState
// }
// from "react";


// import type {
//   TreeSelectionController
// }
// from "../contracts";




// export function useTreeSelection()
// :
// TreeSelectionController
// {


//   const [
//     selectedId,
//     setSelectedId
//   ] = useState<string | null>(null);




//   return {


//     selectedId,



//     isSelected(
//       id:string
//     ){

//       return selectedId === id;

//     },



//     select(
//       id:string
//     ){

//          console.log(
//    "SELECT In Hook =",
//    id
//  );
//       setSelectedId(id);

//     },



//     clear(){

//       setSelectedId(null);

//     }


//   };

// }