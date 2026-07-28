//src/core/components/DataTreeGrid/hooks/useRowState.ts
import {
  useCallback,
  useState
} from "react";

import {
  useMemo
} from "react";

import type {
  RowStateController
} from "../contracts/rowStateController";


import type {
  TreeRowState
} from "../contracts/rowState";



const defaultRowState:TreeRowState =
{

  selected:false,

  editing:false,

  loading:false,

  disabled:false,

  focused:false

};



export function useRowState()
:
RowStateController
{


  const [
    states,
    setStates
  ] =
//   useState(
//  ()=>new Map<string,TreeRowState>()
// )
   useState<
     Map<string,TreeRowState>
   >(
     new Map()
   );




  const get =
  useCallback(

    (
      id:string
    )
    :
    TreeRowState => {


      return (

        states.get(id)

        ??

        defaultRowState

      );


    },

    [
      states
    ]

  );





  const set =
  useCallback(

    (
      id:string,

      state:
        Partial<TreeRowState>

    )=>{


      setStates(

        previous => {


          const next =
            new Map(previous);



          const current =
            next.get(id)
            ??
            defaultRowState;



          next.set(

            id,

            {

              ...current,

              ...state

            }

          );



          return next;

        }

      );


    },

    []

  );





  const clear =
  useCallback(

    (
      id:string
    )=>{


      setStates(

        previous => {


          const next =
            new Map(previous);



          next.delete(id);



          return next;


        }

      );


    },

    []

  );





  const clearAll =
  useCallback(

    ()=>{


      setStates(
        new Map()
      );


    },

    []

  );





  return useMemo(

()=>({

    get,

    set,

    clear,

    clearAll

}),

[
 get,
 set,
 clear,
 clearAll
]

);


}