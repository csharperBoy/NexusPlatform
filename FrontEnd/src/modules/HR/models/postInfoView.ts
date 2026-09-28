// models/postInfoView.ts

import { HierarchicalEntity } from "@/core/components/treeCrud";

export interface PostInfoView extends HierarchicalEntity{
  // id: string;
  // parentId?: string | null;
  postCode: string;  
  fkJobTitleId?: string | null;
  fkOrganizationUnitId?: string | null;
  fkJobLevelId?: string | null;
  fkGradeId?: string | null;
  fkCostCenterId?: string | null;
  costCenterName?: string | null;
  gradeTitle?: string | null;
  jobLevelTitle?: string | null;
  jobTitleName?: string | null;
  officePhone?: string | null;
  orgMobile?: string | null;
  orgEmail?: string | null;
  employmentId?: string | null;
  employmentCode?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  nationalCode?: string | null;
  gender?: number | null;
  assignmentsAssigneeType?: number | null;
  organizationUnitsName: string;

  locationsEffectiveFrom?: Date | null;

 locationsEffectiveTo?:Date| null;

locationTitle?: string| null;
 locationId?:string| null;
}