// src/core/components/Table/features/useSorting.ts
import { useState, useMemo, useCallback } from 'react';
import { SortingProps, SortingState, ColumnDef } from '../Table.types';

export function useSorting<T>(props: SortingHookProps<T>) {
  const {
    data,
    columns,
    isServerSort,
    sortable = true,
    initialSorting,
    controlledSorting,
    onSortingChange,
  } = props;

  const [internalSorting, setInternalSorting] = useState<SortingState>(initialSorting ?? { columnId: undefined, direction: 'asc' });

  // Determine the active sorting state (controlled or internal)
  const sortingState = controlledSorting !== undefined ? controlledSorting : internalSorting;

  const toggleSortBy = useCallback((columnId: string) => {
    if (!sortable) return;

    let newDirection: 'asc' | 'desc' = 'asc';
    // If already sorting by this column, toggle direction
    if (sortingState.columnId === columnId) {
      newDirection = sortingState.direction === 'asc' ? 'desc' : 'asc';
    }

    const newSorting: SortingState = { columnId, direction: newDirection };

    // If server-side sorting, just update the state and let the parent handle data fetching
    if (isServerSort) {
      onSortingChange?.(newSorting);
      // If controlled, the parent will update via controlledSorting prop
      // If uncontrolled, we update internal state
      if (controlledSorting === undefined) {
        setInternalSorting(newSorting);
      }
    } else {
      // Client-side sorting: update state and trigger sort
      if (controlledSorting === undefined) {
        setInternalSorting(newSorting);
      } else {
        onSortingChange?.(newSorting); // Call parent for controlled component
      }
      // The actual sorting happens in the applySort function below
    }
  }, [sortable, sortingState, isServerSort, onSortingChange, controlledSorting]);

  const applySort = useCallback((inputData: T[]) => {
    // If no sorting is applied or it's server-side, return original data
    if (!sortingState.columnId || isServerSort) {
      return inputData;
    }

    const { columnId, direction } = sortingState;

    // Find the column definition for the current sorting column
    const sortedColumn = columns.find((col) => col.id === columnId);

    // --- FIX START ---
    // Check if sortedColumn and its accessor exist before using them
    if (!sortedColumn || !sortedColumn.accessor) {
      console.warn(`Column with id "${columnId}" not found or does not have an accessor. Cannot sort.`);
      return inputData; // Return original data if column or accessor is missing
    }
    // --- FIX END ---

    // Perform client-side sorting
    return [...inputData].sort((a, b) => {
      let valueA: any;
      let valueB: any;

      // Safely get values using the accessor
      try {
        valueA = sortedColumn.accessor(a);
        valueB = sortedColumn.accessor(b);
      } catch (error) {
        console.error(`Error accessing sort values for column "${columnId}":`, error);
        return 0; // Return neutral comparison if accessor fails
      }


      // Handle cases where values might be null or undefined
      if (valueA === null || valueA === undefined) return direction === 'asc' ? -1 : 1;
      if (valueB === null || valueB === undefined) return direction === 'asc' ? 1 : -1;

      // Standard comparison
      if (valueA < valueB) {
        return direction === 'asc' ? -1 : 1;
      }
      if (valueA > valueB) {
        return direction === 'asc' ? 1 : -1;
      }
      return 0; // Values are equal
    });
  }, [sortingState, columns, isServerSort]); // Removed onSortingChange from dependency array as it's a stable function

  return {
    sortingState,
    toggleSortBy,
    applySort,
    isSorted: !!sortingState.columnId, // Add helper to indicate if sorting is active
  };
}
