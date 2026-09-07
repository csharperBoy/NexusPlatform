// src/core/components/Generic/templates/views/TestView.tsx

import React, { useEffect } from 'react';
import { useCrudData } from '../../features/page/data/useCrudData';

export interface TestEntity {
  id: string;
  title: string;
  category: string;
}

export const TestView: React.FC = () => {
  const { data, isLoading, error, pagination, fetchData, setPagination } = 
    useCrudData<TestEntity, Omit<TestEntity, 'id'>, TestEntity, { query?: string }>();

  // دریافت داده‌ها در ابتدای رندر کامپوننت
  useEffect(() => {
    fetchData();
  }, [fetchData]);

  return (
    <div className="p-6 max-w-4xl mx-auto space-y-6 dir-rtl bg-gray-50 rounded-xl shadow-sm border border-gray-200">
      {/* Header & Main Actions */}
      <div className="flex items-center justify-between border-b pb-4 border-gray-200">
        <div>
          <h2 className="text-xl font-bold text-gray-800">محیط تست ماژول داده (Data Slice)</h2>
          <p className="text-sm text-gray-500 mt-1">
            وضعیت صفحه‌بندی: {pagination ? 'فعال (Paginated)' : 'غیرفعال (Full List)'}
          </p>
        </div>

        <div className="flex gap-2">
            <button
                onClick={() => setPagination(null)}
                className={`px-3 py-1.5 text-xs font-medium rounded-lg border ${
                !pagination ? 'bg-slate-800 text-white' : 'bg-white text-gray-700 hover:bg-gray-50'
                }`}
            >
                بدون پجینیشن
            </button>

            <button
                onClick={() => setPagination({ pageIndex: 0, pageSize: 5, mode: 'client', totalCount: data.length })}
                className={`px-3 py-1.5 text-xs font-medium rounded-lg border ${
                pagination?.mode === 'client' ? 'bg-slate-800 text-white' : 'bg-white text-gray-700 hover:bg-gray-50'
                }`}
            >
                پجینیشن Client-Side
            </button>

            <button
                onClick={() => setPagination({ pageIndex: 0, pageSize: 5, mode: 'server', totalCount: 100 })}
                className={`px-3 py-1.5 text-xs font-medium rounded-lg border ${
                pagination?.mode === 'server' ? 'bg-slate-800 text-white' : 'bg-white text-gray-700 hover:bg-gray-50'
                }`}
            >
                پجینیشن Server-Side
            </button>
            </div>
      </div>

      {/* Display Error State */}
      {error && (
        <div className="p-4 bg-red-50 border-r-4 border-red-500 text-red-700 rounded-md text-sm">
          <strong>خطا:</strong> {error}
        </div>
      )}

      {/* Data Table */}
      <div className="bg-white rounded-lg shadow border border-gray-200 overflow-hidden">
        <table className="w-full text-right text-sm">
          <thead className="bg-gray-100 text-gray-600 font-semibold border-b border-gray-200">
            <tr>
              <th className="p-3">شناسه</th>
              <th className="p-3">عنوان</th>
              <th className="p-3">دسته‌بندی</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {isLoading && data.length === 0 ? (
              <tr>
                <td colSpan={3} className="text-center p-8 text-gray-400">
                  در حال بارگذاری اطلاعات...
                </td>
              </tr>
            ) : data.length === 0 ? (
              <tr>
                <td colSpan={3} className="text-center p-8 text-gray-400">
                  هیچ داده‌ای یافت نشد.
                </td>
              </tr>
            ) : (
              data.map((item) => (
                <tr key={item.id} className="hover:bg-gray-50 transition-colors">
                  <td className="p-3 font-mono text-xs text-gray-500">{item.id}</td>
                  <td className="p-3 font-medium text-gray-800">{item.title}</td>
                  <td className="p-3 text-gray-600">{item.category}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination Controls (در صورت فعال بودن Paging) */}
      {pagination && (
        <div className="flex items-center justify-between bg-white p-3 rounded-lg border border-gray-200 text-sm">
          <span className="text-gray-600">
            صفحه {pagination.pageIndex + 1} از {Math.ceil(pagination.totalCount / pagination.pageSize) || 1}
          </span>
          <div className="flex gap-2">
            <button
              disabled={pagination.pageIndex === 0}
              onClick={() => setPagination({ pageIndex: pagination.pageIndex - 1 })}
              className="px-3 py-1 bg-gray-100 hover:bg-gray-200 disabled:opacity-50 rounded text-gray-700"
            >
              قبلی
            </button>
            <button
              disabled={(pagination.pageIndex + 1) * pagination.pageSize >= pagination.totalCount}
              onClick={() => setPagination({ pageIndex: pagination.pageIndex + 1 })}
              className="px-3 py-1 bg-gray-100 hover:bg-gray-200 disabled:opacity-50 rounded text-gray-700"
            >
              بعدی
            </button>
          </div>
        </div>
      )}
    </div>
  );
};