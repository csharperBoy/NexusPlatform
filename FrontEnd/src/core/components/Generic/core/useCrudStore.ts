// src/core/components/Generic/core/useCrudStore.ts
import { useContext } from 'react';
import { useStore } from 'zustand';
import { CrudContext } from './CrudProvider'; 
import { CrudStore, BaseApi } from './types';

/**
 * هوک پایه برای دریافت کانتکست
 */
export function useCrudContext() {
  const context = useContext(CrudContext);
  if (!context) {
    throw new Error('تمامی کامپوننت‌های CRUD باید داخل CrudProvider قرار بگیرند.');
  }
  return context;
}

/**
 * هوک اصلی اتصال به استور Zustand
 */
export function useCrudStore<T, S, U>(
  selector: (state: CrudStore<T, S>) => U
): U {
  const { store } = useCrudContext();
  return useStore(store, selector);
}

/**
 * هوک دریافت API با تایپ‌های استاندارد شما
 */
export function useCrudApi<T, S, C, U_Update>() {
  const { api } = useCrudContext();
  return api as BaseApi<T, S, C, U_Update>;
}