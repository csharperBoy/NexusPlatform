// src/components/DataTables/DataTable.tsx
import React from 'react';
import { GridColDef } from '@mui/x-data-grid';
import BaseDataTable from './BaseDataTable';

interface DataTableProps {
  columns: GridColDef[];
  rows: object[];
  slug: string;
  includeActionColumn: boolean;
}

const DataTable: React.FC<DataTableProps> = ({
  columns,
  rows,
  slug,
  includeActionColumn,
}) => {
  return (
    <BaseDataTable
      columns={columns}
      rows={rows}
      slug={slug}
      includeActionColumn={includeActionColumn}
      // پیکربندی‌های ثابت DataTable معمولی:
      getRowHeight={() => 'auto'}
      initialState={{
        pagination: { paginationModel: { pageSize: 10 } },
      }}
      pageSizeOptions={[5]}
      checkboxSelection
      disableRowSelectionOnClick
      disableColumnFilter
      disableDensitySelector
      disableColumnSelector
      // از تول‌بار پیش‌فرض (GridToolbar) استفاده می‌کند
    />
  );
};

export default DataTable;
