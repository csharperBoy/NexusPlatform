//src/core/components/DataTreeGrid/contracts/rowState.ts
// =====================================================
// Row UI State
// =====================================================


export interface TreeRowState {


  /**
   * انتخاب شده
   */
  selected:boolean;



  /**
   * در حال ویرایش
   */
  editing:boolean;



  /**
   * در حال Loading
   */
  loading:boolean;



  /**
   * غیر فعال
   */
  disabled:boolean;



  /**
   * Focus شده
   */
  focused:boolean;

}