// تمام تایپ‌های استیت‌ها اینجا جمع می‌شوند تا استور اصلی بتواند آن‌ها را بشناسد
import { PaginationState } from "../features/pagination/types";
import { SelectionState } from '../features/selection/types';

// استیت کلی ترکیبی از استیت‌های فیچرهای مختلف است
export type CrudState<T, S> = PaginationState & SelectionState & {
  data: T[];
  isLoading: boolean;
  error: string | null;
  filters: Partial<S>;
};

// قرارداد استاندارد API شما
export interface BaseApi<T, S, C, U> {
  search: (req: S) => Promise<T[]>;
  create: (cmd: C) => Promise<void>;
  update: (cmd: U) => Promise<void>;
  batchUpdate: (cmds: U[]) => Promise<void>;
}