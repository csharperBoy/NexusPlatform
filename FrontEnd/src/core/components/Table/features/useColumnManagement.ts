// features/useColumnManagement.ts
// import { useState, useCallback, useMemo } from 'react';
// import { ColumnDef, ColumnOrderState, ColumnVisibilityState, ColumnWidthsState } from '../Table.types';

// interface UseColumnManagementParams<T> {
//   columns: ColumnDef<T>[];
//   defaultColumnState?: {
//     order?: string[];
//     widths?: Record<string, number>;
//     visibility?: Record<string, boolean>;
//   };
//   onColumnOrderChange?: (newOrder: string[]) => void;
//   onColumnWidthsChange?: (newWidths: Record<string, number>) => void;
//   onColumnVisibilityChange?: (newVisibility: Record<string, boolean>) => void;
//   // Controlled props (optional)
//   controlledColumnOrder?: string[];
//   controlledColumnWidths?: Record<string, number>;
//   controlledColumnVisibility?: Record<string, boolean>;
// }

// interface UseColumnManagementReturn<T> {
//   columnOrder: string[];
//   columnWidths: Record<string, number>;
//   columnVisibility: Record<string, boolean>;
//   setColumnOrder: (newOrder: string[]) => void;
//   setColumnWidths: (newWidths: Record<string, number>) => void;
//   setColumnVisibility: (newVisibility: Record<string, boolean>) => void;
//   // Methods for interacting with columns
//   isColumnVisible: (columnId: string) => boolean;
//   getVisibleColumns: () => ColumnDef<T>[];
//   // For resizing logic (needs DOM interaction)
//   handleColumnResize: (columnId: string, newWidth: number) => void;
//   // For reordering logic (needs drag & drop)
//   handleColumnReorder: (draggedColumnId: string, targetIndex: number) => void;
// }

// export function useColumnManagement<T>({
//   columns,
//   defaultColumnState,
//   onColumnOrderChange,
//   onColumnWidthsChange,
//   onColumnVisibilityChange,
//   controlledColumnOrder,
//   controlledColumnWidths,
//   controlledColumnVisibility,
// }: UseColumnManagementParams<T>): UseColumnManagementReturn<T> {

//   // Initial states
//   const initialOrder = useMemo(() => controlledColumnOrder ?? defaultColumnState?.order ?? columns.map(col => col.id), [controlledColumnOrder, defaultColumnState?.order, columns]);
//   const initialWidths = useMemo(() => controlledColumnWidths ?? defaultColumnState?.widths ?? {}, [controlledColumnWidths, defaultColumnState?.widths]);
//   const initialVisibility = useMemo(() => controlledColumnVisibility ?? defaultColumnState?.visibility ?? {}, [controlledColumnVisibility, defaultColumnState?.visibility]);

//   // State management
//   const [columnOrder, setColumnOrderState] = useState<string[]>(initialOrder);
//   const [columnWidths, setColumnWidthsState] = useState<Record<string, number>>(initialWidths);
//   const [columnVisibility, setColumnVisibilityState] = useState<Record<string, boolean>>(initialVisibility);

//   // Handlers to update state and call callbacks
//   const setColumnOrder = useCallback((newOrder: string[]) => {
//     if (controlledColumnOrder === undefined) {
//       setColumnOrderState(newOrder);
//     }
//     onColumnOrderChange?.(newOrder);
//   }, [controlledColumnOrder, onColumnOrderChange]);

//   const setColumnWidths = useCallback((newWidths: Record<string, number>) => {
//     if (controlledColumnWidths === undefined) {
//       setColumnWidthsState(newWidths);
//     }
//     onColumnWidthsChange?.(newWidths);
//   }, [controlledColumnWidths, onColumnWidthsChange]);

//   const setColumnVisibility = useCallback((newVisibility: Record<string, boolean>) => {
//     if (controlledColumnVisibility === undefined) {
//       setColumnVisibilityState(newVisibility);
//     }
//     onColumnVisibilityChange?.(newVisibility);
//   }, [controlledColumnVisibility, onColumnVisibilityChange]);

//   // Helper to get current visibility, ensuring all columns have a defined state
//   const getCurrentVisibility = useCallback((cols: ColumnDef<T>[]): Record<string, boolean> => {
//       const current = controlledColumnVisibility ?? columnVisibility;
//       const visibilityMap: Record<string, boolean> = {};
//       cols.forEach(col => {
//           visibilityMap[col.id] = current[col.id] ?? true; // Default to visible if not specified
//       });
//       return visibilityMap;
//   }, [columnVisibility, controlledColumnVisibility]);


//   // Get visible columns based on current visibility state
//   const getVisibleColumns = useCallback((): ColumnDef<T>[] => {
//     const currentVisibility = getCurrentVisibility(columns);
//     return columns.filter(col => currentVisibility[col.id]);
//   }, [columns, getCurrentVisibility]);

//    // Get ordered visible columns
//    const orderedVisibleColumns = useMemo(() => {
//       const visibleCols = getVisibleColumns();
//       const currentOrder = controlledColumnOrder ?? columnOrder;
//       return visibleCols.sort((a, b) => currentOrder.indexOf(a.id) - currentOrder.indexOf(b.id));
//    }, [getVisibleColumns, columnOrder, controlledColumnOrder]);


//   // Check if a column is currently visible
//   const isColumnVisible = useCallback((columnId: string): boolean => {
//     const currentVisibility = getCurrentVisibility(columns);
//     return currentVisibility[columnId] ?? true; // Default to true
//   }, [columns, getCurrentVisibility]);

//   // Placeholder for resize handler - requires DOM interaction
//   const handleColumnResize = useCallback((columnId: string, newWidth: number) => {
//     // In a real implementation, this would update columnWidthsState
//     // and possibly trigger onColumnWidthsChange.
//     console.log(`Resizing column ${columnId} to ${newWidth}`);
//     // Example:
//     // setColumnWidths(prev => ({ ...prev, [columnId]: newWidth }));
//   }, []);

//   // Placeholder for reorder handler - requires drag & drop implementation
//   const handleColumnReorder = useCallback((draggedColumnId: string, targetIndex: number) => {
//     // In a real implementation, this would update columnOrderState
//     // and possibly trigger onColumnOrderChange.
//     console.log(`Reordering column ${draggedColumnId} to index ${targetIndex}`);
//     // Example:
//     // const newOrder = [...(controlledColumnOrder ?? columnOrder)];
//     // const oldIndex = newOrder.indexOf(draggedColumnId);
//     // if (oldIndex !== -1) {
//     //   newOrder.splice(oldIndex, 1);
//     //   newOrder.splice(targetIndex, 0, draggedColumnId);
//     //   setColumnOrder(newOrder);
//     // }
//   }, []);

//   return {
//     columnOrder: orderedVisibleColumns.map(col => col.id), // Return order of *visible* columns
//     columnWidths,
//     columnVisibility,
//     setColumnOrder,
//     setColumnWidths,
//     setColumnVisibility,
//     isColumnVisible,
//     getVisibleColumns: () => orderedVisibleColumns, // Return the ordered, visible columns
//     handleColumnResize,
//     handleColumnReorder,
//   };
// }
