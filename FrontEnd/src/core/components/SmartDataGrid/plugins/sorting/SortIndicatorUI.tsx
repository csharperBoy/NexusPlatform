import React from 'react';
import { useGridContext } from '../../core/SmartDataGridContext';

export function SortIndicatorUI({ columnId }: { columnId: string }) {
  const instance = useGridContext();
  const sorting = instance.pluginState.sorting;

  if (!sorting || sorting.sortColumn !== columnId) return null;

  return (
    <span className="text-xs text-blue-600">
      {sorting.sortDirection === 'asc' ? '▲' : '▼'}
    </span>
  );
}