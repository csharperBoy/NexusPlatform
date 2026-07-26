/**
 * DataTreeGrid Formatter Contracts
 *
 * مسئول تبدیل Data به View Model است.
 *
 * مثال:
 *
 * 1  ---> مرد
 *
 * 1405-01-01 ---> 01/01/1405
 *
 */


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
