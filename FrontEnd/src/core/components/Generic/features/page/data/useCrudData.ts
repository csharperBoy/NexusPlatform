// src/core/components/Generic/features/page/data/useCrudData.ts

import { useShallow } from 'zustand/react/shallow';
import { useCrudStore } from '../../../core/useCrudStore';
import { CrudDataSlice } from './dataSlice';

export function useCrudData<TDto, TCreateCmd, TUpdateCmd, TSearchReq>() {
  return useCrudStore<
    TDto, 
    TCreateCmd, 
    TUpdateCmd, 
    TSearchReq, 
    CrudDataSlice<TDto, TSearchReq> & { pagedData: TDto[] }
  >(
    useShallow((state) => {
      let pagedData = state.data;

      // برش داده‌ها فقط در صورتی که پجینیشن فعال و روی حالت Client باشد انجام می‌شود
      if (state.pagination && state.pagination.mode === 'client') {
        const { pageIndex, pageSize } = state.pagination;
        const start = pageIndex * pageSize;
        const end = start + pageSize;
        pagedData = state.data.slice(start, end);
      }

      return {
        data: state.data,
        pagedData,
        isLoading: state.isLoading,
        error: state.error,
        pagination: state.pagination,
        fetchData: state.fetchData,
        setPagination: state.setPagination,
        setData: state.setData,
      };
    })
  );
}