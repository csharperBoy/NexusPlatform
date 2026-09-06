// src/components/crud/hooks/useCrudStore.ts

import { useContext } from 'react';
import { useStore } from 'zustand';
import { CrudContext } from '../CrudProvider'; 
import { CrudStore } from './types';

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
 * T: مدل موجودیت (مثلاً PostDto)
 * S: مدل جستجو (مثلاً PostSearchDto)
 * U: خروجی سلکتور (مثلاً number برای page)
 */
export function useCrudStore<T, S, U>(
  selector: (state: CrudStore<T, S>) => U
): U {
  // ۱. دریافت رفرنس استور از کانتکست
  const { store } = useCrudContext();
  
  // ۲. پاس دادن استور و سلکتور به هوک useStore خود Zustand
  // این کار باعث می‌شود کامپوننت فقط زمانی رندر شود که خروجی سلکتور تغییر کند
  return useStore(store, selector);
}

/**
 * هوک دریافت API با تایپ‌های استاندارد شما
 */
export function useCrudApi<T, S, C, U_Update>() {
  const { api } = useCrudContext();
  return api as BaseApi<T, S, C, U_Update>;
}