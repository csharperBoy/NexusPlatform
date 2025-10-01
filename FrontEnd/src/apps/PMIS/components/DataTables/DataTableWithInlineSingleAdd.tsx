// src/components/DataTables/DataTableWithInlineSingleAdd.tsx

import React, { useState, useCallback, useEffect } from 'react';
import {
  GridColDef,
  GridRowModel,
  GridRowModes,
  GridRowModesModel,
  // GridCellParams,
  // GridRowParams,
} from '@mui/x-data-grid';
import { useNavigate } from 'react-router-dom';
import {
  HiOutlineCheck,
  HiXMark,
  HiOutlineEye,
  HiOutlinePencilSquare,
  HiOutlineTrash,
} from 'react-icons/hi2';
import toast from 'react-hot-toast';
import BaseDataTable from './BaseDataTable';

interface DataTableWithInlineAddProps {
  columns: GridColDef[];
  rows: GridRowModel[];
  slug: string;
  includeActionColumn: boolean;
  onAddRow?: (data: any) => Promise<void>;
  fetchNewRowTemplate?: () => Promise<GridRowModel>; // ✅ تابع جدید
}

const DataTableWithInlineSingleAdd: React.FC<DataTableWithInlineAddProps> = ({
  columns,
  rows,
  slug,
  includeActionColumn,
  onAddRow,
  fetchNewRowTemplate,
}) => {
  const navigate = useNavigate();
  const editableColumns = columns.map(col => ({ ...col, editable: true }));
  const [localRows, setLocalRows] = useState<GridRowModel[]>(rows);
  const [isAdding, setIsAdding] = useState(false);
  const [newRowId, setNewRowId] = useState(-1);
  const [rowModesModel, setRowModesModel] = useState<GridRowModesModel>({});

  useEffect(() => setLocalRows(rows), [rows]);

  const CustomToolbar = () => (
    <div className="flex justify-between items-center p-2">
      <button
        className="btn btn-primary btn-sm"
        onClick={async () => {
          if (isAdding) return;

          let newRow: GridRowModel = { id: newRowId };

          if (fetchNewRowTemplate) {
            try {
              const result = await fetchNewRowTemplate();
              newRow = { ...result, id: newRowId };
            } catch (err) {
              toast.error('خطا در دریافت ردیف پیش‌فرض از سرور');
              return;
            }
          } else {
            editableColumns.forEach(col => {
              if (col.field !== 'id') newRow[col.field] = '';
            });
          }

          setLocalRows(prev => [newRow, ...prev]);
          setRowModesModel(prev => ({
            ...prev,
            [newRowId]: { mode: GridRowModes.Edit },
          }));
          setIsAdding(true);
          setNewRowId(prev => prev - 1);
        }}
      >
        + افزودن
      </button>
    </div>
  );

  const processRowUpdate = useCallback(
    async (newRow: GridRowModel, oldRow: GridRowModel) => {
      if (isAdding && newRow.id === oldRow.id) {
        setLocalRows(prev =>
          prev.map(r => (r.id === newRow.id ? newRow : r))
        );
        setIsAdding(false);

        if (onAddRow) {
          try {
            await onAddRow(newRow);
            toast.success('افزوده شد!');
          } catch (error) {
            toast.error('خطا در افزودن!');
          }
        } else {
          toast.success('به صورت محلی افزوده شد');
        }
      }
      return newRow;
    },
    [isAdding, onAddRow]
  );

  const handleRowModesModelChange = (newModel: GridRowModesModel) =>
    setRowModesModel(newModel);

  const handleProcessRowUpdateError = useCallback((error: Error) => {
    console.error(error);
    toast.error('خطا در بروزرسانی سطر');
  }, []);

  const renderInlineActions = ({ id }: { id: any }) => {
    const isEditing = rowModesModel[id]?.mode === GridRowModes.Edit;
    if (isEditing) {
      return [
        <button
          key="save"
          onClick={() =>
            setRowModesModel(prev => ({
              ...prev,
              [id]: { mode: GridRowModes.View },
            }))
          }
          className="btn btn-ghost btn-sm mr-2"
        >
          <HiOutlineCheck />
        </button>,
        <button
          key="cancel"
          onClick={() => {
            setRowModesModel(prev => ({
              ...prev,
              [id]: { mode: GridRowModes.View, ignoreModifications: true },
            }));
            if (isAdding) {
              setLocalRows(prev => prev.filter(r => r.id !== id));
              setIsAdding(false);
            }
          }}
          className="btn btn-ghost btn-sm"
        >
          <HiXMark />
        </button>,
      ];
    }
    return [
      <button
        key="view"
        onClick={() => navigate(`/${slug}/${id}`)}
        className="btn btn-ghost btn-sm mr-2"
      >
        <HiOutlineEye />
      </button>,
      <button
        key="edit"
        onClick={() =>
          setRowModesModel(prev => ({
            ...prev,
            [id]: { mode: GridRowModes.Edit },
          }))
        }
        className="btn btn-ghost btn-sm mr-2"
      >
        <HiOutlinePencilSquare />
      </button>,
      <button
        key="delete"
        onClick={() => toast('حذف مجاز نیست', { icon: '😠' })}
        className="btn btn-ghost btn-sm"
      >
        <HiOutlineTrash />
      </button>,
    ];
  };

  return (
    <BaseDataTable
      columns={editableColumns}
      rows={localRows}
      slug={slug}
      includeActionColumn={includeActionColumn}
      renderActionCell={renderInlineActions}
      ToolbarComponent={CustomToolbar}
      editMode="row"
      processRowUpdate={processRowUpdate}
      onProcessRowUpdateError={handleProcessRowUpdateError}
      rowModesModel={rowModesModel}
      onRowModesModelChange={handleRowModesModelChange}
      // onCellDoubleClick={(params: GridCellParams, event) => event.stopPropagation()}
      // onRowDoubleClick={(params: GridRowParams, event) => event.stopPropagation()}
      initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
      pageSizeOptions={[5]}
      checkboxSelection
      disableRowSelectionOnClick
      disableColumnFilter
      disableDensitySelector
      disableColumnSelector
    />
  );
};

export default DataTableWithInlineSingleAdd;
