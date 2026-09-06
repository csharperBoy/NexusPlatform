import { createStore } from 'zustand/vanilla';
import { CrudState } from './types';

export const createCrudStore = <T, S>(initialState?: Partial<CrudState<T, S>>) => {
  return createStore<CrudState<T, S> & any>()((set) => ({
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
    setData: (data: T[], totalCount: number) => set({ data, totalCount, isLoading: false }),
    setLoading: (isLoading: boolean) => set({ isLoading }),
    
    // اکشن‌های صفحه‌بندی
    setPagination: (page: number, pageSize: number) => set({ page, pageSize }),
    
    // اکشن‌های انتخاب
    toggleSelection: (id: string) => 
      set((state: CrudState<T, S>) => ({
        selectedIds: state.selectedIds.includes(id)
          ? state.selectedIds.filter((i) => i !== id)
          : [...state.selectedIds, id],
      })),
    clearSelection: () => set({ selectedIds: [] }),
  }));
};