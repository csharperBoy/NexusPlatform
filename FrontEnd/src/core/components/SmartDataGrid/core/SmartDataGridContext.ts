import { createContext, useContext, ReactNode } from 'react';
import { ColumnDef } from '../SmartDataGrid.types';

export interface GridInstance<T> {
  rawData: T[];
  setRawData: React.Dispatch<React.SetStateAction<T[]>>;
//   columns: ColumnDef<T>[];
//   setColumns: React.Dispatch<React.SetStateAction<ColumnDef<T>[]>>;
  
  keyExtractor: (row: T, index?: number) => string | number;

  columns: ColumnDef<T>[];
  setColumns: React.Dispatch<React.SetStateAction<ColumnDef<T>[]>>;

  // ثبت وضعیت‌های داینامیک پلاگین‌ها
  pluginState: Record<string, any>;
  setPluginState: (pluginKey: string, value: any | ((prev: any) => any)) => void;
  
  // اکشن‌های به اشتراک گذاشته شده توسط پلاگین‌ها
  actions: Record<string, (...args: any[]) => void>;
  registerAction: (actionName: string, fn: (...args: any[]) => void) => void;
  
  // خط لوله پردازش داده و ستون‌ها
  transformers: {
    sort?: (data: T[]) => T[];
    paginate?: (data: T[]) => T[];
    columns?: (cols: ColumnDef<T>[]) => ColumnDef<T>[];
  };
  registerTransformer: (type: 'sort' | 'paginate' | 'columns', fn: any) => void;
  
  getFinalColumns: () => ColumnDef<T>[];
  getProcessedData: () => T[];
}

const SmartDataGridContext = createContext<GridInstance<any> | null>(null);

export const SmartDataGridProvider = SmartDataGridContext.Provider;

export function useGridContext<T>() {
  const context = useContext(SmartDataGridContext);
  if (!context) throw new Error('useGridContext must be used within SmartDataGridProvider');
  return context as GridInstance<T>;
}