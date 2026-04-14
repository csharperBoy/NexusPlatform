// src/core/components/Table/useTable.ts
import React, { useState, useMemo, useCallback, useEffect } from 'react';
import { VariableSizeList } from 'react-window'; // For virtualization

import {
  ColumnDef,
  SortState,
  FilterState,
  PaginationState,
  SelectionMode,
  TableProps,
  UseTableReturn,
  TableTheme,
} from './Table.types';

// --- Feature Hooks ---
// (اینها در فایل‌های جداگانه تعریف خواهند شد)
import { useSorting } from './features/useSorting';
import { useFiltering } from './features/useFiltering';
import { usePagination } from './features/usePagination';
import { useSelection } from './features/useSelection';
import { useRowExpansion } from './features/useRowExpansion';
import { useColumnManagement } from './features/useColumnManagement'; // For resize & drag
import { useVirtualization } from './features/useVirtualization';
// ... other feature hooks

// --- Default Values ---
const DEFAULT_ROW_HEIGHT = 40;
const DEFAULT_THEME: TableTheme = {
  density: 'normal',
  // Add more default theme values
};

// --- Main useTable Hook ---
export function useTable<T>(props: TableProps<T>): UseTableReturn<T> {
  const {
    data,
    columns,
    getRowId,

    // Sorting props
    sortable,
    sort: controlledSort,
    defaultSort,
    onSortChange,
    serverSort,

    // Filtering props
    filterable,
    filters: controlledFilters,
    defaultFilters,
    onFiltersChange,
    serverFiltering,

    // Pagination props
    pagination,
    paginationState: controlledPagination,
    defaultPaginationState,
    onPaginationChange,
    serverPagination,

    // Selection props
    selectionMode = 'none',
    cascadeSelection,
    selected: controlledSelected,
    defaultSelected,
    onSelectedChange,

    // Row Expansion props
    expandableRows,
    expandedRows: controlledExpandedRows,
    defaultExpandedRows,
    onExpandedRowsChange,
    renderExpandedRow,

    // Virtualization props
    virtualized,
    rowHeight = DEFAULT_ROW_HEIGHT,
    overscan = 5,

    // Column Management props
    resizableColumns,
    draggableColumns,
    onColumnOrderChange,
    onColumnResize,
    onColumnDrag, // Added for drag callback

    // Sticky props
    stickyHeader,
    stickyColumns,

    // Theming props
    theme: customTheme,
    className,
    dir = 'ltr',

    // Loading/Empty state props
    loading,
    renderEmptyState,
    renderLoadingState,
  } = props;

  // --- State Management ---
  const getRowIdMemo = useCallback((row: T) => getRowId?.(row) ?? (row as any).id, [getRowId]);

  // Controlled/Uncontrolled State Management for Sorting, Filtering, Pagination, Selection, Expansion
  const { sortState, setSort } = useSorting<T>(controlledSort, defaultSort, onSortChange, serverSort);
  const { filterState, setFilters } = useFiltering<T>(controlledFilters, defaultFilters, onFiltersChange, serverFiltering);
  const { paginationState, setPagination } = usePagination(controlledPagination, defaultPaginationState, onPaginationChange, serverPagination);
  const { selectedIds, toggleRowSelected } = useSelection(controlledSelected, defaultSelected, onSelectedChange, selectionMode, cascadeSelection, getRowIdMemo);
  const { expandedIds, toggleRowExpanded } = useRowExpansion(controlledExpandedRows, defaultExpandedRows, onExpandedRowsChange, getRowIdMemo);

  // Column Management (Order, Size)
  const {
    orderedColumns,
    columnSizes,
    setColumnOrder,
    setColumnSize,
    registerColumnSize,
    // Dragging state might be handled within useColumnManagement
  } = useColumnManagement(columns, resizableColumns, draggableColumns, onColumnResize, onColumnOrderChange);

  // Memoize processed data
  const processedData = useMemo(() => {
    // Apply Server-side logic first if enabled
    let currentData = data;

    if (serverSort && sortState) {
      // Should ideally be handled by API call, but for client-side simulation:
      console.warn('Server-side sorting is enabled. Ensure data is pre-sorted by the server.');
    }
    if (serverFiltering && filterState.length > 0) {
      console.warn('Server-side filtering is enabled. Ensure data is pre-filtered by the server.');
    }
    if (serverPagination && (paginationState.page !== 0 || paginationState.pageSize !== 10)) {
      console.warn('Server-side pagination is enabled. Ensure data is pre-paginated by the server.');
    }

    // Client-side sorting
    let sortedData = currentData;
    if (sortable && sortState && !serverSort) {
      sortedData = [...currentData].sort((a, b) => {
         // Implement sorting logic based on ColumnDef accessor and sortState
         // This is a simplified placeholder
         const col = orderedColumns.find(c => c.id === sortState.columnId);
         if (!col || !col.accessor) return 0;
         const valA = col.accessor(a);
         const valB = col.accessor(b);
         if (valA < valB) return sortState.direction === 'asc' ? -1 : 1;
         if (valA > valB) return sortState.direction === 'asc' ? 1 : -1;
         return 0;
      });
    }

    // Client-side filtering
    let filteredData = sortedData;
    if (filterable && filterState.length > 0 && !serverFiltering) {
       filteredData = sortedData.filter(row => {
         return filterState.every(filter => {
           const column = orderedColumns.find(c => c.id === filter.columnId);
           if (!column || !column.accessor) return true;
           const cellValue = column.accessor(row);
           // Basic filtering logic (needs enhancement for different types/operators)
           return String(cellValue).toLowerCase().includes(String(filter.value).toLowerCase());
         });
       });
    }

    // Client-side pagination
    let paginatedData = filteredData;
    if (pagination && !serverPagination) {
      const startIndex = paginationState.page * paginationState.pageSize;
      const endIndex = startIndex + paginationState.pageSize;
      paginatedData = filteredData.slice(startIndex, endIndex);
    }

    return paginatedData; // This is the data that will be rendered
  }, [
    data,
    sortable, sortState, serverSort, // Sorting dependencies
    filterable, filterState, serverFiltering, // Filtering dependencies
    pagination, paginationState, serverPagination, // Pagination dependencies
    getRowIdMemo,
    orderedColumns,
    // Ensure any other logic changing data is included here
  ]);

  // --- Virtualization Logic ---
  const {
    virtualizedRows,
    virtualizedListRef,
    getItemSize,
    cellRenderer,
    rowRenderer,
  } = useVirtualization(
    processedData,
    orderedColumns,
    rowHeight,
    overscan,
    expandedIds, // Needed to calculate row height if expanded content varies
    renderExpandedRow,
    getRowIdMemo,
    theme, // Pass theme to calculate density adjustments if needed
    stickyHeader,
    stickyColumns
  );

  // Determine which data to render (virtualized or full)
  const renderableRows = virtualized ? virtualizedRows : processedData;

  // --- Column Definitions for Rendering ---
  const visibleColumns = useMemo(() => {
    // Filter out columns that might be hidden due to multi-level headers if needed
    // For now, just use the ordered columns
    return orderedColumns;
  }, [orderedColumns]);

  // --- Theme Merging ---
  const theme = useMemo(() => ({ ...DEFAULT_THEME, ...customTheme }), [customTheme]);

  // --- State for the component ---
  const tableState: UseTableReturn<T> = {
    rows: renderableRows, // Use virtualized rows if enabled
    visibleColumns: visibleColumns,

    // Pass down processed data and states
    sortedRows: processedData, // Full dataset after sorting (for external use)
    filteredRows: processedData, // Full dataset after filtering
    paginatedRows: processedData, // Full dataset after pagination

    sortState,
    setSort,

    filterState,
    setFilters,

    paginationState,
    setPagination,

    selectedIds,
    toggleRowSelected,

    expandedIds,
    toggleRowExpanded,

    columnOrder: orderedColumns.map(c => c.id), // Map to just IDs for simplicity
    setColumnOrder,

    columnSizes,
    setColumnSize: (id, size) => {
        setColumnSize(id, size);
        onColumnResize?.(id, size); // Callback for resize
    },

    isVirtualized: !!virtualized,
    theme: theme,

    // Add virtualization ref and item getter
    // These would be used by the Table.tsx component
    // virtualListRef: virtualizedListRef,
    // getItemSize: getItemSize,
    // cellRenderer: cellRenderer,
    // rowRenderer: rowRenderer,
  };

  return tableState;
}

// --- Placeholder for Feature Hooks (to be implemented) ---
// These would typically be in separate files like features/useSorting.ts

// Example for useSorting (simplified)
function useSorting<T>(
    controlledSort: SortState | null | undefined,
    defaultSort: SortState | null | undefined,
    onSortChange?: (sort: SortState | null) => void,
    serverSort?: boolean
) {
    const [sort, setInternalSort] = useState<SortState | null>(controlledSort ?? defaultSort ?? null);

    useEffect(() => {
        if (controlledSort !== undefined) {
            setInternalSort(controlledSort);
        }
    }, [controlledSort]);

    const handleSetSort = useCallback((newSort: SortState | null) => {
        if (!serverSort) {
            setInternalSort(newSort);
        }
        onSortChange?.(newSort);
    }, [serverSort, onSortChange]);

    return { sortState: sort, setSort: handleSetSort };
}

// ... Implement similar hooks for useFiltering, usePagination, etc.
// ... Implement useColumnManagement, useVirtualization, etc.
