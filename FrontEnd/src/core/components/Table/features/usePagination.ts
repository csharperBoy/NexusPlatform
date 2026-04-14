// features/usePagination.ts
import { useState, useCallback, useEffect, useMemo } from 'react';
import { PaginationState, TableProps } from '../Table.types';

interface UsePaginationParams extends Pick<TableProps<any>, 'pagination' | 'defaultPaginationState' | 'onPaginationChange' | 'serverPagination'> {
  controlledPaginationState?: PaginationState;
}

interface UsePaginationReturn {
  paginationState: PaginationState;
  setPagination: (updater: (prevState: PaginationState) => PaginationState) => void;
  applyPagination: <T>(data: T[]) => T[];
  pageCount: number;
}

const DEFAULT_PAGE_SIZE = 10;
const DEFAULT_PAGINATION_STATE: PaginationState = { page: 0, pageSize: DEFAULT_PAGE_SIZE };

export function usePagination({
  pagination,
  controlledPaginationState,
  defaultPaginationState,
  onPaginationChange,
  serverPagination,
}: UsePaginationParams): UsePaginationReturn {
  const [paginationState, setInternalPaginationState] = useState<PaginationState>(() => {
    if (controlledPaginationState !== undefined) return controlledPaginationState;
    return defaultPaginationState ?? DEFAULT_PAGINATION_STATE;
  });

  // Effect to update internal state if controlledPaginationState prop changes
  useEffect(() => {
    if (controlledPaginationState !== undefined) {
      setInternalPaginationState(controlledPaginationState);
    }
  }, [controlledPaginationState]);

  const setPagination = useCallback((updater: (prevState: PaginationState) => PaginationState) => {
    const newState = updater(paginationState); // Calculate new state based on previous internal state

    if (!serverPagination) {
      setInternalPaginationState(newState);
    }
    onPaginationChange?.(newState);
  }, [paginationState, serverPagination, onPaginationChange]); // Include paginationState in dependencies

  const applyPagination = useCallback(<T>(data: T[]): T[] => {
    if (!pagination || serverPagination) {
      return data;
    }
    const { page, pageSize } = paginationState;
    const startIndex = page * pageSize;
    const endIndex = startIndex + pageSize;
    return data.slice(startIndex, endIndex);
  }, [pagination, paginationState, serverPagination]);

  // Calculate page count based on data length (only for client-side)
  const pageCount = useMemo(() => {
      // This needs the *total* data length (before client-side filtering/sorting if applicable)
      // This calculation should happen OUTSIDE this hook, or be passed in.
      // For now, we'll assume it's calculated elsewhere and passed if needed.
      // Let's return a placeholder and refine this when Table.tsx is built.
      return 10; // Placeholder
  }, []); // Dependency would be total data count if passed

  return {
    paginationState: paginationState,
    setPagination: setPagination,
    applyPagination,
    pageCount, // This needs refinement based on total data
  };
}
