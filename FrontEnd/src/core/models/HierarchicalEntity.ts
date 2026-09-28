
import { BaseEntity } from "./BaseEntity";
// ─────────────────────────────────────────────────────────────────────
//  قرارداد پایه: هر موجودیت درختی باید این فیلدها رو داشته باشه.
//  منطبق با IHierarchicalStructureEntity در بک‌اند.
// ─────────────────────────────────────────────────────────────────────
export interface HierarchicalEntity extends BaseEntity {
  id: string;
  parentId?: string | null;
}