// src/core/components/Generic/core/types.ts
import { CrudDataSlice } from '../features/page/data/dataSlice';


export interface SelectionListDto {
  value: string;
  label: string;
  display: string;
}

export interface GenericCrudApi<TDto, TCreateCmd, TUpdateCmd, TSearchReq> {
  create: (cmd: TCreateCmd) => Promise<TDto>;
  batchCreate: (cmds: TCreateCmd[]) => Promise<TDto[]>;
  update: (cmd: TUpdateCmd) => Promise<TDto>;
  batchUpdate: (cmds: TUpdateCmd[]) => Promise<TDto[]>;
  delete: (id: string) => Promise<void>;
  batchDelete: (ids: string[]) => Promise<void>;
  getList: () => Promise<TDto[]>;
  search: (req: TSearchReq) => Promise<TDto[]>;
  getSelectionList: () => Promise<SelectionListDto[]>;
}

export interface CrudCoreState<TDto, TCreateCmd, TUpdateCmd, TSearchReq> {
  api: GenericCrudApi<TDto, TCreateCmd, TUpdateCmd, TSearchReq>;
}

  // اضافه شدن CrudDataSlice به تقاطع (Intersection) تایپ‌ها
export type CrudStore<TDto, TCreateCmd, TUpdateCmd, TSearchReq> = 
  CrudCoreState<TDto, TCreateCmd, TUpdateCmd, TSearchReq> & 
  CrudDataSlice<TDto, TSearchReq>;
  