// src/core/components/Generic/core/types.ts
import { PaginationState } from "../features/pagination/types";
import { SelectionState } from '../features/selection/types';

// استیت کلی ترکیبی از استیت‌های فیچرهای مختلف است
export type CrudState<T, S> = PaginationState & SelectionState & {
  data: T[];
  isLoading: boolean;
  error: string | null;
  filters: Partial<S>;
};

// اکشن‌های استاندارد استور
export interface CrudActions<T, S> {
  setData: (data: T[], totalCount: number) => void;
  setLoading: (isLoading: boolean) => void;
  setError: (error: string | null) => void;
  setFilters: (filters: Partial<S>) => void;
  setPagination: (page: number, pageSize: number) => void;
  toggleSelection: (id: string) => void;
  selectAll: (ids: string[]) => void;
  clearSelection: () => void;
}

// تایپ استور جامع (ترکیب استیت و اکشن‌ها)
export type CrudStore<T, S> = CrudState<T, S> & CrudActions<T, S>;

// قرارداد استاندارد API شما
export interface BaseApi<T, S, C, U> {
  search: (req: S) => Promise<T[]>;
  create: (cmd: C) => Promise<void>;
  update: (cmd: U) => Promise<void>;
  batchUpdate: (cmds: U[]) => Promise<void>;
  getList?: () => Promise<T[]>;
  getSelectionList?: () => Promise<any[]>;
}