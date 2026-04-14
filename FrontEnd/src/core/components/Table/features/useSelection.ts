// features/useSelection.ts
import { useState, useCallback, useEffect, useMemo } from 'react';
import { SelectionMode, TableProps } from '../Table.types';

type SelectedRowId<T> = string | number | T; // Can be ID, or the row object itself if getRowId is complex

interface UseSelectionParams<T> extends Pick<TableProps<T>, 'selectionMode' | 'cascadeSelection' | 'onSelectedChange'> {
  controlledSelected?: SelectedRowId<T>[] | null;
  defaultSelected?: SelectedRowId<T>[];
  getRowId: (row: T) => string | number;
}

interface UseSelectionReturn<T> {
  selectedIds: Set<string | number>;
  toggleRowSelected: (row: T, event?: React.MouseEvent) => void;
  isRowSelected: (row: T) => boolean;
  selectAllRows: (rows: T[]) => void;
  deselectAllRows: () => void;
  isSelectAllChecked: (rows: T[]) => boolean;
}

export function useSelection<T>({
  selectionMode = 'none',
  controlledSelected,
  defaultSelected,
  onSelectedChange,
  cascadeSelection, // For future implementation (e.g., selecting parent selects children)
  getRowId,
}: UseSelectionParams<T>): UseSelectionReturn<T> {

  const [selectedInternal, setInternalSelected] = useState<Set<string | number>>(() => {
    if (controlledSelected !== undefined) {
      return new Set(controlledSelected.map(item => typeof item === 'object' ? getRowId(item as T) : item));
    }
    return new Set(defaultSelected?.map(item => typeof item === 'object' ? getRowId(item as T) : item) ?? []);
  });

  // Effect to sync with controlled prop
  useEffect(() => {
    if (controlledSelected !== undefined) {
      setInternalSelected(new Set(controlledSelected.map(item => typeof item === 'object' ? getRowId(item as T) : item)));
    }
  }, [controlledSelected, getRowId]);

  const setSelected = useCallback((newSelectedIds: Set<string | number>) => {
      if (controlledSelected === undefined) { // Only update internal state if not controlled
          setInternalSelected(newSelectedIds);
      }
      onSelectedChange?.(Array.from(newSelectedIds).map(id => {
          // Try to return row objects if possible, otherwise just IDs
          // This part might need access to the full data set, which is complex here.
          // For now, just return IDs. Consider passing row objects back if feasible.
          return id;
      }));
  }, [controlledSelected, onSelectedChange, getRowId]); // Add getRowId if needed for mapping back

  const toggleRowSelected = useCallback((row: T, event?: React.MouseEvent) => {
    // Prevent row expansion/navigation if checkbox is clicked
    event?.stopPropagation();

    if (selectionMode === 'none') return;

    const rowId = getRowId(row);
    const newSelected = new Set(selectedInternal);

    if (newSelected.has(rowId)) {
      newSelected.delete(rowId);
    } else {
      newSelected.add(rowId);
    }
    setSelected(newSelected);
  }, [selectedInternal, selectionMode, setSelected, getRowId]);

  const isRowSelected = useCallback((row: T): boolean => {
    return selectedInternal.has(getRowId(row));
  }, [selectedInternal, getRowId]);

  const selectAllRows = useCallback((rows: T[]) => {
      const newSelected = new Set(selectedInternal);
      rows.forEach(row => newSelected.add(getRowId(row)));
      setSelected(newSelected);
  }, [selectedInternal, setSelected, getRowId]);

  const deselectAllRows = useCallback(() => {
      // Clear selection if mode allows, or if user explicitly deselects all
      setSelected(new Set());
  }, [setSelected]);

  const isSelectAllChecked = useCallback((rows: T[]): boolean => {
    if (rows.length === 0) return false;
    // Return true if all rows are currently selected
    return rows.every(row => selectedInternal.has(getRowId(row)));
  }, [selectedInternal, getRowId]);

  return {
    selectedIds: selectedInternal,
    toggleRowSelected,
    isRowSelected,
    selectAllRows,
    deselectAllRows,
    isSelectAllChecked,
  };
}
