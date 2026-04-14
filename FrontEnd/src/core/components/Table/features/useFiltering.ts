// features/useFiltering.ts
import { useState, useCallback, useEffect, useMemo } from 'react';
import { FilterState, ColumnDef, TableProps } from '../Table.types';

interface UseFilteringParams<T> extends Pick<TableProps<T>, 'filterable' | 'defaultFilters' | 'onFiltersChange' | 'serverFiltering'> {
  controlledFilters?: FilterState[];
}

interface UseFilteringReturn<T> {
  filterState: FilterState[];
  setFilters: (newFilters: FilterState[]) => void;
  applyFilters: (data: T[], columns: ColumnDef<T>[]) => T[];
  isFiltered: boolean;
}

export function useFiltering<T>({
  filterable,
  controlledFilters,
  defaultFilters,
  onFiltersChange,
  serverFiltering,
}: UseFilteringParams<T>): UseFilteringReturn<T> {
  const [filters, setInternalFilters] = useState<FilterState[]>(() => {
    if (controlledFilters !== undefined) return controlledFilters;
    return defaultFilters ?? [];
  });

  // Effect to update internal state if controlledFilters prop changes
  useEffect(() => {
    if (controlledFilters !== undefined) {
      setInternalFilters(controlledFilters);
    }
  }, [controlledFilters]);

  const setFilters = useCallback((newFilters: FilterState[]) => {
    if (!serverFiltering) {
      setInternalFilters(newFilters);
    }
    onFiltersChange?.(newFilters);
  }, [serverFiltering, onFiltersChange]);

  const applyFilters = useCallback((data: T[], columns: ColumnDef<T>[]): T[] => {
    if (!filterable || filters.length === 0 || serverFiltering) {
      return data;
    }

    return data.filter((row) => {
      return filters.every((filter) => {
        const column = columns.find((col) => col.id === filter.columnId);
        if (!column || !column.accessor) {
          console.warn(`Filtering column "${filter.columnId}" not found or has no accessor.`);
          return true; // Don't filter if column is invalid
        }

        const cellValue = column.accessor(row);
        const filterValue = filter.value;

        // Basic filter logic (can be extended with operators like 'contains', 'equals', 'startsWith', etc.)
        // Assuming filter.value is a string for basic includes check
        if (filterValue === undefined || filterValue === null || filterValue === '') {
          return true; // No filter value, so it passes
        }
        if (cellValue === undefined || cellValue === null) {
            return false; // Row value is null/undefined, doesn't match non-empty filter
        }

        // Convert both to string for simple comparison. Enhance this for different data types.
        return String(cellValue).toLowerCase().includes(String(filterValue).toLowerCase());
      });
    });
  }, [filters, filterable, serverFiltering]); // Include columns if accessor changes

  const isFiltered = useMemo(() => filters.length > 0, [filters]);

  return {
    filterState: filters,
    setFilters,
    applyFilters,
    isFiltered,
  };
}
