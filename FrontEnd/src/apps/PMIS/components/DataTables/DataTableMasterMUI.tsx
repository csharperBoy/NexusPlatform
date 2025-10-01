import { useState, useEffect, ChangeEvent } from 'react';
import { DataGrid, type GridColDef, GridToolbar } from '@mui/x-data-grid';
import { saveAs } from 'file-saver';
import * as XLSX from 'xlsx';
import dayjs from 'dayjs';
import jalaliday from 'jalaliday';

dayjs.extend(jalaliday);

export interface Option {
  id: string | number;
  label: string;
}

export interface RowChange<T, ID = any> {
  added: T[];
  updated: T[];
  deleted: ID[];
}

interface Props<T extends { id: any }> {
  data: T[];
  columns: Array<
    Omit<GridColDef, 'field'> & {
      field: keyof T;
      type?: 'string' | 'number' | 'date' | 'singleSelect';
      options?: Option[];
      width?: number | string;
    }
  >;
  onAddDrafts?: () => T[];
  onEdit?: (row: T) => void;
  onDelete?: (id: T['id']) => void;
  onSaveAll?: (changes: RowChange<T>) => void;
}

export default function DataTableMaster<T extends { id: any }>(props: Props<T>) {
  const { data, columns, onAddDrafts, onEdit, 
    // onDelete, 
    onSaveAll } = props;

  const [rows, setRows] = useState<T[]>([]);
  const [addedRows, setAddedRows] = useState<T[]>([]);
  const [updatedRows, setUpdatedRows] = useState<Map<T['id'], T>>(new Map());
  const [deletedIds, setDeletedIds] = useState<Set<T['id']>>(new Set());

  useEffect(() => {
    setRows(data.filter(r => !deletedIds.has(r.id)));
  }, [data, deletedIds]);

  const handleAdd = () => {
    if (!onAddDrafts) return;
    const drafts = onAddDrafts();
    setAddedRows(prev => [...drafts, ...prev]);
    setRows(prev => [...drafts, ...prev]);
  };

  const handleCellEditCommit = (params: any) => {
    const { id, field, value } = params;
    setRows(prev =>
      prev.map(r => (r.id === id ? { ...r, [field]: value } : r))
    );

    const original = data.find(r => r.id === id);
    if (original) {
      const updated = { ...original, ...(rows.find(r => r.id === id) as any), [field]: value } as T;
      setUpdatedRows(prev => new Map(prev).set(id, updated));
      onEdit?.(updated);
    }
  };

  // const handleDelete = (id: T['id']) => {
  //   setDeletedIds(prev => new Set(prev).add(id));
  //   onDelete?.(id);
  // };

  const handleExport = () => {
    const exportData = rows.map(r => {
      const out: any = {};
      columns.forEach(col => {
        const val = r[col.field];
        if (col.type === 'singleSelect' && col.options) {
          const opt = col.options.find(o => o.id === val);
          out[col.headerName || col.field] = opt ? opt.label : '';
        } else if (col.type === 'date' && val) {
          out[col.headerName || col.field] = dayjs(val as any).calendar('jalali').format('YYYY/MM/DD');
        } else {
          out[col.headerName || col.field] = val;
        }
      });
      return out;
    });
    const ws = XLSX.utils.json_to_sheet(exportData);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Sheet1');
    const wbout = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
    saveAs(new Blob([wbout], { type: 'application/octet-stream' }), 'export.xlsx');
  };

  const handleImport = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = evt => {
      const bstr = evt.target?.result;
      const wb = XLSX.read(bstr, { type: 'binary' });
      const sheet = wb.Sheets[wb.SheetNames[0]];
      const json = XLSX.utils.sheet_to_json<any>(sheet);
      const imported: T[] = json.map(row => {
        const obj: any = {};
        columns.forEach(col => {
          const cell = row[col.headerName || col.field];
          if (col.type === 'singleSelect' && col.options) {
            const opt = col.options.find(o => o.label === cell);
            obj[col.field] = opt ? opt.id : null;
          } else if (col.type === 'date' && cell) {
            obj[col.field] = dayjs(cell, 'YYYY/MM/DD').toDate();
          } else {
            obj[col.field] = cell;
          }
        });
        return obj as T;
      });
      setRows(prev => [...imported, ...prev]);
      setAddedRows(prev => [...imported, ...prev]);
    };
    reader.readAsBinaryString(file);
    e.target.value = '';
  };

  const handleSaveAll = () => {
    const changes: RowChange<T> = {
      added: addedRows,
      updated: Array.from(updatedRows.values()),
      deleted: Array.from(deletedIds.values()),
    };
    onSaveAll?.(changes);
    setAddedRows([]);
    setUpdatedRows(new Map());
    setDeletedIds(new Set());
  };

  const muiColumns: GridColDef[] = columns.map(col => {
    const base: Partial<GridColDef> = {
      field: String(col.field),
      headerName: col.headerName || String(col.field),
      width: typeof col.width === 'number' ? col.width : undefined,
      sortable: true,
      filterable: true,
      editable: true,
      type: col.type === 'singleSelect' ? 'singleSelect' : col.type as any,
    };

    if (col.type === 'singleSelect' && col.options) {
      (base as any).valueOptions = col.options.map(o => ({ value: o.id, label: o.label }));
    }

    return base as GridColDef;
  });

  return (
    <div style={{ height: 600, width: '100%' }}>
      <div className="flex gap-2 mb-4">
        <button onClick={handleAdd} className="btn btn-primary">افزودن</button>
        <button onClick={handleExport} className="btn btn-secondary">خروجی اکسل</button>
        <label className="btn btn-accent cursor-pointer">
          بارگذاری از اکسل
          <input type="file" accept=".xlsx,.xls" onChange={handleImport} hidden />
        </label>
        <button onClick={handleSaveAll} className="btn btn-success ml-auto">ثبت کلی</button>
      </div>

      <DataGrid
        {...{
          rows,
          columns: muiColumns,
          getRowId: (row: T) => row.id,
          pagination: true,
          pageSizeOptions: [5, 10, 20],
          editMode: 'cell',
          onCellEditCommit: handleCellEditCommit,
          slots: { toolbar: GridToolbar },
        } as any}
      />
    </div>
  );
}

