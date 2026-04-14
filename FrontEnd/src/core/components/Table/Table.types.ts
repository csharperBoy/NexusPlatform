// src/core/components/Table/Table.types.ts
// -------------------------------------------
// Core Types for Advanced Modular Table
// -------------------------------------------

export type Direction = "ltr" | "rtl";

export interface SortState {
  columnId: string;
  direction: "asc" | "desc";
}

export interface FilterState {
  columnId: string;
  value: any;
}

export interface PaginationState {
  page: number;
  pageSize: number;
}

// -------------------------------------------
// Column Definition (Supports: levels, drag, resize, sticky, renderers)
// -------------------------------------------

export interface ColumnDef<T> {
  id: string;

  header: string | React.ReactNode;
  accessor?: (row: T) => any;

  width?: number;
  minWidth?: number;
  maxWidth?: number;
  resizable?: boolean;

  draggable?: boolean;

  sticky?: "left" | "right";
  stickyOffset?: number;

  sortable?: boolean;
  filterable?: boolean;

  renderCell?: (row: T, index: number) => React.ReactNode;
  renderHeader?: () => React.ReactNode;

  children?: ColumnDef<T>[]; // Multi-level header
}

// -------------------------------------------
// Selection Types
// -------------------------------------------

export type SelectionMode = "none" | "single" | "multiple";

export interface SelectionState {
  ids: string[];
}

// -------------------------------------------
// Row Expansion
// -------------------------------------------

export interface ExpandedRowState {
  ids: string[];
}

// -------------------------------------------
// Theme System
// -------------------------------------------

export interface TableTheme {
  borderColor?: string;
  headerBg?: string;
  rowHoverBg?: string;
  stickyHeaderShadow?: string;
  fontSize?: string;
  density?: "compact" | "normal" | "comfortable";
}

// -------------------------------------------
// Props for Advanced Table Component
// -------------------------------------------

export interface TableProps<T> {
  data: T[];
  columns: ColumnDef<T>[];

  getRowId?: (row: T) => string;

  // -----------------------------
  // Sorting
  // -----------------------------
  sortable?: boolean;
  sort?: SortState | null;
  defaultSort?: SortState | null;
  onSortChange?: (sort: SortState | null) => void;
  serverSort?: boolean;

  // -----------------------------
  // Filtering
  // -----------------------------
  filterable?: boolean;
  filters?: FilterState[];
  defaultFilters?: FilterState[];
  onFiltersChange?: (filters: FilterState[]) => void;
  serverFiltering?: boolean;

  // -----------------------------
  // Pagination
  // -----------------------------
  pagination?: boolean;
  paginationState?: PaginationState;
  defaultPaginationState?: PaginationState;
  onPaginationChange?: (state: PaginationState) => void;
  serverPagination?: boolean;

  // -----------------------------
  // Selection
  // -----------------------------
  selectionMode?: SelectionMode;
  cascadeSelection?: boolean;
  selected?: string[];
  defaultSelected?: string[];
  onSelectedChange?: (selectedIds: string[], rows: T[]) => void;

  // -----------------------------
  // Row Expansion
  // -----------------------------
  expandableRows?: boolean;
  expandedRows?: string[];
  defaultExpandedRows?: string[];
  onExpandedRowsChange?: (ids: string[]) => void;
  renderExpandedRow?: (row: T) => React.ReactNode;

  // -----------------------------
  // Virtualization
  // -----------------------------
  virtualized?: boolean;
  rowHeight?: number;
  overscan?: number;

  // -----------------------------
  // Column Resize / Drag
  // -----------------------------
  resizableColumns?: boolean;
  draggableColumns?: boolean;
  onColumnOrderChange?: (columns: ColumnDef<T>[]) => void;
  onColumnResize?: (columnId: string, newWidth: number) => void;

  // -----------------------------
  // Sticky
  // -----------------------------
  stickyHeader?: boolean;
  stickyColumns?: boolean;

  // -----------------------------
  // Theming
  // -----------------------------
  theme?: TableTheme;
  className?: string;
  dir?: Direction;

  // -----------------------------
  // Render Customization
  // -----------------------------
  renderEmptyState?: () => React.ReactNode;
  renderLoadingState?: () => React.ReactNode;
  loading?: boolean;

  // -----------------------------
  // Debug
  // -----------------------------
  debug?: boolean;
}

// -------------------------------------------
// Return type of useTable()
// -------------------------------------------

export interface UseTableReturn<T> {
  rows: T[];
  visibleColumns: ColumnDef<T>[];

  sortedRows: T[];
  filteredRows: T[];
  paginatedRows: T[];

  sortState: SortState | null;
  setSort: (sort: SortState | null) => void;

  filterState: FilterState[];
  setFilters: (filters: FilterState[]) => void;

  paginationState: PaginationState;
  setPagination: (state: PaginationState) => void;

  selectedIds: string[];
  toggleRowSelected: (id: string) => void;

  expandedIds: string[];
  toggleRowExpanded: (id: string) => void;

  columnOrder: string[];
  setColumnOrder: (ids: string[]) => void;

  columnSizes: Record<string, number>;
  setColumnSize: (id: string, size: number) => void;

  isVirtualized: boolean;
  theme: TableTheme;
}
