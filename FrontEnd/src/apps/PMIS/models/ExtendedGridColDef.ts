import { GridColDef } from '@mui/x-data-grid';

export type ExtendedGridColDef = GridColDef & {
  valueOptions?: any[];
};
