import { useEffect, useState } from 'react';
import { GridInstance } from '../../core/SmartDataGridContext';

export function useGridPagination<T>(instance: GridInstance<T>, options?: { pageSize?: number }) {
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = options?.pageSize;
  const hasPagination = pageSize !== undefined && pageSize > 0;

  useEffect(() => {
    if (!hasPagination) return;
    
    // تعداد کل رکوردها را بر اساس دیتا بعد از فیلتر/سورتمون به دست میاریم
    const sortedData = instance.transformers.sort ? instance.transformers.sort(instance.rawData) : instance.rawData;
    const totalPages = Math.ceil(sortedData.length / pageSize!);

    instance.setPluginState('pagination', { currentPage, totalPages, hasPagination });
    instance.registerAction('setPage', (page: number) => setCurrentPage(page));
  }, [currentPage, pageSize, instance.rawData, instance.transformers.sort, hasPagination]);

  useEffect(() => {
    if (!hasPagination) return;
    instance.registerTransformer('paginate', (data: T[]) => {
      const start = (currentPage - 1) * pageSize!;
      return data.slice(start, start + pageSize!);
    });
  }, [currentPage, pageSize, hasPagination]);
}