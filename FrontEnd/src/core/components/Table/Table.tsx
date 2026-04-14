// src/core/components/Table/Table.tsx
import React, { useMemo, useCallback } from 'react';
import { ColumnDef, TableProps, Row, CellProps, TableState, TableMeta } from './Table.types';
import { useSorting } from './features/useSorting';
import { useFiltering } from './features/useFiltering';
import { usePagination } from './features/usePagination';
import { useSelection } from './features/useSelection';
import { useRowExpansion } from './features/useRowExpansion';
import { useVirtualization } from './features/useVirtualization';
import { useColumnManagement } from './features/useColumnManagement';
// import { useGlobalFilter } from './features/useGlobalFilter'; // Example of another potential hook

// Define the main Table component
export function Table<T>(props: TableProps<T>) {
  const {
    data,
    columns,
    getRowId = (row) => (row as any).id, // Default getRowId function
    initialState,
    state, // Controlled state
    onStateChange,
    // Sorting props
    manualSorting,
    sortable,
    // Filtering props
    manualFiltering,
    // Pagination props
    manualPagination,
    pageSize: controlledPageSize,
    pageCount: controlledPageCount,
    // Selection props
    selectionMode,
    getRowCanSelect,
    // Row Expansion props
    getRowCanExpand,
    // Virtualization props
    virtualizationOptions,
    getRowHeight,
    // Column Management props
    defaultColumnState,
    onColumnOrderChange,
    onColumnWidthsChange,
    onColumnVisibilityChange,
    controlledColumnOrder,
    controlledColumnWidths,
    controlledColumnVisibility,
    // other props like tableClassName, etc. can be added
  } = props;

  // --- State Management with Hooks ---

  // Initialize table state - combine initial state and controlled state
  const tableState: TableState<T> = useMemo(() => ({
    sorting: state?.sorting ?? initialSorting,
    filters: state?.filters ?? initialFilters,
    pagination: state?.pagination ?? initialPagination,
    selection: state?.selection ?? initialSelection,
    rowExpansion: state?.rowExpansion ?? initialRowExpansion,
    columnOrder: controlledColumnOrder ?? defaultColumnState?.order ?? columns.map(col => col.id),
    columnWidths: controlledColumnWidths ?? defaultColumnState?.widths ?? {},
    columnVisibility: controlledColumnVisibility ?? defaultColumnState?.visibility ?? {},
    // Add other states here (e.g., global filter)
  }), [state, initialState, columns, controlledColumnOrder, defaultColumnState, controlledColumnWidths, controlledColumnVisibility]);

  // Sorting Hook
  const sortingHook = useSorting<T>({
    data,
    columns,
    isServerSort: manualSorting,
    sortable: sortable ?? true, // Default to sortable
    initialSorting: tableState.sorting,
    // Pass controlled sorting state if provided
    ...(state?.sorting !== undefined && { controlledSorting: state.sorting }),
    onSortingChange: (newSorting) => handleStateChange({ sorting: newSorting }),
  });

  // Filtering Hook
  const filteringHook = useFiltering<T>({
    data,
    isServerFilter: manualFiltering,
    initialFilters: tableState.filters,
    // Pass controlled filters state if provided
    ...(state?.filters !== undefined && { controlledFilters: state.filters }),
    onFiltersChange: (newFilters, isFiltered) => handleStateChange({ filters: newFilters, isFiltered }),
  });

  // Pagination Hook
  const paginationHook = usePagination<T>({
    data,
    isServerPagination: manualPagination,
    initialPageSize: controlledPageSize ?? tableState.pagination.pageSize,
    initialPageCount: controlledPageCount ?? Math.ceil(data.length / (controlledPageSize ?? tableState.pagination.pageSize)),
    initialPageIndex: tableState.pagination.pageIndex,
    // Pass controlled pagination state if provided
    ...(state?.pagination !== undefined && { controlledPagination: state.pagination }),
    onPaginationChange: (newPagination) => handleStateChange({ pagination: newPagination }),
  });

  // Selection Hook
  const selectionHook = useSelection<T>({
    data,
    getRowId,
    selectionMode,
    getRowCanSelect,
    initialSelectedRowIds: tableState.selection.selectedRowIds,
    initialIsAllRowsSelected: tableState.selection.isAllRowsSelected,
    // Pass controlled selection state if provided
     ...(state?.selection !== undefined && { controlledSelection: state.selection }),
    onSelectionChange: (newSelection) => handleStateChange({ selection: newSelection }),
  });

  // Row Expansion Hook
  const rowExpansionHook = useRowExpansion<T>({
    data,
    getRowId,
    getRowCanExpand,
    initialExpandedRowIds: tableState.rowExpansion.expandedRowIds,
    initialIsAllRowsExpanded: tableState.rowExpansion.isAllRowsExpanded,
    // Pass controlled expansion state if provided
    ...(state?.rowExpansion !== undefined && { controlledRowExpansion: state.rowExpansion }),
    onRowExpansionChange: (newExpansion) => handleStateChange({ rowExpansion: newExpansion }),
  });

   // Column Management Hook
   const columnManagementHook = useColumnManagement<T>({
      columns,
      defaultColumnState: defaultColumnState,
      controlledColumnOrder: tableState.columnOrder,
      controlledColumnWidths: tableState.columnWidths,
      controlledColumnVisibility: tableState.columnVisibility,
      onColumnOrderChange: (newOrder) => handleStateChange({ columnOrder: newOrder }),
      onColumnWidthsChange: (newWidths) => handleStateChange({ columnWidths: newWidths }),
      onColumnVisibilityChange: (newVisibility) => handleStateChange({ columnVisibility: newVisibility }),
   });

   // Virtualization Hook
   const virtualizationHook = useVirtualization<T>({
      data,
      columns,
      getRowHeight,
      virtualizationOptions,
   });

  // --- Data Processing Pipeline ---

  // 1. Apply Sorting
  const sortedData = sortingHook.applySort(data);

  // 2. Apply Filtering
  const filteredData = filteringHook.applyFilters(sortedData);

  // 3. Apply Row Expansion (affects data structure for pagination/display)
  // This is a simplified application. Real implementation might need to consider expansion state.
  const expandedData = filteredData; // Placeholder: logic for expanding rows would go here if needed before pagination

  // 4. Apply Pagination
  const pagedData = paginationHook.applyPagination(expandedData);

  // --- Prepare Data for Rendering ---

  // Combine all processed data and states into a meta object
  const tableMeta: TableMeta<T> = {
    // States
    sorting: sortingHook.sortingState,
    filters: filteringHook.filtersState,
    pagination: paginationHook.paginationState,
    selection: selectionHook.selectionState,
    rowExpansion: rowExpansionHook.rowExpansionState,
    columnOrder: columnManagementHook.columnOrder,
    columnWidths: columnManagementHook.columnWidths,
    columnVisibility: columnManagementHook.columnVisibility,
    // Flags and helpers
    isFiltered: filteringHook.isFiltered,
    isSorted: sortingHook.isSorted,
    pageCount: paginationHook.pageCount,
    getVisibleColumns: columnManagementHook.getVisibleColumns,
    isColumnVisible: columnManagementHook.isColumnVisible,
    // Selection helpers
    isRowSelected: selectionHook.isRowSelected,
    isSelectAllChecked: selectionHook.isSelectAllChecked,
    // Row Expansion helpers
    isRowExpanded: rowExpansionHook.isRowExpanded,
    isAllRowsExpanded: rowExpansionHook.isAllRowsExpanded,
    // Virtualization
    listProps: virtualizationHook.listProps,
    listRef: virtualizationHook.listRef,
    // Callbacks for actions
    toggleSortBy: sortingHook.toggleSortBy,
    setFilter: filteringHook.setFilter,
    setPageIndex: paginationHook.setPageIndex,
    setPageSize: paginationHook.setPageSize,
    toggleRowSelected: selectionHook.toggleRowSelected,
    selectAllRows: selectionHook.selectAllRows,
    deselectAllRows: selectionHook.deselectAllRows,
    toggleRowExpansion: rowExpansionHook.toggleRowExpansion,
    expandAllRows: rowExpansionHook.expandAllRows,
    collapseAllRows: rowExpansionHook.collapseAllRows,
    setColumnOrder: columnManagementHook.setColumnOrder,
    setColumnWidths: columnManagementHook.setColumnWidths,
    setColumnVisibility: columnManagementHook.setColumnVisibility,
    // Resize/Reorder handlers
    handleColumnResize: columnManagementHook.handleColumnResize,
    handleColumnReorder: columnManagementHook.handleColumnReorder,
  };

  // Handler for updating the overall table state
  const handleStateChange = useCallback((newState: Partial<TableState<T>>) => {
    // Merge new state with existing state
    const updatedState = { ...tableMeta, ...newState }; // This needs refinement to merge properly with controlled state

    // If state is controlled, only call the callback
    if (state !== undefined) {
      onStateChange?.(updatedState as TableState<T>); // Cast needed as we are merging
    } else {
      // If uncontrolled, update the internal state (This requires internal state management, e.g., using useReducer)
      // For now, we'll assume onStateChange handles updates for both controlled/uncontrolled via the meta object
      // A more robust implementation would use a centralized useReducer.
      console.warn("Uncontrolled table state update logic needs implementation (e.g., using useReducer)");
    }
  }, [onStateChange, state, tableMeta]); // Dependencies need careful review

  // Memoize the rows to be rendered
  const rows: Row<T>[] = useMemo(() => {
    const visibleColumns = tableMeta.getVisibleColumns();
    const visibleColumnIds = visibleColumns.map(col => col.id);

    return pagedData.map((rowData, index) => {
      const rowId = getRowId(rowData);
      const cells: React.ReactNode[] = visibleColumns.map((column) => {
        const cellProps: CellProps<T> = {
          column,
          row: { original: rowData, id: rowId, index },
          value: column.accessor ? column.accessor(rowData) : null,
          meta: tableMeta, // Pass meta object down to cells
        };

        // Render cell content using column's cell renderer or default
        const CellContent = column.Cell ? column.Cell : ({ value }) => <span>{value}</span>;

        return (
          <div key={column.id} className={`td ${column.className || ''}`} style={{ width: tableMeta.columnWidths[column.id] || column.width || 'auto' }}>
            <CellContent {...cellProps} />
          </div>
        );
      });

      return {
        id: rowId,
        original: rowData,
        index: index, // Index within the current page/dataset slice
        cells: cells,
        isExpanded: tableMeta.isRowExpanded(rowId),
        isSelected: tableMeta.isRowSelected(rowId),
      };
    });
  }, [pagedData, getRowId, tableMeta, getVisibleColumns]); // Dependencies

  // --- Rendering Logic ---

  // Determine if virtualization should be used
  const isVirtualizationEnabled = !!virtualizationOptions && !!virtualizationHook.listProps;

  return (
    <div className="table-container">
      {/* Header Row */}
      <div className="thead">
        <div className="tr">
          {tableMeta.getVisibleColumns().map((column) => (
             <div
                key={column.id}
                className={`th ${column.sortable === false ? '' : 'sortable'} ${column.className || ''}`}
                style={{ width: tableMeta.columnWidths[column.id] || column.width || 'auto' }}
                onClick={() => column.sortable !== false && tableMeta.toggleSortBy(column.id)}
              >
                {column.header ? column.header() : column.headerText}
                {/* Sorting indicator */}
                {tableMeta.sorting.columnId === column.id && (
                    <span>{tableMeta.sorting.direction === 'asc' ? ' ▲' : ' ▼'}</span>
                )}
             </div>
          ))}
        </div>
      </div>

      {/* Body Rows */}
      <div className="tbody" {...(isVirtualizationEnabled ? virtualizationHook.listProps : {})}>
        {isVirtualizationEnabled ? (
          // Render using react-window if virtualization is enabled
          // The children prop of listProps already handles rendering rows
          <></> // The actual list component is rendered by react-window via listProps.children
        ) : (
          // Render normally if virtualization is disabled
          rows.map((row) => (
             <div key={row.id} className={`tr ${row.isExpanded ? 'expanded' : ''} ${row.isSelected ? 'selected' : ''}`}>
               {row.cells}
             </div>
          ))
        )}
      </div>

      {/* Footer/Controls (Pagination, Selection Summary, etc.) */}
      <div className="tfoot">
        {/* Example: Pagination Controls */}
        <div>
          <button onClick={() => tableMeta.setPageIndex(0)} disabled={tableMeta.pagination.pageIndex === 0}>{'<<'}</button>
          <button onClick={() => tableMeta.setPageIndex(tableMeta.pagination.pageIndex - 1)} disabled={tableMeta.pagination.pageIndex === 0}>{'<'}</button>
          <span>Page {tableMeta.pagination.pageIndex + 1} of {tableMeta.pageCount}</span>
          <button onClick={() => tableMeta.setPageIndex(tableMeta.pagination.pageIndex + 1)} disabled={tableMeta.pagination.pageIndex >= tableMeta.pageCount - 1}> {'>'} </button>
          <button onClick={() => tableMeta.setPageIndex(tableMeta.pageCount - 1)} disabled={tableMeta.pagination.pageIndex >= tableMeta.pageCount - 1}> {'>>'} </button>
          <select value={tableMeta.pagination.pageSize} onChange={(e) => tableMeta.setPageSize(Number(e.target.value))}>
            {[10, 20, 30, 50].map(size => <option key={size} value={size}>{size} per page</option>)}
          </select>
        </div>

        {/* Example: Selection Summary */}
        {selectionHook.selectionState.selectedRowIds.length > 0 && (
           <div>
              Selected {selectionHook.selectionState.selectedRowIds.length} rows.
              {selectionHook.selectionState.isAllRowsSelected && ` (All ${data.length} rows selected)`}
           </div>
        )}
      </div>
    </div>
  );
}

// --- Helper Functions and Default Components ---

// Default initial states (can be overridden by props)
const initialSorting = { columnId: undefined, direction: 'asc' as const };
const initialFilters = {};
const initialPagination = { pageIndex: 0, pageSize: 10 };
const initialSelection = { selectedRowIds: [], isAllRowsSelected: false };
const initialRowExpansion = { expandedRowIds: [], isAllRowsExpanded: false };

// Placeholder for default Cell renderer if not provided by column
// const DefaultCell: React.FC<CellProps<any>> = ({ value }) => <span>{value}</span>;

// Note: The `handleStateChange` function needs a more robust implementation, likely involving a `useReducer`
// at the top level of the Table component or within a custom hook (`useTableState`) to manage all states centrally,
// especially for the uncontrolled mode. The current implementation passes state updates through `tableMeta`
// which needs to be used correctly by the parent component or managed internally.

