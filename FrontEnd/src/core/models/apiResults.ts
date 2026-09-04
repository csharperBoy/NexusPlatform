//src/
export interface BatchResult<T> {
  succeeded: boolean;
  successMessages?: string[];
  errors?: string[];
  data?: T;
}

// نوع مخصوص زمانی که داده‌ای برگردانده نمی‌شود
// export type BatchResultWithoutData = BatchResult<undefined>;
 export type BatchResultWithoutData = BatchResult<never>;