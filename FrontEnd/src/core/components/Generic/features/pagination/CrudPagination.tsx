import React from 'react';
import { useCrudPagination } from './useCrudPagination';

export function CrudPagination() {
  const { page, totalPages, totalCount, nextPage, prevPage } = useCrudPagination();

  if (totalCount === 0) return null;

  return (
    <div className="flex items-center justify-between border-t py-3">
      <span className="text-sm text-gray-500">
        مجموع رکوردها: {totalCount}
      </span>
      <div className="flex gap-2">
        <button 
          onClick={prevPage} 
          disabled={page === 1}
          className="px-3 py-1 border rounded disabled:opacity-50"
        >
          قبلی
        </button>
        <span className="px-3 py-1">صفحه {page} از {totalPages}</span>
        <button 
          onClick={nextPage} 
          disabled={page === totalPages}
          className="px-3 py-1 border rounded disabled:opacity-50"
        >
          بعدی
        </button>
      </div>
    </div>
  );
}