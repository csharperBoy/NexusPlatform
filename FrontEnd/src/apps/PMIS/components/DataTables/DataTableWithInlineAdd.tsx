import React, { useCallback, useState, memo } from 'react';
import type { ExtendedGridColDef } from '../../models/ExtendedGridColDef';
import { matchOptionLabel } from '../../utils/optionUtils';

import {
  GridColDef,
  GridRowModel,
  GridRowModes,
  GridRowModesModel,
} from '@mui/x-data-grid';
import DateObject from "react-date-object";
import persian from "react-date-object/calendars/persian";
import persian_fa from "react-date-object/locales/persian_fa";

import toast from 'react-hot-toast';
import * as XLSX from 'xlsx';
import {
  HiOutlineCheck,
  HiXMark,
  // HiOutlineEye,
  HiOutlinePencilSquare,
  HiOutlineTrash,
  HiOutlineArrowDownTray,
  HiOutlineArrowUpTray,
} from 'react-icons/hi2';
import { HiPlus } from 'react-icons/hi2';
import { FiSave } from 'react-icons/fi';
import BaseDataTable from './BaseDataTable';

interface DataTableWithInlineAddProps {
  columns: GridColDef[];
  rows: GridRowModel[];
  rowModesModel: GridRowModesModel;
  slug: string;
  includeActionColumn?: boolean;
  onRowsChange: (rows: GridRowModel[]) => void;
  onRowModesModelChange: (model: GridRowModesModel) => void;
  onAddRow?: (data: GridRowModel) => Promise<void>;
  fetchNewRowTemplate?: () => Promise<GridRowModel[]>;
  enableExcel?: boolean;
  onExcelImport?: (rows: GridRowModel[]) => void;
  exportFileName?: string;
  onSaveAll?: (rows: GridRowModel[]) => Promise<void>;
  onDeleteRow?: (id: number | string) => Promise<void>;
}

const ToolbarButtons = memo(({ onAdd, onExcelImport, onExcelExport, onSaveAll, enableExcel }: any) => (
  <div className="mb-2">
    <div className="w-full flex items-center justify-between">
      {/* Action icons on the left (icon-only, compact) */}
      <div className="flex items-center gap-2">
        <button title="افزودن" className="btn btn-sm btn-primary btn-circle text-lg" onClick={onAdd}><HiPlus className="text-xl"/></button>

        {enableExcel && (
          <>
            <label title="بارگذاری اکسل" className="btn btn-sm btn-ghost btn-circle cursor-pointer text-lg" htmlFor="excel-import">
              <HiOutlineArrowDownTray className="text-xl" />
            </label>
            <input id="excel-import" type="file" accept=".xlsx, .xls" onChange={onExcelImport} className="hidden" />
            <button title="خروجی اکسل" className="btn btn-sm btn-ghost btn-circle text-lg" onClick={onExcelExport}>
              <HiOutlineArrowUpTray className="text-xl"/></button>
          </>
        )}
      </div>

      {/* Save all on the right (icon-only) */}
      <div className="flex items-center">
        {onSaveAll && (
          <button
            title="ثبت نهایی"
            onClick={onSaveAll}
            className="btn btn-sm btn-success btn-circle text-lg"
          >
            <FiSave className="text-xl" />
          </button>
        )}
      </div>
    </div>
  </div>
));

