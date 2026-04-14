// src/core/components/Table/useTable.ts
// src/core/components/Table/useTable.ts
import { useState, useMemo, useCallback } from "react";
import { UseTableProps, UseTableReturn, SortState } from "./Table.types"; // Assuming SortState is the correct type for sorting

export function useTable<T>({
  data,
  columns,
  pageSize = 10,
  defaultPage = 1,
  controlledPage,
  onPageChange,

  defaultSort,
  sort: controlledSort,
  onSortChange,

  selectable = false,
  cascadeSelection = false,
  selected: controlledSelected,
  defaultSelected = [],
  onSelectionChange,

  getRowId = (row: any) => row.id,
}: UseTableProps<T>): UseTableReturn<T> {
  // ---------- Sorting ----------
  const isSortControlled = controlledSort !== undefined;
  // Initialize internalSort with a type that matches SortState or null
  const [internalSort, setInternalSort] = useState<SortState | null>(defaultSort || null);

  // Ensure sortState has the correct type
  const sortState = isSortControlled ? controlledSort : internalSort;

  const sortedData = useMemo(() => {
    if (!sortState) return data;
    const col = columns.find(c => c.id === sortState.columnId);
    if (!col) return data;

    // Ensure accessor is correctly typed or handled
    const accessor = col.accessor || ((row: T) => row[col.id as keyof T]); // Use keyof T for better type safety

    return [...data].sort((a: T, b: T) => { // Explicitly type a and b
      const va = accessor(a);
      const vb = accessor(b);
      
      // Handle potential null/undefined values for robust sorting
      if (va == null && vb == null) return 0;
      if (va == null) return sortState.direction === "asc" ? -1 : 1;
      if (vb == null) return sortState.direction === "asc" ? 1 : -1;

      if (va < vb) return sortState.direction === "asc" ? -1 : 1;
      if (va > vb) return sortState.direction === "asc" ? 1 : -1;
      return 0;
    });
  }, [data, sortState, columns]);

  const toggleSort = useCallback(
    (columnId: string) => {
      // Define the type for the next sort state explicitly
      let nextDirection: "asc" | "desc";
      if (sortState?.columnId === columnId) {
        nextDirection = sortState.direction === "asc" ? "desc" : "asc";
      } else {
        nextDirection = "asc";
      }
      
      const next: SortState = { columnId, direction: nextDirection };

      if (isSortControlled) {
        // Type safety for onSortChange
        onSortChange?.(next);
      } else {
        // Type safety for setInternalSort
        setInternalSort(next);
      }
    },
    [sortState, isSortControlled, onSortChange] // Include sortState in dependencies
  );

  // ---------- Pagination ----------
  const isPageControlled = controlledPage !== undefined;
  const [internalPage, setInternalPage] = useState(defaultPage);

  const page = isPageControlled ? controlledPage : internalPage;
  const totalPages = Math.ceil(sortedData.length / pageSize);

  const paginatedData = useMemo(() => {
    const start = (page - 1) * pageSize;
    return sortedData.slice(start, start + pageSize);
  }, [page, pageSize, sortedData]);

  const setPage = useCallback(
    (p: number) => {
      const valid = Math.min(Math.max(1, p), totalPages);
      if (isPageControlled) onPageChange?.(valid);
      else setInternalPage(valid);
    },
    [isPageControlled, totalPages, onPageChange]
  );

  // ---------- Selection ----------
  const isSelectionControlled = controlledSelected !== undefined;
  const [internalSelected, setInternalSelected] = useState<Set<string>>(
    new Set(defaultSelected)
  );

  const selectedSet = isSelectionControlled
    ? new Set(controlledSelected)
    : internalSelected;

  const updateSelection = useCallback(
    (newSet: Set<string>) => {
      if (!isSelectionControlled) setInternalSelected(newSet);
      // Ensure onSelectionChange receives correct types
      onSelectionChange?.(
        Array.from(newSet),
        data.filter(row => newSet.has(getRowId(row)))
      );
    },
    [isSelectionControlled, onSelectionChange, data, getRowId]
  );

  const toggleSelect = useCallback(
    (row: T) => {
      if (!selectable) return;
      const id = getRowId(row);
      const newSet = new Set(selectedSet);

      if (newSet.has(id)) newSet.delete(id);
      else newSet.add(id);

      updateSelection(newSet);
    },
    [selectedSet, selectable, getRowId, updateSelection]
  );

  const selectAllPage = useCallback(() => {
    if (!selectable) return;
    const newSet = new Set(selectedSet);

    paginatedData.forEach(row => newSet.add(getRowId(row)));
    updateSelection(newSet);
  }, [paginatedData, selectable, updateSelection, getRowId, selectedSet]);

  const clearSelection = useCallback(() => {
    updateSelection(new Set());
  }, [updateSelection]);

  const isSelected = useCallback(
    (row: T) => selectedSet.has(getRowId(row)),
    [selectedSet, getRowId]
  );

  return {
    paginatedData,
    totalPages,

    sort: sortState,
    toggleSort,

    selectedSet,
    toggleSelect,
    isSelected,
    selectAllPage,
    clearSelection,

    page,
    setPage,

    getRowId,
  };
}
