/**
 * DataTreeGrid Column Contract
 */

export type ColumnEditorType =
  | "text"
  | "number"
  | "boolean"
  | "date"
  | "select"
  | "custom";



export interface ColumnSelectOption {

  label:string;

  value:string | number;

}



export interface ColumnEditorConfig {

  type:ColumnEditorType;

  options?:ColumnSelectOption[];

}



export interface DataTreeGridColumn<T> {


  id:string;


  header:string;


  accessorKey?:keyof T;


  treeColumn?:boolean;


  editable?:boolean;


  editor?:ColumnEditorConfig;


}