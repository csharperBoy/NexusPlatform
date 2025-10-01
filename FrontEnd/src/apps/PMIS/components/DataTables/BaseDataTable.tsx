import React, { useContext, useMemo } from 'react';
import {
  DataGrid,
  GridColDef,
  GridToolbar,
  GridToolbarProps,
  DataGridProps,
} from '@mui/x-data-grid';
import { useNavigate } from 'react-router-dom';
import {
  HiOutlineEye,
  HiOutlinePencilSquare,
  HiOutlineTrash,
} from 'react-icons/hi2';
import toast from 'react-hot-toast';
import ThemeContext from '../../contexts/ThemeContext';

export interface BaseDataTableProps
  extends Omit<DataGridProps, 'columns' | 'rows' | 'slots'> {
  columns: GridColDef[];
  rows: any[];
  slug?: string;
  includeActionColumn?: boolean;
  renderActionCell?: (params: any) => React.ReactNode;
  ToolbarComponent?: React.ComponentType<GridToolbarProps>;
}

const BaseDataTable: React.FC<BaseDataTableProps> = ({
  columns,
  rows,
  slug,
  includeActionColumn = false,
  renderActionCell,
  ToolbarComponent,
  ...gridProps
}) => {
  const navigate = useNavigate();
  const themeContext = useContext(ThemeContext);
  const currentTheme = themeContext?.theme || 'light';

  const defaultRenderActionCell = (params: any) => (
    <div className="flex items-center">
      <button
        onClick={() => slug && navigate(`/${slug}/${params.row.id}`)}
        className="btn btn-square btn-ghost"
      >
        <HiOutlineEye />
      </button>
      <button
        onClick={() => toast('ویرایش مجاز نیست!', { icon: '😠' })}
        className="btn btn-square btn-ghost"
      >
        <HiOutlinePencilSquare />
      </button>
      <button
        onClick={() => toast('حذف مجاز نیست!', { icon: '😠' })}
        className="btn btn-square btn-ghost"
      >
        <HiOutlineTrash />
      </button>
    </div>
  );

  const actionColumn: GridColDef = useMemo(
    () => ({
      field: 'action',
      headerName: 'عملیات‌ها',
      minWidth: 200,
      flex: 1,
      renderCell: renderActionCell ?? defaultRenderActionCell,
    }),
    [renderActionCell, slug]
  );

  const finalColumns = useMemo(
    () => (includeActionColumn ? [...columns, actionColumn] : columns),
    [columns, includeActionColumn, actionColumn]
  );

  const MemoizedToolbar = useMemo(() => ToolbarComponent ?? GridToolbar, [ToolbarComponent]);

  return (
    <div className="w-full bg-base-100 text-base-content overflow-x-auto">
   
      <DataGrid
        rows={rows}
        columns={finalColumns}
        slots={{
          toolbar: MemoizedToolbar,
          noRowsOverlay: () => (
            <div className="text-gray-500 p-4">داده‌ای برای نمایش وجود ندارد.</div>
          ),
        }}
        sx={{
          backgroundColor: currentTheme === 'dark' ? '#1f1f1f' : '#ffffff',
          color: currentTheme === 'dark' ? '#e5e7eb' : '#1e293b',
          border: 0,
          minHeight: 200,
          height: 500,
          '.MuiDataGrid-cell': {
            borderBottom: `1px solid ${currentTheme === 'dark' ? '#333' : '#ddd'}`,
          },
          '.MuiDataGrid-columnHeaders': {
            backgroundColor: currentTheme === 'dark' ? '#2d2d2d' : '#f3f4f6',
            color: currentTheme === 'dark' ? '#f9fafb' : '#1f2937',
            fontWeight: 600,
          },
          '.MuiDataGrid-toolbarContainer': {
            backgroundColor: currentTheme === 'dark' ? '#2a2a2a' : '#f9fafb',
          },
          '.MuiDataGrid-footerContainer': {
            backgroundColor: currentTheme === 'dark' ? '#2d2d2d' : '#f3f4f6',
            borderTop: `1px solid ${currentTheme === 'dark' ? '#444' : '#e5e7eb'}`,
            direction: 'ltr',
          },
          width: 'max-content',
        }}
        {...gridProps}
      />
    </div>
  );
};

export default React.memo(BaseDataTable);
