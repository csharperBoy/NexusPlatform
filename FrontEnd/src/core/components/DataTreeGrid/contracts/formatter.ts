import type {
  ReactNode
}
from "react";

export interface FormatterContext<T>{

  value:
    unknown;

  row:
    T;

}

export type CellFormatter<T> = (

  context:
    FormatterContext<T>

)=>
  ReactNode;