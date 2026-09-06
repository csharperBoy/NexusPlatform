// src/components/crud/store.ts
import { createStore } from 'zustand/vanilla';
import { CrudStore, CrudState } from './types';

export const createCrudStore = <T, S>(
  initialState?: Partial<CrudState<T, S>>
) => {
  const defaultState: CrudState<T, S> = {
    data: [],
    totalCount: 0,
    isLoading: false,
    error: null,
    page: 1,
    pageSize: 10,
    sortBy: null,
    sortDesc: false,
    filters: {},
    selectedIds: [],
  };

  return createStore<CrudStore<T, S>>()((set) => ({
    ...defaultState,
    ...initialState,

    setData: (data, totalCount) => set({ data, totalCount, isLoading: false, error: null }),
    setLoading: (isLoading) => set({ isLoading }),
    setError: (error) => set({ error, isLoading: false }),
    
    setPagination: (page, pageSize) => set({ page, pageSize }),
    setSorting: (sortBy, sortDesc) => set({ sortBy, sortDesc, page: 1 }), // تغییر سورت یعنی رفتن به صفحه اول
    
    setFilters: (filters) => set({ filters, page: 1 }), // تغییر فیلتر یعنی رفتن به صفحه اول
    
    toggleSelection: (id) => 
      set((state) => ({
        selectedIds: state.selectedIds.includes(id)
          ? state.selectedIds.filter((i) => i !== id)
          : [...state.selectedIds, id],
      })),
      
    selectAll: (ids) => set({ selectedIds: ids }),
    clearSelection: () => set({ selectedIds: [] }),
  }));
};