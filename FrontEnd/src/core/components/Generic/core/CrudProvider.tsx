// src/components/crud/CrudProvider.tsx
import React, { createContext, useContext, useRef } from 'react';
import { useStore } from 'zustand';
import { createCrudStore } from './store';
import {  CrudState } from '../core/types';

// این تایپ بر اساس استاندارد API شماست
export interface BaseApi<T, S, C, U> {
  search: (req: S) => Promise<T[]>;
  create: (cmd: C) => Promise<void>;
  update: (cmd: U) => Promise<void>;
  batchUpdate: (cmds: U[]) => Promise<void>;
  // ... سایر متدها
}

interface CrudContextProps<T, S, C, U> {
  store: ReturnType<typeof createCrudStore<T, S>>;
  api: BaseApi<T, S, C, U>;
}

// ساخت Context
const CrudContext = createContext<CrudContextProps<any, any, any, any> | null>(null);

interface CrudProviderProps<T, S, C, U> {
  children: React.ReactNode;
  api: BaseApi<T, S, C, U>;
  initialState?: Partial<CrudState<T, S>>;
}

export function CrudProvider<T, S, C, U>({ children, api, initialState }: CrudProviderProps<T, S, C, U>) {
  // استفاده از useRef برای اطمینان از اینکه استور فقط یک بار در اولین رندر ساخته می‌شود
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