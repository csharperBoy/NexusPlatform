import { useEffect, useState, useMemo, useCallback } from 'react';
import { GridInstance } from '../../core/SmartDataGridContext';
import { BatchChanges } from '../../SmartDataGrid.types';
import React from 'react';
import { RowActionsUI } from './RowActionsUI';

export function useGridEditing<T>(instance: GridInstance<T>, config: {
  allowEdit?: boolean; allowDelete?: boolean;
  validateRow?: (row: T) => string[] | null;
  emptyRowFactory?: () => T;
  onSaveRow?: (row: T, actionType: 'add' | 'edit' | 'delete', index: number) => Promise<void> | void;
  onSaveBatch?: (changes: BatchChanges<T>) => Promise<void> | void;
}) {
  const [addedKeys, setAddedKeys] = useState<Set<string | number>>(new Set());
  const [modifiedKeys, setModifiedKeys] = useState<Set<string | number>>(new Set());
  const [deletedKeys, setDeletedKeys] = useState<Set<string | number>>(new Set());
  const [editingKeys, setEditingKeys] = useState<Set<string | number>>(new Set());

  const rowErrors = useMemo(() => {
    const errorsMap = new Map<string | number, string[]>();
    if (!config.validateRow) return errorsMap;
    instance.rawData.forEach((row, idx) => {
      const key = instance.keyExtractor(row, idx);
      if (!deletedKeys.has(key)) {
        const errors = config.validateRow!(row);
        if (errors && errors.length > 0) errorsMap.set(key, errors);
      }
    });
    return errorsMap;
  }, [instance.rawData, deletedKeys, instance.keyExtractor, config.validateRow]);

  const handleAddNewRow = useCallback(() => {
    if (!config.emptyRowFactory) return;
    const newRow = config.emptyRowFactory();
    const key = instance.keyExtractor(newRow, -1);
    instance.setRawData(prev => [newRow, ...prev]);
    setAddedKeys(prev => new Set(prev).add(key));
    setEditingKeys(prev => new Set(prev).add(key));
  }, [config.emptyRowFactory, instance]);

  const handleCellChange = useCallback((key: string | number, field: string, value: any) => {
    instance.setRawData(prev => prev.map(row => instance.keyExtractor(row) === key ? { ...row, [field]: value } : row));
    if (!addedKeys.has(key)) setModifiedKeys(prev => new Set(prev).add(key));
  }, [instance, addedKeys]);

  const cancelEditing = useCallback((key: string | number, idx: number) => {
    setEditingKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
    if (addedKeys.has(key)) {
      instance.setRawData(prev => prev.filter((r, i) => instance.keyExtractor(r, i) !== key));
      setAddedKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
    } else {
      setModifiedKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
    }
  }, [addedKeys, instance]);

  const processSaveBatch = useCallback(async () => {
    if (rowErrors.size > 0) {
      alert('جدول حاوی سطرهای دارای خطا است.');
      return;
    }
    if (config.onSaveBatch) {
      const added: T[] = []; const modified: T[] = [];
      instance.rawData.forEach(row => {
        const key = instance.keyExtractor(row);
        if (addedKeys.has(key)) added.push(row);
        else if (modifiedKeys.has(key)) modified.push(row);
      });
      await config.onSaveBatch({ added, modified, deletedIds: Array.from(deletedKeys) });
      setAddedKeys(new Set()); setModifiedKeys(new Set()); setDeletedKeys(new Set()); setEditingKeys(new Set());
    }
  }, [rowErrors, config.onSaveBatch, instance.rawData, addedKeys, modifiedKeys, deletedKeys, instance.keyExtractor]);

  const isDirty = addedKeys.size > 0 || modifiedKeys.size > 0 || deletedKeys.size > 0;

  useEffect(() => {
    instance.setPluginState('editing', { editingKeys, addedKeys, modifiedKeys, deletedKeys, rowErrors, isDirty, allowAdd: !!config.emptyRowFactory });
    instance.registerAction('handleCellChange', handleCellChange);
    instance.registerAction('handleAddNewRow', handleAddNewRow);
    instance.registerAction('startEditing', (key: any) => setEditingKeys(prev => new Set(prev).add(key)));
    instance.registerAction('cancelEditing', cancelEditing);
    instance.registerAction('processSaveBatch', processSaveBatch);
    instance.registerAction('handleDeleteRow', (key: any) => {
      if (addedKeys.has(key)) {
        instance.setRawData(prev => prev.filter((r, i) => instance.keyExtractor(r, i) !== key));
        setAddedKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
      } else {
        setDeletedKeys(prev => new Set(prev).add(key));
      }
    });
  }, [editingKeys, addedKeys, modifiedKeys, deletedKeys, rowErrors, isDirty, handleCellChange, handleAddNewRow, cancelEditing, processSaveBatch, config.emptyRowFactory]);

  useEffect(() => {
    instance.registerTransformer('columns', (cols: any[]) => {
      if (!config.allowEdit && !config.allowDelete) return cols;
      const hasActionCol = cols.some(c => c.id === '__actions');
      if (hasActionCol) return cols;
      return [
        ...cols,
        {
          id: '__actions', header: 'عملیات سیستم', width: '140px', type: 'custom',
          render: (row: T, idx: number) => {
            const key = instance.keyExtractor(row, idx);
            // return React.createElement(require('./RowActionsUI').RowActionsUI, { rowKey: key, rowIndex: idx, row });
            return React.createElement(RowActionsUI, { rowKey: key, rowIndex: idx, row });
          }
        }
      ];
    });
  }, [config.allowEdit, config.allowDelete, instance.keyExtractor]);
}