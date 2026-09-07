// src/core/components/Generic/core/CrudProvider.tsx

import React, { createContext, useRef, ReactNode } from 'react';
import { createCrudStore, CrudStoreInstance } from './store';
import { GenericCrudApi } from './types';

// استفاده از any برای مقدار اولیه Context مجاز است، زیرا Type-Safety در هوک useCrudStore تضمین می‌شود
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export const CrudContext = createContext<CrudStoreInstance<any, any, any, any> | null>(null);

interface CrudProviderProps<TDto, TCreateCmd, TUpdateCmd, TSearchReq> {
  api: GenericCrudApi<TDto, TCreateCmd, TUpdateCmd, TSearchReq>;
  children: ReactNode;
}

export const CrudProvider = <TDto, TCreateCmd, TUpdateCmd, TSearchReq>({
  api,
  children,
}: CrudProviderProps<TDto, TCreateCmd, TUpdateCmd, TSearchReq>) => {
  const storeRef = useRef<CrudStoreInstance<TDto, TCreateCmd, TUpdateCmd, TSearchReq>>(null);

  if (!storeRef.current) {
    storeRef.current = createCrudStore<TDto, TCreateCmd, TUpdateCmd, TSearchReq>(api);
  }

  return (
    <CrudContext.Provider value={storeRef.current}>
      {children}
    </CrudContext.Provider>
  );
};