// src/core/components/Generic/features/page/data/dataSlice.ts

import { StateCreator } from 'zustand';
import { CrudStore } from '../../../core/types';

export type PaginationMode = 'client' | 'server';

export interface PaginationState {
  pageIndex: number;
  pageSize: number;
  totalCount: number;
  mode: PaginationMode;
}

export interface CrudDataState<TDto> {
  data: TDto[];
  isLoading: boolean;
  error: string | null;
  pagination: PaginationState | null; 
}

export interface CrudDataActions<TDto, TSearchReq> {
  setPagination: (pagination: Partial<PaginationState> | null) => void;
  fetchData: (searchReq?: TSearchReq) => Promise<void>;
  setData: (data: TDto[] | ((prev: TDto[]) => TDto[])) => void;
}

export type CrudDataSlice<TDto, TSearchReq> = CrudDataState<TDto> & CrudDataActions<TDto, TSearchReq>;

export const createDataSlice = <TDto, TCreateCmd, TUpdateCmd, TSearchReq>(
  initialPagination: PaginationState | null = null
): StateCreator<
  CrudStore<TDto, TCreateCmd, TUpdateCmd, TSearchReq>,
  [],
  [],
  CrudDataSlice<TDto, TSearchReq>
> => (set, get) => ({
  data: [],
  isLoading: false,
  error: null,
  pagination: initialPagination,

  setPagination: (newPagination) => set((state) => {
    if (newPagination === null) {
      return { pagination: null };
    }
    
    const currentMode = newPagination.mode ?? state.pagination?.mode ?? 'client';
    
    return {
      pagination: {
        pageIndex: newPagination.pageIndex ?? state.pagination?.pageIndex ?? 0,
        pageSize: newPagination.pageSize ?? state.pagination?.pageSize ?? 10,
        totalCount: newPagination.totalCount ?? state.pagination?.totalCount ?? state.data.length,
        mode: currentMode,
      }
    };
  }),

  setData: (updater) => set((state) => {
    const newData = typeof updater === 'function' ? (updater as (prev: TDto[]) => TDto[])(state.data) : updater;
    return {
      data: newData,
      pagination: state.pagination 
        ? { 
            ...state.pagination, 
            totalCount: state.pagination.mode === 'client' ? newData.length : state.pagination.totalCount 
          } 
        : null
    };
  }),

  fetchData: async (searchReq?: TSearchReq) => {
    const { api, pagination } = get();
    set({ isLoading: true, error: null });
    
    try {
      // در حالت Server-Side پارامترهای پجینیشن به درخواست ادغام می‌شوند
      const requestPayload = (pagination?.mode === 'server' && typeof searchReq === 'object' && searchReq !== null)
        ? { ...searchReq, pageIndex: pagination.pageIndex, pageSize: pagination.pageSize }
        : searchReq;

      const result = requestPayload 
        ? await api.search(requestPayload as TSearchReq) 
        : await api.getList();
        
      set((state) => ({ 
        data: result, 
        isLoading: false,
        pagination: state.pagination 
          ? { 
              ...state.pagination, 
              totalCount: state.pagination.mode === 'client' ? result.length : state.pagination.totalCount 
            } 
          : null
      }));
      
    } catch (error: any) {
      set({ 
        error: error?.message || 'خطایی در دریافت اطلاعات رخ داده است.', 
        isLoading: false 
      });
    }
  },
});