const DataTableWithInlineAdd: React.FC<DataTableWithInlineAddProps> = ({
  columns,
  rows,
  rowModesModel,
  slug,
  includeActionColumn = true,
  onRowsChange,
  onRowModesModelChange,
  onAddRow,
  fetchNewRowTemplate,
  enableExcel = false,
  onExcelImport,
  exportFileName = 'data.xlsx',
  onSaveAll,
  onDeleteRow,
}) => {
  const [newRowId, setNewRowId] = useState(-1);

  const handleAdd = useCallback(async () => {
    try {
      const newRows = fetchNewRowTemplate ? await fetchNewRowTemplate() : [{ id: newRowId }];
      const preparedRows = newRows.map((r, idx) => (
        { 
          ...r, 
          id: newRowId - idx ,
          hasChanges: true,
        }));
      const newModes = preparedRows.reduce((acc, row) => ({ ...acc, [row.id]: { mode: GridRowModes.Edit } }), {});

      onRowsChange([...preparedRows, ...rows]);
      onRowModesModelChange({ ...rowModesModel, ...newModes });
      setNewRowId(prev => prev - preparedRows.length);
    } catch {
      toast.error('خطا در دریافت داده جدید');
    }
  }, [fetchNewRowTemplate, newRowId, onRowsChange, rows, rowModesModel, onRowModesModelChange]);

  const handleExcelImport = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();

    reader.onload = ev => {
      const data = new Uint8Array(ev.target?.result as ArrayBuffer);
      const workbook = XLSX.read(data, { type: 'array' });
      const sheet = workbook.Sheets[workbook.SheetNames[0]];
      const json: any[] = XLSX.utils.sheet_to_json(sheet);

      const mappedRows = json.map((item, idx) => {
        const row: any = { 
          id: `import-${Date.now()}-${idx}`,
          hasChanges: true,
         };
         (columns as ExtendedGridColDef[]).forEach(col => {
        // columns.forEach(col => {
          const rawValue = item[col.headerName || col.field];

          let value = rawValue;

          // اگر این فیلد، فیلد تاریخ باشه (مثلاً بر اساس نام فیلد یا هدرش)
          if (col.field === 'dateTime' && typeof rawValue === 'string') {
            // تبدیل اعداد فارسی به انگلیسی
            const faToEnDigits = rawValue.replace(/[۰-۹]/g, (d) => '۰۱۲۳۴۵۶۷۸۹'.indexOf(d).toString());

            try {
              const dateObject = new DateObject({
                date: faToEnDigits,
                format: "YYYY/MM/DD",
                calendar: persian,
                locale: persian_fa,
              });
              value = dateObject.toDate(); // میلادی معادل
            } catch {
              value = null;
            }
          }

          if (col.type === 'singleSelect' && col.valueOptions) {
              row[col.field] = matchOptionLabel(value, col.valueOptions, { field: col.field, debug: true });
            } else {
              row[col.field] = value;
          }

        });

        return row;
      });

      const editModes = mappedRows.reduce((acc, row) => ({ ...acc, [row.id]: { mode: GridRowModes.Edit } }), {});

      onExcelImport ? onExcelImport(mappedRows) : onRowsChange([...mappedRows, ...rows]);
      onRowModesModelChange({ ...rowModesModel, ...editModes });

      toast.success('اکسل بارگذاری شد');
    };

    reader.readAsArrayBuffer(file);
  }, [columns, onExcelImport, onRowsChange, rows, rowModesModel, onRowModesModelChange]);

  const handleExcelExport = useCallback(() => {
    const exportData = rows.map(row => {
      const obj: Record<string, any> = {};
      columns.forEach(col => {
        const val = col.valueFormatter ? col.valueFormatter({ value: row[col.field], field: col.field, api: {} as any }) : row[col.field];
        obj[col.headerName || col.field] = val;
      });
      return obj;
    });

    const ws = XLSX.utils.json_to_sheet(exportData);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Sheet1');
    XLSX.writeFile(wb, exportFileName);
  }, [columns, rows, exportFileName]);

  return (
    <div>
      <ToolbarButtons
        onAdd={handleAdd}
        onExcelImport={handleExcelImport}
        onExcelExport={handleExcelExport}
        onSaveAll={onSaveAll ? () => onSaveAll(rows) : undefined}
        enableExcel={enableExcel}
      />

      <BaseDataTable
        columns={columns}
        rows={rows}
        slug={slug}
        includeActionColumn={includeActionColumn}
          renderActionCell={({ id }) => {
          const isEditing = rowModesModel[id]?.mode === GridRowModes.Edit;
          return isEditing ? (
            <div className="flex items-center gap-2">
              <button title="ذخیره" className="btn btn-success btn-sm" onClick={() => onRowModesModelChange({ ...rowModesModel, [id]: { mode: GridRowModes.View } })}><HiOutlineCheck /></button>
              <button title="انصراف" className="btn btn-error btn-sm" onClick={() => { onRowModesModelChange({ ...rowModesModel, [id]: { mode: GridRowModes.View, ignoreModifications: true } }); onRowsChange(rows.filter(r => r.id !== id)); }}><HiXMark /></button>
            </div>
          ) : (
            <div className="flex items-center gap-2">
              <button title="ویرایش" className="btn btn-ghost btn-sm" onClick={() => onRowModesModelChange({ ...rowModesModel, [id]: { mode: GridRowModes.Edit } })}><HiOutlinePencilSquare /></button>
              <button title="حذف" className="btn btn-ghost btn-sm text-error" onClick={() => onDeleteRow && onDeleteRow(id)}><HiOutlineTrash /></button>
            </div>
          );
        }}
        editMode="row"
        // processRowUpdate={async (newRow, oldRow) => {
        //   const updatedRows = rows.map(r => (r.id === newRow.id ? newRow : r));
        //   onRowsChange(updatedRows);
        //   if (onAddRow) await onAddRow(newRow);
        //   onRowModesModelChange({ ...rowModesModel, [newRow.id]: { mode: GridRowModes.View } });
        //   return newRow;
        // }}
        processRowUpdate={async (newRow) => {
            // تنظیم فلگ تغییر
            const updatedRow = { ...newRow, hasChanges: true };

            // اعمال تغییرات در لیست رکوردها
            const updatedRows = rows.map(r => (r.id === updatedRow.id ? updatedRow : r));

            // به‌روزرسانی وضعیت جدول
            onRowsChange(updatedRows);

            // ذخیره‌سازی اگر نیاز باشه
            if (onAddRow) await onAddRow(updatedRow);

            // تغییر حالت ردیف به View
            onRowModesModelChange({ ...rowModesModel, [updatedRow.id]: { mode: GridRowModes.View } });

            return updatedRow;
          }}

        onProcessRowUpdateError={() => toast.error('خطا در بروزرسانی')}
        rowModesModel={rowModesModel}
        onRowModesModelChange={onRowModesModelChange}
        initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
        pageSizeOptions={[5, 10, 20]}
        // checkboxSelection        
        disableRowSelectionOnClick
      />
    </div>
  );
};

export default DataTableWithInlineAdd;
