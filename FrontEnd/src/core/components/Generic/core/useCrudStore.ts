// src/core/components/Generic/core/useCrudStore.ts

import { useContext } from 'react';
import { useStore } from 'zustand';
import { CrudContext } from './CrudProvider';
import { CrudStore } from './types';
import { CrudStoreInstance } from './store';

// هوک کمکی برای دریافت نمونه Context با اعمال تایپ‌های داینامیک
export function useCrudContext<TDto, TCreateCmd, TUpdateCmd, TSearchReq>(): CrudStoreInstance<TDto, TCreateCmd, TUpdateCmd, TSearchReq> {
  const context = useContext(CrudContext);
  if (!context) {
    throw new Error('useCrudStore must be used within a CrudProvider');
  }
  return context as CrudStoreInstance<TDto, TCreateCmd, TUpdateCmd, TSearchReq>;
}

// هوک اصلی برای Select کردن از استور
export function useCrudStore<TDto, TCreateCmd, TUpdateCmd, TSearchReq, TSelected>(
  selector: (state: CrudStore<TDto, TCreateCmd, TUpdateCmd, TSearchReq>) => TSelected
): TSelected {
  const store = useCrudContext<TDto, TCreateCmd, TUpdateCmd, TSearchReq>();
  return useStore(store, selector);
}