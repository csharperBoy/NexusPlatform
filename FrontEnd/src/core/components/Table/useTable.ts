// src/core/components/Table/useTable.ts
import { useState, useCallback, useEffect } from 'react';
import { UseTableProps, UseTableReturn } from './Table.type';

export function useTable<T>({
  rows,
  onRowClick,
  getRowId = (row: any) => row.id,
  getRowLabel = (row: any) => row.label || row.id,
  selectable = false,
  selected: controlledSelected,
  defaultSelected = [],
  onSelectionChange,
}: UseTableProps<T>): UseTableReturn<T> {
  

  const handleRowClick = useCallback((row: T) => {
   
  }, [getRowId,  onRowClick]);

  // ---------- مدیریت selected ----------
  const isControlled = controlledSelected !== undefined;
  const [internalSelected, setInternalSelected] = useState<Set<string>>(
    () => new Set(defaultSelected)
  );

  const selectedSet = isControlled ? new Set(controlledSelected) : internalSelected;

  const updateSelection = useCallback((newSet: Set<string>, changedRow?: T) => {
    if (!isControlled) {
      setInternalSelected(newSet);
    }
    const selectedArray = Array.from(newSet);
    // برای یافتن گره‌های انتخاب‌شده، نیاز به جستجو در کل درخت داریم – برای سادگی فقط idها را برمی‌گردانیم
    onSelectionChange?.(selectedArray, []);
  }, [isControlled, onSelectionChange]);

  const toggleSelect = useCallback((rowId: string, row: T) => {
    if (!selectable) return;

    const newSet = new Set(selectedSet);
    if (newSet.has(rowId)) {
      newSet.delete(rowId);
      // اگر cascade فعال باشد، فرزندان را نیز حذف می‌کنیم
      
    } else {
      newSet.add(rowId);
      
    }
    updateSelection(newSet, row);
  }, [selectable, selectedSet,  updateSelection]);

  const isSelected = useCallback((rowId: string) => selectedSet.has(rowId), [selectedSet]);

  const selectAll = useCallback(() => {
    if (!selectable) return;
  }, [selectable, rows,  updateSelection]);

  const clearSelection = useCallback(() => {
    if (!selectable) return;
    updateSelection(new Set());
  }, [selectable, updateSelection]);

  // اگر `rows` تغییر کند و در حالت controlled نیستیم، می‌توانیم انتخاب‌های نامعتبر را پاک کنیم (اختیاری)
  useEffect(() => {
    if (!isControlled && selectable) {
      // حذف idهایی که دیگر در درخت وجود ندارند (اختیاری)
    }
  }, [rows, isControlled, selectable]);

  return {
    rows,
   
    handleRowClick,
    getRowId,
    getRowLabel,
    selectedRows: selectedSet,
    toggleSelect,
    isSelected,
    selectAll,
    clearSelection,
  };
}