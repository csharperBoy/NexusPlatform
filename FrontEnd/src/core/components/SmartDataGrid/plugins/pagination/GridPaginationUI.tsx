import React from 'react';
import { useGridContext } from '../../core/SmartDataGridContext';

export function GridPaginationUI() {
  const instance = useGridContext();
  const pagination = instance.pluginState.pagination;

  if (!pagination || !pagination.hasPagination || pagination.totalPages <= 1) return null;

  const { currentPage, totalPages } = pagination;
  const setPage = instance.actions.setPage;

  return (
    <div className="flex justify-center items-center gap-4 mt-2">
      <button
        onClick={() => setPage?.(currentPage - 1)}
        disabled={currentPage <= 1}
        className="px-4 py-1.5 border rounded-md text-sm bg-white hover:bg-gray-50 disabled:opacity-40 transition"
      >
        قبلی
      </button>
      <span className="text-sm text-gray-600">
        صفحه <span className="font-semibold text-gray-900">{currentPage}</span> از <span className="font-semibold text-gray-900">{totalPages}</span>
      </span>
      <button
        onClick={() => setPage?.(currentPage + 1)}
        disabled={currentPage >= totalPages}
        className="px-4 py-1.5 border rounded-md text-sm bg-white hover:bg-gray-50 disabled:opacity-40 transition"
      >
        بعدی
      </button>
    </div>
  );
}