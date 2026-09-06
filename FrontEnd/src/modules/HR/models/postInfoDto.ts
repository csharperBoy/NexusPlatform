// src/modules/HR/models/postInfoDto.ts

import { LocationInfoView as Location } from "./LocationInfoView";
export interface PostInfoDto {
  id: string;
  postCode: string;
  parentId?: string | null;
  fkParentId?: string | null;
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