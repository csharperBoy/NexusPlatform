// src/core/components/Table/Table.types.ts
import React from "react";


export interface ColumnDef<T> {
  id: string;
  label: string;
  width?: string | number;
  sortable?: boolean;
  render?: (row: T) => React.ReactNode;
  accessor?: (row: T) => any; // اگر render نبود
}


export interface UseTableProps<T> {
  data: T[];
  columns: ColumnDef<T>[];

// ——— Action ———  
  onEdit? : (id:string) => void;
onDelete? : (id:string) => void;
  
// ——— Pagination ———
  pageSize?: number;
  defaultPage?: number;
  controlledPage?: number;
  onPageChange?: (page: number) => void;

  // ——— Sorting ———
  defaultSort?: { columnId: string; direction: "asc" | "desc" };
  sort?: { columnId: string; direction: "asc" | "desc" }; // controlled
  onSortChange?: (sort: { columnId: string; direction: "asc" | "desc" }) => void;

  // ——— Selection ———
  selectable?: boolean;
  cascadeSelection?: boolean;
  selected?: string[]; // external control
  defaultSelected?: string[]; // internal
  onSelectionChange?: (selectedIds: string[], selectedRows: T[]) => void;

  // ——— Generic helpers ———
  getRowId?: (row: T) => string;

  dir?: string ;
   className?: string;
}


export interface SortState {
  columnId: string;
  direction: "asc" | "desc";
}


export interface UseTableReturn<T> {
  paginatedData: T[];
  totalPages: number;

  // sorting
  sort: { columnId: string; direction: "asc" | "desc" } | null;
  toggleSort: (columnId: string) => void;

  // selection
  selectedSet: Set<string>;
  toggleSelect: (row: T) => void;
  isSelected: (row: T) => boolean;
  selectAllPage: () => void;
  clearSelection: () => void;

  // pagination
  page: number;
  setPage: (p: number) => void;

  getRowId: (row: T) => string;
}
