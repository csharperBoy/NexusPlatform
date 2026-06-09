// src/core/components/SmartDataGrid/useSmartDataGrid.ts
import { useState, useMemo, useCallback, useEffect } from 'react';
import { SmartDataGridProps, BatchChanges } from './SmartDataGrid.types';
import * as XLSX from 'xlsx';

export function useSmartDataGrid<T>(props: SmartDataGridProps<T>) {
  const {
    data: initialData,
    columns,
    keyExtractor,
    allowEdit,
    allowDelete,
    allowExcelImport,
    allowExcelExport,
    pageSize,
    validateRow,
    emptyRowFactory,
    onSaveRow,
    onSaveBatch
  } = props;

  const [localData, setLocalData] = useState<T[]>([]);
  const [addedKeys, setAddedKeys] = useState<Set<string | number>>(new Set());
  const [modifiedKeys, setModifiedKeys] = useState<Set<string | number>>(new Set());
  const [deletedKeys, setDeletedKeys] = useState<Set<string | number>>(new Set());
  const [editingKeys, setEditingKeys] = useState<Set<string | number>>(new Set());

  useEffect(() => {
    setLocalData(initialData);
    setAddedKeys(new Set());
    setModifiedKeys(new Set());
    setDeletedKeys(new Set());
    setEditingKeys(new Set());
  }, [initialData]);

  const rowErrors = useMemo(() => {
    const errorsMap = new Map<string | number, string[]>();
    if (!validateRow) return errorsMap;

    localData.forEach((row, idx) => {
      const key = keyExtractor(row, idx);
      if (!deletedKeys.has(key)) {
        const errors = validateRow(row);
        if (errors && errors.length > 0) {
          errorsMap.set(key, errors);
        }
      }
    });
    return errorsMap;
  }, [localData, deletedKeys, keyExtractor, validateRow]);

  const handleAddNewRow = useCallback(() => {
    if (!emptyRowFactory) return;
    const newRow = emptyRowFactory();
    const key = keyExtractor(newRow, -1);
    
    setLocalData(prev => [newRow, ...prev]);
    setAddedKeys(prev => { const n = new Set(prev); n.add(key); return n; });
    setEditingKeys(prev => { const n = new Set(prev); n.add(key); return n; });
  }, [emptyRowFactory, keyExtractor]);

  const handleDeleteRow = useCallback((row: T, idx: number) => {
    const key = keyExtractor(row, idx);
    if (addedKeys.has(key)) {
      setLocalData(prev => prev.filter((r, i) => keyExtractor(r, i) !== key));
      setAddedKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
      setEditingKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
    } else {
      setDeletedKeys(prev => { const n = new Set(prev); n.add(key); return n; });
      setEditingKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
    }
  }, [keyExtractor, addedKeys]);

  const handleCellChange = useCallback((key: string | number, field: string, value: any) => {
    setLocalData(prev => prev.map(row => {
      if (keyExtractor(row) === key) {
        return { ...row, [field]: value };
      }
      return row;
    }));

    if (!addedKeys.has(key)) {
      setModifiedKeys(prev => { const n = new Set(prev); n.add(key); return n; });
    }
  }, [keyExtractor, addedKeys]);

  const startEditing = useCallback((key: string | number) => {
    setEditingKeys(prev => { const n = new Set(prev); n.add(key); return n; });
  }, []);

  const cancelEditing = useCallback((key: string | number, idx: number) => {
    setEditingKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
    if (addedKeys.has(key)) {
      setLocalData(prev => prev.filter((r, i) => keyExtractor(r, i) !== key));
      setAddedKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
    } else {
      const originalRow = initialData.find((r, i) => keyExtractor(r, i) === key);
      if (originalRow) {
        setLocalData(prev => prev.map(r => keyExtractor(r) === key ? originalRow : r));
      }
      setModifiedKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
    }
  }, [addedKeys, initialData, keyExtractor]);

  const handleExcelUpload = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    if (!allowExcelImport || !e.target.files || e.target.files.length === 0) return;
    const file = e.target.files[0];
    const reader = new FileReader();

    reader.onload = (evt) => {
      if (!evt.target?.result) return;
      const bstr = evt.target.result;
      const wb = XLSX.read(bstr, { type: 'binary' });
      const wsname = wb.SheetNames[0];
      const ws = wb.Sheets[wsname];
      const importedRows = XLSX.utils.sheet_to_json<any>(ws);

      setLocalData(prev => {
        let updatedList = [...prev];
        const newAddedKeys = new Set(addedKeys);
        const newModifiedKeys = new Set(modifiedKeys);

        importedRows.forEach((excelRow) => {
          const parsedRow: any = emptyRowFactory ? emptyRowFactory() : {};

          columns.forEach(col => {
            if (col.id === '__actions') return;

            const excelKey = Object.keys(excelRow).find(k => 
              k.trim().toLowerCase() === String(col.id).toLowerCase() || 
              k.trim() === col.header
            );

            if (excelKey !== undefined) {
              let rawValue = excelRow[excelKey];

              if (col.type === 'select' && col.options) {
                const matchedOption = col.options.find((opt) => 
                  String(opt.value) === String(rawValue) ||
                  String(opt.label).toLowerCase() === String(rawValue).toLowerCase() ||
                  String(opt.display) === String(rawValue)
                );

                if (matchedOption) {
                  rawValue = isNaN(Number(matchedOption.value)) ? matchedOption.value : Number(matchedOption.value);
                }
              }
              parsedRow[col.id] = rawValue;
            }
          });

          const excelKey = keyExtractor(parsedRow, -1);
          const existingIndex = updatedList.findIndex(r => keyExtractor(r) === excelKey);

          if (existingIndex > -1) {
            updatedList[existingIndex] = { ...updatedList[existingIndex], ...parsedRow };
            if (!newAddedKeys.has(excelKey)) {
              newModifiedKeys.add(excelKey);
            }
          } else {
            updatedList = [parsedRow, ...updatedList];
            newAddedKeys.add(excelKey);
          }
          setEditingKeys(prevEdit => { const n = new Set(prevEdit); n.add(excelKey); return n; });
        });

        setAddedKeys(newAddedKeys);
        setModifiedKeys(newModifiedKeys);
        return updatedList;
      });
    };
    reader.readAsBinaryString(file);
    e.target.value = '';
  }, [allowExcelImport, columns, keyExtractor, emptyRowFactory, addedKeys, modifiedKeys]);

  const handleExcelExport = useCallback(() => {
    if(!allowExcelExport) return;
    const visibleData = localData.filter(row => !deletedKeys.has(keyExtractor(row)));
    
    const exportRows = visibleData.map((row) => {
      const exportedRow: Record<string, any> = {};
      
      columns.forEach(col => {
        if (col.id === '__actions') return;
        
        let val = col.accessor ? col.accessor(row) : (row as any)[col.id];
        
        if (col.type === 'select' && col.options) {
          const matchedOption = col.options.find(opt => String(opt.value) === String(val));
          if (matchedOption) {
            val = matchedOption.display || matchedOption.label;
          }
        } else if (col.type === 'date' && val) {
          val = new Date(val).toLocaleDateString('fa-IR');
        } else if (col.type === 'checkbox') {
          val = val ? 'بله' : 'خیر';
        }
        
        exportedRow[col.header] = val ?? '';
      });
      
      return exportedRow;
    });
    
    const ws = XLSX.utils.json_to_sheet(exportRows);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "لیست داده‌ها");
    XLSX.writeFile(wb, `DataGrid_Export_${new Date().toISOString().slice(0,10)}.xlsx`);
  }, [localData, deletedKeys, columns, keyExtractor]);

  const getBatchChanges = useCallback((): BatchChanges<T> => {
    const added: T[] = [];
    const modified: T[] = [];
    
    localData.forEach(row => {
      const key = keyExtractor(row);
      if (addedKeys.has(key)) added.push(row);
      else if (modifiedKeys.has(key)) modified.push(row);
    });

    return {
      added,
      modified,
      deletedIds: Array.from(deletedKeys)
    };
  }, [localData, keyExtractor, addedKeys, modifiedKeys, deletedKeys]);

  const isDirty = addedKeys.size > 0 || modifiedKeys.size > 0 || deletedKeys.size > 0;

  // ------------------ منطق‌های منتقل‌شده به داخل هوک ------------------
  
  // ۱. آماده‌سازی هوشمند ستون‌ها
  const hasActions = allowEdit || allowDelete;
  const finalColumns = useMemo(() => {
    if (!hasActions) return columns;
    return [
      ...columns,
      {
        id: '__actions',
        header: 'عملیات سیستم',
        width: '140px',
        type: 'custom' as const,
      },
    ];
  }, [columns, hasActions]);

  // ۲. مدیریت اعتبارسنجی و ذخیره تک‌سطری
  const processSaveRow = useCallback(async (row: T, idx: number) => {
    const key = keyExtractor(row, idx);
    if (rowErrors.has(key)) {
      alert(`لطفا خطاهای سطر را ابتدا برطرف کنید:\n${rowErrors.get(key)?.join('\n')}`);
      return;
    }
    if (onSaveRow) {
      const type = addedKeys.has(key) ? 'add' : 'edit';
      await onSaveRow(row, type, idx);
    }
    cancelEditing(key, idx);
  }, [keyExtractor, rowErrors, onSaveRow, addedKeys, cancelEditing]);

  // ۳. مدیریت اعتبارسنجی و ذخیره دسته‌ای (Batch)
  const processSaveBatch = useCallback(async () => {
    if (rowErrors.size > 0) {
      alert('جدول حاوی سطر‌های دارای خطا (قرمز رنگ) است. لطفا ابتدا آن‌ها را اصلاح کنید.');
      return;
    }
    if (onSaveBatch) {
      const changes = getBatchChanges();
      await onSaveBatch(changes);
    }
  }, [rowErrors, onSaveBatch, getBatchChanges]);

  // ------------------ مرتب‌سازی و صفحه‌بندی ------------------
  const [sortColumn, setSortColumn] = useState<string | undefined>(undefined);
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc' | undefined>(undefined);

  const toggleSort = useCallback((columnId: string) => {
    const col = columns.find(c => c.id === columnId);
    if (!col?.sortable) return;
    setSortDirection(prev => (sortColumn === columnId && prev === 'asc' ? 'desc' : 'asc'));
    setSortColumn(columnId);
  }, [columns, sortColumn]);

  const sortedData = useMemo(() => {
    const visibleData = localData.filter(row => !deletedKeys.has(keyExtractor(row)));
    if (!sortColumn || !sortDirection) return visibleData;

    const col = columns.find(c => c.id === sortColumn);
    if (!col) return visibleData;
    const accessor = col.accessor || ((row: T) => (row as any)[col.id]);

    return [...visibleData].sort((a, b) => {
      let aVal = accessor(a);
      let bVal = accessor(b);
      if (aVal == null && bVal == null) return 0;
      if (aVal == null) return sortDirection === 'asc' ? -1 : 1;
      if (bVal == null) return sortDirection === 'asc' ? 1 : -1;
      if (typeof aVal === 'number' && typeof bVal === 'number') {
        return sortDirection === 'asc' ? aVal - bVal : bVal - aVal;
      }
      return sortDirection === 'asc' 
        ? String(aVal).localeCompare(String(bVal)) 
        : String(bVal).localeCompare(String(aVal));
    });
  }, [localData, deletedKeys, keyExtractor, sortColumn, sortDirection, columns]);

  const [currentPage, setCurrentPage] = useState(1);
  const hasPagination = pageSize !== undefined && pageSize > 0;
  const totalPages = hasPagination ? Math.ceil(sortedData.length / pageSize!) : 1;

  const paginatedData = useMemo(() => {
    if (!hasPagination) return sortedData;
    const start = (currentPage - 1) * pageSize!;
    return sortedData.slice(start, start + pageSize!);
  }, [sortedData, currentPage, pageSize, hasPagination]);

  return {
    paginatedData,
    currentPage,
    totalPages,
    setPage: setCurrentPage,
    sortColumn,
    sortDirection,
    toggleSort,
    editingKeys,
    addedKeys,
    rowErrors,
    isDirty,
    handleAddNewRow,
    handleDeleteRow,
    handleCellChange,
    startEditing,
    cancelEditing,
    handleExcelUpload,
    handleExcelExport,
    finalColumns,        // خروجی جدید
    processSaveRow,      // خروجی جدید
    processSaveBatch,    // خروجی جدید
  };
}