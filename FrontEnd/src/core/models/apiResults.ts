//src/
export interface BatchResult<T> {
  succeeded: boolean;
  successMessages?: string[];
  errors?: string[];
  data?: T;
}