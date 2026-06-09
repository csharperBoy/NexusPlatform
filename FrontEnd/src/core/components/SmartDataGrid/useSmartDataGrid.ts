// src/core/components/SmartDataGrid/useSmartDataGrid.ts
import { useState, useMemo, useCallback, useEffect } from 'react';
import { SmartDataGridProps, BatchChanges } from './SmartDataGrid.types';
import * as XLSX from 'xlsx';

export function useSmartDataGrid<T>({
  data: initialData,
  columns,
  keyExtractor,
  allowExcelImport,
  pageSize,
  validateRow,
  emptyRowFactory,
}: SmartDataGridProps<T>) {
  
  // لایه اصلی داده‌های محلی جدول
  const [localData, setLocalData] = useState<T[]>([]);
  
  // وضعیت‌های پیش‌نویس (Draft States)
  const [addedKeys, setAddedKeys] = useState<Set<string | number>>(new Set());
  const [modifiedKeys, setModifiedKeys] = useState<Set<string | number>>(new Set());
  const [deletedKeys, setDeletedKeys] = useState<Set<string | number>>(new Set());
  
  // وضعیت رکوردهایی که هم‌اکنون در حال ویرایش درون‌خطی (Inline) هستند
  const [editingKeys, setEditingKeys] = useState<Set<string | number>>(new Set());

  // همگام‌سازی داده‌های بیرونی با دیتای داخلی جدول
  useEffect(() => {
    setLocalData(initialData);
    setAddedKeys(new Set());
    setModifiedKeys(new Set());
    setDeletedKeys(new Set());
    setEditingKeys(new Set());
  }, [initialData]);

  // محاسبه‌ی خطاهای سطرها جهت نمایش رنگ قرمز
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

  // ---------- افزودن سطر جدید ----------
  const handleAddNewRow = useCallback(() => {
    if (!emptyRowFactory) return;
    const newRow = emptyRowFactory();
    const key = keyExtractor(newRow, -1);
    
    setLocalData(prev => [newRow, ...prev]);
    setAddedKeys(prev => { const n = new Set(prev); n.add(key); return n; });
    setEditingKeys(prev => { const n = new Set(prev); n.add(key); return n; });
  }, [emptyRowFactory, keyExtractor]);

  // ---------- حذف سطر (علامت‌گذاری برای حذف) ----------
  const handleDeleteRow = useCallback((row: T, idx: number) => {
    const key = keyExtractor(row, idx);
    if (addedKeys.has(key)) {
      // اگر سطر کلاً جدید بوده و هنوز ذخیره نشده، کلاً از حافظه محلی پاکش کن
      setLocalData(prev => prev.filter((r, i) => keyExtractor(r, i) !== key));
      setAddedKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
      setEditingKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
    } else {
      // اگر از قبل در دیتابیس بوده، علامت حذف بزن
      setDeletedKeys(prev => { const n = new Set(prev); n.add(key); return n; });
      setEditingKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
    }
  }, [keyExtractor, addedKeys]);

  // ---------- تغییرات درون سلولی ----------
  const handleCellChange = useCallback((key: string | number, field: string, value: any) => {
    setLocalData(prev => prev.map(row => {
      const rKey = keyExtractor(row);
      if (rKey === key) {
        return { ...row, [field]: value };
      }
      return row;
    }));

    if (!addedKeys.has(key)) {
      setModifiedKeys(prev => { const n = new Set(prev); n.add(key); return n; });
    }
  }, [keyExtractor, addedKeys]);

  // ---------- کنترل وضعیت ویرایش تکی (Inline Edit) ----------
  const startEditing = useCallback((key: string | number) => {
    setEditingKeys(prev => { const n = new Set(prev); n.add(key); return n; });
  }, []);

  const cancelEditing = useCallback((key: string | number, idx: number) => {
    setEditingKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
    if (addedKeys.has(key)) {
      setLocalData(prev => prev.filter((r, i) => keyExtractor(r, i) !== key));
      setAddedKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
    } else {
      // بازگرداندن به مقدار اولیه از دیتای اصلی پروژه
      const originalRow = initialData.find((r, i) => keyExtractor(r, i) === key);
      if (originalRow) {
        setLocalData(prev => prev.map(r => keyExtractor(r) === key ? originalRow : r));
      }
      setModifiedKeys(prev => { const n = new Set(prev); n.delete(key); return n; });
    }
  }, [addedKeys, initialData, keyExtractor]);

  // ---------- بارگذاری هوشمند فایل اکسل (Upsert) ----------
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
      const importedData = XLSX.utils.sheet_to_json<any>(ws);

      setLocalData(prev => {
        let updatedList = [...prev];
        const newAddedKeys = new Set(addedKeys);
        const newModifiedKeys = new Set(modifiedKeys);

        importedData.forEach((excelRow) => {
          // استخراج کلید بر اساس فانکشن داینامیک معرفی شده توسط کاربر
          const excelKey = keyExtractor(excelRow, -1);
          const existingIndex = updatedList.findIndex(r => keyExtractor(r) === excelKey);

          if (existingIndex > -1) {
            // سناریو: سطر وجود دارد -> بازنویسی داده‌ها (Merge/Overwrite)
            updatedList[existingIndex] = { ...updatedList[existingIndex], ...excelRow };
            if (!newAddedKeys.has(excelKey)) {
              newModifiedKeys.add(excelKey);
            }
          } else {
            // سناریو: سطر جدید است -> ایجاد سطر جدید و افزودن به پیش‌نویس‌ها
            updatedList = [excelRow, ...updatedList];
            newAddedKeys.add(excelKey);
          }
          // رکوردهای وارد شده از اکسل اتوماتیک به وضعیت ویرایش انبوه می‌روند
          setEditingKeys(prevEdit => { const n = new Set(prevEdit); n.add(excelKey); return n; });
        });

        setAddedKeys(newAddedKeys);
        setModifiedKeys(newModifiedKeys);
        return updatedList;
      });
    };
    reader.readAsBinaryString(file);
    e.target.value = ''; // ریست کردن اینپوت فایل
  }, [allowExcelImport, keyExtractor, addedKeys, modifiedKeys]);

  // ---------- پکیج کردن کل تغییرات برای ذخیره دسته‌ای ----------
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

  // ---------- مدیریت مرتب‌سازی و صفحه‌بندی محلی ----------
  const [sortColumn, setSortColumn] = useState<string | undefined>(undefined);
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc' | undefined>(undefined);

  const toggleSort = useCallback((columnId: string) => {
    const col = columns.find(c => c.id === columnId);
    if (!col?.sortable) return;
    setSortDirection(prev => (sortColumn === columnId && prev === 'asc' ? 'desc' : 'asc'));
    setSortColumn(columnId);
  }, [columns, sortColumn]);

  const sortedData = useMemo(() => {
    // رکوردهای حذف شده را در نمای ظاهری فیلتر کن
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
    localData,
    paginatedData,
    currentPage,
    totalPages,
    setPage: setCurrentPage,
    sortColumn,
    sortDirection,
    toggleSort,
    editingKeys,
    addedKeys,
    modifiedKeys,
    rowErrors,
    isDirty,
    handleAddNewRow,
    handleDeleteRow,
    handleCellChange,
    startEditing,
    cancelEditing,
    handleExcelUpload,
    getBatchChanges,
  };
}