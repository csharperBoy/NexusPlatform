// src/core/components/SmartDataGrid/plugins/excel/useGridExcel.ts
import { useEffect, useCallback } from 'react';
import { GridInstance } from '../../core/SmartDataGridContext';
import * as XLSX from 'xlsx';

interface ExcelPluginConfig<T> {
  allowImport?: boolean;
  allowExport?: boolean;
  columns: any[];
  keyExtractor: (row: T, index?: number) => string | number;
  emptyRowFactory?: () => T;
}

export function useGridExcel<T>(instance: GridInstance<T>, config: ExcelPluginConfig<T>) {
  const { columns, keyExtractor, emptyRowFactory, allowImport, allowExport } = config;

  // 🔥 ۱. بازنویسی کامل اکسپورت هوشمند با پشتیبانی از شمسی، سلکت و چک‌باکس
  const handleExcelExport = useCallback(() => {
    if (!allowExport) return;

    // استخراج لیست شناسه سطرهای حذف شده از کانتکست ویرایش
    const deletedIds = instance.pluginState.editing?.deletedIds || new Set();
    
    // فیلتر کردن سطرهای حذف شده در سشن فعلی
    const visibleData = instance.rawData.filter(row => {
      const key = keyExtractor(row);
      return deletedIds instanceof Set ? !deletedIds.has(key) : !deletedIds.includes(key);
    });

    const exportRows = visibleData.map((row) => {
      const exportedRow: Record<string, any> = {};

      columns.forEach(col => {
        if (col.id === '__actions') return;

        let val = col.accessor ? col.accessor(row) : (row as any)[col.id];

        // اعمال منطق‌های فرمت‌دهی دقیق شما موقع خروجی گرفتن
        if (col.type === 'select' && col.options) {
          const matchedOption = col.options.find((opt: any) => String(opt.value) === String(val));
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
    XLSX.writeFile(wb, `DataGrid_Export_${new Date().toISOString().slice(0, 10)}.xlsx`);
  }, [instance.rawData, instance.pluginState.editing?.deletedIds, columns, keyExtractor, allowExport]);


  // 🔥 ۲. بازنویسی کامل ایمپورت با قابلیت تطبیق متون کمبوباکس و فعال کردن مد ادیت سطرها
  const handleExcelImport = useCallback((file: File) => {
    if (!allowImport) return;

    const reader = new FileReader();
    reader.onload = (evt) => {
      if (!evt.target?.result) return;
      const bstr = evt.target.result;
      const wb = XLSX.read(bstr, { type: 'binary' });
      const wsname = wb.SheetNames[0];
      const ws = wb.Sheets[wsname];
      const importedRows = XLSX.utils.sheet_to_json<any>(ws);

      // به‌روزرسانی دیتای اصلی گرید
      instance.setRawData((prev) => {
        let updatedList = [...prev];

        // خواندن وضعیت‌های ویرایشی فعلی از افزونه ادیت برای ادغام دیتای جدید
        const currentEditingState = instance.pluginState.editing || {};
        const newAddedKeys = new Set<string | number>(currentEditingState.addedKeys || []);
        const newModifiedKeys = new Set<string | number>(currentEditingState.modifiedKeys || []);
        const newEditingKeys = new Set<string | number>(currentEditingState.editingKeys || []);

        importedRows.forEach((excelRow) => {
          const parsedRow: any = emptyRowFactory ? emptyRowFactory() : {};

          columns.forEach(col => {
            if (col.id === '__actions') return;

            // پیدا کردن کلید متناظر در اکسل بر اساس هدر یا آی‌دی ستون
            const excelKey = Object.keys(excelRow).find(k =>
              k.trim().toLowerCase() === String(col.id).toLowerCase() ||
              k.trim() === col.header
            );

            if (excelKey !== undefined) {
              let rawValue = excelRow[excelKey];

              // تبدیل متن فارسی/انگلیسی اکسل به مقدار valueی متناظر در آپشن‌های کامپوننت
              if (col.type === 'select' && col.options) {
                const matchedOption = col.options.find((opt: any) =>
                  String(opt.value) === String(rawValue) ||
                  String(opt.label).toLowerCase() === String(rawValue).toLowerCase() ||
                  String(opt.display) === String(rawValue)
                );

                if (matchedOption) {
                  rawValue = isNaN(Number(matchedOption.value)) ? matchedOption.value : Number(matchedOption.value);
                }
              }
              
              // بازگرداندن مقادیر چک‌باکس از متون فارسی به بابلین
              if (col.type === 'checkbox') {
                rawValue = rawValue === 'بله' || rawValue === true || String(rawValue).toLowerCase() === 'true';
              }

              parsedRow[col.id] = rawValue;
            }
          });

          const rowKey = keyExtractor(parsedRow, -1);
          const existingIndex = updatedList.findIndex(r => keyExtractor(r) === rowKey);

          if (existingIndex > -1) {
            // سطر از قبل وجود داشته -> دیتای جدید اکسل روی آن اوررایت می‌شود
            updatedList[existingIndex] = { ...updatedList[existingIndex], ...parsedRow };
            if (!newAddedKeys.has(rowKey)) {
              newModifiedKeys.add(rowKey);
            }
          } else {
            // سطر جدید است -> به ابتدای جدول اضافه می‌شود
            updatedList = [parsedRow, ...updatedList];
            newAddedKeys.add(rowKey);
          }
          
          // باز شدن خودکار سطر در وضعیت ویرایش جهت بازبینی کاربر
          newEditingKeys.add(rowKey);
        });

        // هماهنگ‌سازی و تزریق استیت‌های تغییر یافته به افزونه سیستم ویرایش (useGridEditing)
        instance.setPluginState('editing', {
          ...currentEditingState,
          addedKeys: newAddedKeys,
          modifiedKeys: newModifiedKeys,
          editingKeys: newEditingKeys,
          isDirty: newAddedKeys.size > 0 || newModifiedKeys.size > 0 || (currentEditingState.deletedIds?.size > 0),
        });

        return updatedList;
      });
    };
    
    reader.readAsBinaryString(file);
  }, [allowImport, columns, keyExtractor, emptyRowFactory, instance.pluginState.editing]);

  // ثبت اکشن‌ها و استیت افزونه در کانتکست گرید
  useEffect(() => {
    instance.setPluginState('excel', { allowImport: !!allowImport, allowExport: !!allowExport });
    instance.registerAction('excelExport', handleExcelExport);
    instance.registerAction('excelImport', handleExcelImport);
  }, [allowImport, allowExport, handleExcelExport, handleExcelImport]);
}