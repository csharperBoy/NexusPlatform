// src/core/components/SmartDataGrid/plugins/tree/useGridTree.ts
import { useState, useMemo, useEffect, useCallback } from 'react';
import { GridInstance } from '../../core/SmartDataGridContext';

export interface TreePluginConfig<T> {
  useTreeStructure?: boolean;
  parentKey?: keyof T;
  keyExtractor: (row: T, index?: number) => string | number;
  emptyRowFactory?: () => T;
  currentPage: number;
  pageSize?: number;
}

export function useGridTree<T>(instance: GridInstance<T>, config: TreePluginConfig<T>) {
  const { 
    useTreeStructure, 
    parentKey = 'parentId', 
    keyExtractor, 
    emptyRowFactory,
    currentPage,
    pageSize 
  } = config;

  const [expandedKeys, setExpandedKeys] = useState<Set<string | number>>(new Set());

  const toggleRowExpand = useCallback((rowKey: string | number) => {
    setExpandedKeys(prev => {
      const next = new Set(prev);
      if (next.has(rowKey)) next.delete(rowKey);
      else next.add(rowKey);
      return next;
    });
  }, []);

  const addChildRow = useCallback((parentRowId: string | number) => {
    if (!emptyRowFactory) return;
    
    const newRow = emptyRowFactory();
    (newRow as any)[parentKey] = parentRowId; 
    const newRowKey = keyExtractor(newRow, -1);

    instance.setRawData(prev => [newRow, ...prev]);

    setExpandedKeys(prev => {
      const next = new Set(prev);
      next.add(parentRowId);
      return next;
    });

    const currentActions = instance.pluginState.actions || {};
    instance.setPluginState('actions', {
      ...currentActions,
      addedKeys: new Set([...(currentActions.addedKeys || []), newRowKey]),
      editingKeys: new Set([...(currentActions.editingKeys || []), newRowKey]),
      isDirty: true
    });
  }, [emptyRowFactory, parentKey, keyExtractor, instance]);

  useEffect(() => {
    if (!useTreeStructure) return;
    instance.setPluginState('tree', { expandedKeys, parentKey });
    instance.registerAction('toggleRowExpand', toggleRowExpand);
    instance.registerAction('addChildRow', addChildRow);
  }, [useTreeStructure, expandedKeys, parentKey, toggleRowExpand, addChildRow, instance]);

  // 🔥 پردازش هوشمند درخت + صفحه‌بندی اختصاصی روی ریشه‌ها
  const { paginatedTreeData, totalTreePages } = useMemo(() => {
    if (!useTreeStructure) return { paginatedTreeData: [], totalTreePages: 1 };

    const data = instance.rawData;
    const childrenMap = new Map<any, T[]>();
    const allKeys = new Set(data.map(r => keyExtractor(r)));

    // ۱. نقشه‌برداری از فرزندان
    data.forEach(row => {
      const pId = (row as any)[parentKey];
      if (!childrenMap.has(pId)) childrenMap.set(pId, []);
      childrenMap.get(pId)!.push(row);
    });

    // ۲. پیدا کردن تمام ریشه‌های اصلی (بدون والد)
    const allRoots = data.filter(r => {
      const pId = (r as any)[parentKey];
      return !pId || !allKeys.has(pId);
    });

    // ۳. محاسبه تعداد کل صفحات بر اساس تعداد ریشه‌ها
    const hasPagination = pageSize !== undefined && pageSize > 0;
    const totalPages = hasPagination ? Math.ceil(allRoots.length / pageSize!) : 1;

    // ۴. اعمال صفحه‌بندی (Slice) فقط و فقط روی ریشه‌ها
    const start = (currentPage - 1) * (pageSize || 0);
    const paginatedRoots = hasPagination 
      ? allRoots.slice(start, start + pageSize!) 
      : allRoots;

    const flattenedTree: Array<{ row: T, depth: number, hasChildren: boolean }> = [];

    // ۵. تابع بازگشتی برای آوردن فرزندان ریشه‌های مجاز این صفحه
    const traverse = (parentId: any, depth: number) => {
      const children = childrenMap.get(parentId) || [];
      children.forEach(child => {
        const key = keyExtractor(child);
        const hasChildren = childrenMap.has(key) && childrenMap.get(key)!.length > 0;
        
        flattenedTree.push({ row: child, depth, hasChildren });

        if (expandedKeys.has(key)) {
          traverse(key, depth + 1);
        }
      });
    };

    // ۶. ساخت خروجی نهایی جدول برای ریشه‌های صفحه جاری
    paginatedRoots.forEach(root => {
      const key = keyExtractor(root);
      const hasChildren = childrenMap.has(key) && childrenMap.get(key)!.length > 0;
      flattenedTree.push({ row: root, depth: 0, hasChildren });
      
      if (expandedKeys.has(key)) {
        traverse(key, 1);
      }
    });

    return {
      paginatedTreeData: flattenedTree,
      totalTreePages: totalPages
    };
  }, [instance.rawData, useTreeStructure, expandedKeys, parentKey, keyExtractor, currentPage, pageSize]);

  return { paginatedTreeData, totalTreePages };
}