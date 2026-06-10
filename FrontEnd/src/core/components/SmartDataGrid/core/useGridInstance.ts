import { useState, useMemo, useCallback, useEffect } from 'react';
import { ColumnDef } from '../SmartDataGrid.types';
import { GridInstance } from './SmartDataGridContext';

export function useGridInstance<T>(config: {
  data: T[];
  columns: ColumnDef<T>[];
  keyExtractor: (row: T, index?: number) => string | number;
}): GridInstance<T> {
  const [rawData, setRawData] = useState<T[]>(config.data);
const [columns, setColumns] = useState<ColumnDef<T>[]>(config.columns);
  const [pluginState, setInternalPluginState] = useState<Record<string, any>>({});
  const [actions, setActions] = useState<Record<string, any>>({});
//   const [transformers, setTransformers] = useState<any>({});
const [transformers, setTransformers] = useState<Record<string, any>>({});

    useEffect(() => {
    setRawData(config.data);
  }, [config.data]);

  const setPluginState = useCallback((key: string, value: any) => {
    setInternalPluginState(prev => ({
      ...prev,
      [key]: typeof value === 'function' ? value(prev[key]) : value
    }));
  }, []);

  const registerAction = useCallback((name: string, fn: any) => {
    setActions(prev => ({ ...prev, [name]: fn }));
  }, []);

  const registerTransformer = useCallback((type: string, fn: any) => {
    setTransformers(prev => ({ ...prev, [type]: fn }));
  }, []);

  const getFinalColumns = useCallback(() => {
    if (transformers.columns) return transformers.columns(columns);
    return columns;
  }, [columns, transformers]);

  const getProcessedData = useCallback(() => {
    let result = [...rawData];
    if (transformers.sort) result = transformers.sort(result);
    if (transformers.paginate) result = transformers.paginate(result);
    return result;
  }, [rawData, transformers]);

  return {
    rawData, setRawData, columns, setColumns, keyExtractor: config.keyExtractor,
    pluginState, setPluginState, actions, registerAction, transformers, registerTransformer,
    getFinalColumns, getProcessedData
  };
}