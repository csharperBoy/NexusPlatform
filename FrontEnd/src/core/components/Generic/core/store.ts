// src/core/components/Generic/core/store.ts
import { createStore } from 'zustand/vanilla';
import { CrudState, CrudStore } from './types';

export const createCrudStore = <T, S>(initialState?: Partial<CrudState<T, S>>) => {
  return createStore<CrudStore<T, S>>()((set) => ({
    // استیت‌های اولیه
    data: [],
    isLoading: false,
    error: null,
    filters: {},
    
    // استیت اولیه Pagination
    page: 1,
    pageSize: 10,
    totalCount: 0,
    
    // استیت اولیه Selection
    selectedIds: [],

    ...initialState,

    // اکشن‌های عمومی
    setData: (data: T[], totalCount: number) => set({ data, totalCount, isLoading: false, error: null }),
    setLoading: (isLoading: boolean) => set({ isLoading }),
    setError: (error: string | null) => set({ error, isLoading: false }),
    setFilters: (filters: Partial<S>) => set({ filters, page: 1 }),
    
    // اکشن‌های صفحه‌بندی
    setPagination: (page: number, pageSize: number) => set({ page, pageSize }),
    
    // اکشن‌های انتخاب
    toggleSelection: (id: string) => 
      set((state) => ({
        selectedIds: state.selectedIds.includes(id)
          ? state.selectedIds.filter((i) => i !== id)
          : [...state.selectedIds, id],
      })),
    selectAll: (ids: string[]) => set({ selectedIds: ids }),
    clearSelection: () => set({ selectedIds: [] }),
  }));
};