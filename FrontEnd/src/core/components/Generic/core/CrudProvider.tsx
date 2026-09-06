// src/core/components/Generic/core/CrudProvider.tsx
import React, { createContext, useRef } from 'react';
import { createCrudStore } from './store';
import { CrudState, BaseApi } from './types';

export interface CrudContextProps<T, S, C, U> {
  store: ReturnType<typeof createCrudStore<T, S>>;
  api: BaseApi<T, S, C, U>;
}

// Context اکسپورت شد تا در useCrudStore قابل دریافت باشد
export const CrudContext = createContext<CrudContextProps<any, any, any, any> | null>(null);

interface CrudProviderProps<T, S, C, U> {
  children: React.ReactNode;
  api: BaseApi<T, S, C, U>;
  initialState?: Partial<CrudState<T, S>>;
}

export function CrudProvider<T, S, C, U>({ children, api, initialState }: CrudProviderProps<T, S, C, U>) {
  const storeRef = useRef<ReturnType<typeof createCrudStore<T, S>>>(undefined);
  
  if (!storeRef.current) {
    storeRef.current = createCrudStore(initialState);
  }

  return (
    <CrudContext.Provider value={{ store: storeRef.current, api }}>
      {children}
    </CrudContext.Provider>
  );
}