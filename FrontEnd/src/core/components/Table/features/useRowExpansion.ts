// features/useRowExpansion.ts
// import { useState, useCallback, useEffect, useMemo } from 'react';
// import { TableProps } from '../Table.types';

// type ExpandedRowId = string | number;

// interface UseRowExpansionParams<T> extends Pick<TableProps<T>, 'expandableRows' | 'renderExpandedRow' | 'onExpandedRowsChange'> {
//   controlledExpandedRows?: ExpandedRowId[] | null;
//   defaultExpandedRows?: ExpandedRowId[];
//   getRowId: (row: T) => ExpandedRowId;
// }

// interface UseRowExpansionReturn<T> {
//   expandedIds: Set<ExpandedRowId>;
//   toggleRowExpansion: (row: T, event?: React.MouseEvent) => void;
//   isRowExpanded: (row: T) => boolean;
//   expandAllRows: (rows: T[]) => void;
//   collapseAllRows: () => void;
//   isExpandAllButtonVisible: (rows: T[]) => boolean;
//   isAllRowsExpanded: (rows: T[]) => boolean;
// }

// export function useRowExpansion<T>({
//   expandableRows,
//   controlledExpandedRows,
//   defaultExpandedRows,
//   onExpandedRowsChange,
//   getRowId,
// }: UseRowExpansionParams<T>): UseRowExpansionReturn<T> {

//   const [expandedInternal, setExpandedInternal] = useState<Set<ExpandedRowId>>(() => {
//     if (controlledExpandedRows !== undefined) return new Set(controlledExpandedRows);
//     return new Set(defaultExpandedRows ?? []);
//   });

//   // Effect to sync with controlled prop
//   useEffect(() => {
//     if (controlledExpandedRows !== undefined) {
//       setExpandedInternal(new Set(controlledExpandedRows));
//     }
//   }, [controlledExpandedRows]);

//   const setExpanded = useCallback((newExpandedIds: Set<ExpandedRowId>) => {
//       if (controlledExpandedRows === undefined) { // Only update internal state if not controlled
//           setExpandedInternal(newExpandedIds);
//       }
//       onExpandedRowsChange?.(Array.from(newExpandedIds));
//   }, [controlledExpandedRows, onExpandedRowsChange]);

//   const toggleRowExpansion = useCallback((row: T, event?: React.MouseEvent) => {
//     event?.stopPropagation(); // Prevent other actions like sorting or selection
//     if (!expandableRows) return;

//     const rowId = getRowId(row);
//     const newExpanded = new Set(expandedInternal);

//     if (newExpanded.has(rowId)) {
//       newExpanded.delete(rowId);
//     } else {
//       newExpanded.add(rowId);
//     }
//     setExpanded(newExpanded);
//   }, [expandedInternal, expandableRows, setExpanded, getRowId]);

//   const isRowExpanded = useCallback((row: T): boolean => {
//     return expandedInternal.has(getRowId(row));
//   }, [expandedInternal, getRowId]);

//   const expandAllRows = useCallback((rows: T[]) => {
//     const newExpanded = new Set(expandedInternal);
//     rows.forEach(row => newExpanded.add(getRowId(row)));
//     setExpanded(newExpanded);
//   }, [expandedInternal, setExpanded, getRowId]);

//   const collapseAllRows = useCallback(() => {
//     setExpanded(new Set());
//   }, [setExpanded]);

//   const isAllRowsExpanded = useCallback((rows: T[]): boolean => {
//     if (rows.length === 0) return false;
//     return rows.every(row => expandedInternal.has(getRowId(row)));
//   }, [expandedInternal, getRowId]);

//   // Determine if the "Expand All" button should be visible (e.g., if rows exist and not all are expanded)
//   const isExpandAllButtonVisible = useCallback((rows: T[]): boolean => !isAllRowsExpanded(rows) && rows.length > 0, [isAllRowsExpanded]);

//   return {
//     expandedIds: expandedInternal,
//     toggleRowExpansion,
//     isRowExpanded,
//     expandAllRows,
//     collapseAllRows,
//     isExpandAllButtonVisible,
//     isAllRowsExpanded,
//   };
// }

