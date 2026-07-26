//src/core/components/DataTreeGrid/contracts/selection.ts

export interface TreeSelectionController {


  /**
   * Node انتخاب شده
   *
   * چون Single Selection داریم:
   * فقط یک مقدار داریم.
   */
  selectedId:
    string | null;



  /**
   * آیا Node انتخاب شده است؟
   */
  isSelected(
    id:string
  ):
    boolean;



  /**
   * انتخاب Node
   */
  select(
    id:string
  ):
    void;



  /**
   * پاک کردن انتخاب
   */
  clear():
    void;


}