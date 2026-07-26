//src/core/components/DataTreeGrid/contracts/formatter.ts


export interface FormatterContext<T>{


  /**
   * مقدار خام سلول
   */
  value:
    unknown;



  /**
   * کل Row
   */
  row:
    T;


}





export type CellFormatter<T> = (

  context:
    FormatterContext<T>

) => string;
