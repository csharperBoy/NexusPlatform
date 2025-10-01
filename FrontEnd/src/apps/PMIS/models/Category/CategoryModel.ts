
export interface CategoryModel {
  id?: string;
  FklkpTypeId: number;
  FkParentId?: string | null;
  Code?: string;
  Title?: string;
  OrderNum: number;
  Description?: string;
}