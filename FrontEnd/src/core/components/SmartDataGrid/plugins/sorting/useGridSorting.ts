import { useEffect, useState, useCallback } from 'react';
import { GridInstance } from '../../core/SmartDataGridContext';

export function useGridSorting<T>(instance: GridInstance<T>) {
  const [sortColumn, setSortColumn] = useState<string | undefined>(undefined);
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc' | undefined>(undefined);

  const toggleSort = useCallback((columnId: string) => {
    setSortDirection(prev => (sortColumn === columnId && prev === 'asc' ? 'desc' : 'asc'));
    setSortColumn(columnId);
  }, [sortColumn]);

  useEffect(() => {
    instance.setPluginState('sorting', { sortColumn, sortDirection });
    instance.registerAction('toggleSort', toggleSort);
  }, [sortColumn, sortDirection, toggleSort]);

  useEffect(() => {
    instance.registerTransformer('sort', (data: T[]) => {
      if (!sortColumn || !sortDirection) return data;
      const col = instance.columns.find(c => c.id === sortColumn);
      if (!col) return data;
      const accessor = col.accessor || ((row: T) => (row as any)[col.id]);

      return [...data].sort((a, b) => {
        let aVal = accessor(a);
        let bVal = accessor(b);
        if (aVal == null && bVal == null) return 0;
        if (aVal == null) return sortDirection === 'asc' ? -1 : 1;
        if (bVal == null) return sortDirection === 'asc' ? 1 : -1;
        if (typeof aVal === 'number' && typeof bVal === 'number') {
          return sortDirection === 'asc' ? aVal - bVal : bVal - aVal;
        }
        return sortDirection === 'asc' ? String(aVal).localeCompare(String(bVal)) : String(bVal).localeCompare(String(aVal));
      });
    });
  }, [sortColumn, sortDirection, instance.columns]);
}