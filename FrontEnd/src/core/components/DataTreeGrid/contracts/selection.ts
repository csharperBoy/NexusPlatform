/**
 * DataTreeGrid Selection Contract
 *
 * مسئول مدیریت انتخاب Node ها
 *
 * فعلاً Single Selection
 * طراحی به شکلی است که Multi Selection
 * در آینده قابل اضافه شدن باشد.
 */


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