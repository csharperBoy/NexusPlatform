// src/core/components/Table/useTable.ts
import { useState, useMemo, useCallback } from 'react';
import { ColumnDef, TableProps } from './Table.types';

export function useTable<T>({
  data,
  columns,
  keyExtractor,
  selectable,
  selectedIds: externalSelectedIds,
  onSelectionChange,
  pageSize,
  currentPage: externalCurrentPage,
  onPageChange,
  sortColumn: externalSortColumn,
  sortDirection: externalSortDirection,
  onSortChange,
}: Pick<TableProps<T>, 'data' | 'columns' | 'keyExtractor' | 'selectable' | 'selectedIds' | 'onSelectionChange' | 'pageSize' | 'currentPage' | 'onPageChange' | 'sortColumn' | 'sortDirection' | 'onSortChange'>) {
  // ---------- Sorting ----------
  const [internalSortColumn, setInternalSortColumn] = useState<string | undefined>(externalSortColumn);
  const [internalSortDirection, setInternalSortDirection] = useState<'asc' | 'desc' | undefined>(externalSortDirection);
  const sortColumn = externalSortColumn !== undefined ? externalSortColumn : internalSortColumn;
  const sortDirection = externalSortDirection !== undefined ? externalSortDirection : internalSortDirection;

  const toggleSort = useCallback(
    (columnId: string) => {
      const col = columns.find(c => c.id === columnId);
      if (!col?.sortable) return;
      let newDirection: 'asc' | 'desc' = 'asc';
      if (sortColumn === columnId && sortDirection === 'asc') newDirection = 'desc';
      if (externalSortColumn !== undefined && onSortChange) {
        onSortChange(columnId, newDirection);
      } else {
        setInternalSortColumn(columnId);
        setInternalSortDirection(newDirection);
      }
    },
    [columns, sortColumn, sortDirection, externalSortColumn, onSortChange]
  );

  const sortedData = useMemo(() => {
    if (!sortColumn || !sortDirection) return data;
    const col = columns.find(c => c.id === sortColumn);
    if (!col) return data;
    const accessor = col.accessor || ((row: T) => (row as any)[col.id]);
    return [...data].sort((a, b) => {
      let aVal = accessor(a);
      let bVal = accessor(b);
      if (aVal == null && bVal == null) return 0;
      if (aVal == null) return sortDirection === 'asc' ? -1 : 1;
      if (bVal == null) return sortDirection === 'asc' ? 1 : -1;
      // اعداد
      if (typeof aVal === 'number' && typeof bVal === 'number') {
        return sortDirection === 'asc' ? aVal - bVal : bVal - aVal;
      }
      // رشته‌ها
      aVal = String(aVal);
      bVal = String(bVal);
      return sortDirection === 'asc' ? aVal.localeCompare(bVal) : bVal.localeCompare(aVal);
    });
  }, [data, sortColumn, sortDirection, columns]);

  // ---------- Pagination ----------
  const hasPagination = pageSize !== undefined && pageSize > 0;
  const [internalCurrentPage, setInternalCurrentPage] = useState(1);
  const currentPage = externalCurrentPage !== undefined ? externalCurrentPage : internalCurrentPage;
  const totalPages = hasPagination ? Math.ceil(sortedData.length / pageSize!) : 1;

  const paginatedData = useMemo(() => {
    if (!hasPagination) return sortedData;
    const start = (currentPage - 1) * pageSize!;
    return sortedData.slice(start, start + pageSize!);
  }, [sortedData, currentPage, pageSize, hasPagination]);

  const setPage = useCallback(
    (page: number) => {
      const valid = Math.min(Math.max(1, page), totalPages);
      if (externalCurrentPage !== undefined && onPageChange) {
        onPageChange(valid);
      } else {
        setInternalCurrentPage(valid);
      }
    },
    [externalCurrentPage, onPageChange, totalPages]
  );

  // ---------- Selection ----------
  const [internalSelectedIds, setInternalSelectedIds] = useState<Set<string | number>>(new Set());
  const selectedIds = externalSelectedIds !== undefined ? externalSelectedIds : internalSelectedIds;

  const toggleSelect = useCallback(
    (row: T, idx: number) => {
      const id = keyExtractor(row, idx);
      const newSet = new Set(selectedIds);
      if (newSet.has(id)) newSet.delete(id);
      else newSet.add(id);
      if (externalSelectedIds === undefined) setInternalSelectedIds(newSet);
      if (onSelectionChange) {
        const selectedRows = data.filter((r, i) => newSet.has(keyExtractor(r, i)));
        onSelectionChange(newSet, selectedRows);
      }
    },
    [keyExtractor, selectedIds, data, externalSelectedIds, onSelectionChange]
  );

  const toggleSelectAll = useCallback(() => {
    const pageIds = paginatedData.map((row, i) => keyExtractor(row, i));
    const allSelected = pageIds.every(id => selectedIds.has(id));
    const newSet = new Set(selectedIds);
    if (allSelected) {
      pageIds.forEach(id => newSet.delete(id));
    } else {
      pageIds.forEach(id => newSet.add(id));
    }
    if (externalSelectedIds === undefined) setInternalSelectedIds(newSet);
    if (onSelectionChange) {
      const selectedRows = data.filter((r, i) => newSet.has(keyExtractor(r, i)));
      onSelectionChange(newSet, selectedRows);
    }
  }, [paginatedData, keyExtractor, selectedIds, data, externalSelectedIds, onSelectionChange]);

  return {
    sortedData,
    paginatedData,
    currentPage,
    totalPages,
    setPage,
    sortColumn,
    sortDirection,
    toggleSort,
    selectedIds,
    toggleSelect,
    toggleSelectAll,
    hasPagination,
  };
}