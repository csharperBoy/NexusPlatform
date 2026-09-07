import { createStore } from 'zustand/vanilla';
import { CrudStore, GenericCrudApi } from './types';
import { createDataSlice, PaginationState } from '../features/page/data/dataSlice';

export const createCrudStore = <TDto, TCreateCmd, TUpdateCmd, TSearchReq>(
  api: GenericCrudApi<TDto, TCreateCmd, TUpdateCmd, TSearchReq>,
  initialPagination: PaginationState | null = null // دریافت کانفیگ اولیه پجینیشن
) => {
  return createStore<CrudStore<TDto, TCreateCmd, TUpdateCmd, TSearchReq>>()(
    (set, get, apiStore) => ({
      api,
      
      // ترکیب Slice دیتا با استور اصلی
      ...createDataSlice<TDto, TCreateCmd, TUpdateCmd, TSearchReq>(initialPagination)(set, get, apiStore),
    })
  );
};
export type CrudStoreInstance<TDto, TCreateCmd, TUpdateCmd, TSearchReq> = ReturnType<
  typeof createCrudStore<TDto, TCreateCmd, TUpdateCmd, TSearchReq>
>;