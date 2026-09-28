// src/modules/HR/models/postInfoDto.ts

import { HierarchicalEntity } from "@/core/components/treeCrud";
import { LocationInfoView as Location } from "./LocationInfoView";
export interface PostInfoDto extends HierarchicalEntity {
  // id: string;
  // parentId?: string | null;
  postCode: string;
  fkJobTitleId: string;
  fkOrganizationUnitId?: string | null;
  fkJobLevelId?: string | null;
  fkGradeId?: string | null;
  fkCostCenterId?: string | null;
  employmentId?: string | null;
  employmentCode?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  gender?: number | null;
  officePhone?: string | null;
  orgEmail?: string | null;
  orgMobile?: string | null;
  assignmentsAssigneeType?: number | null;
  locations: Location[]; // آرایه
  // در صورت نیاز hrContacts و peopleContacts
}