// models/postContactInfoView.ts

import { HierarchicalEntity } from "@/core/models/HierarchicalEntity";

export interface PostContactInfoView extends HierarchicalEntity{
  // id: string;
  // parentId?: string | null;
  postCode: string;
  costCenterName?: string | null;
  gradeTitle?: string | null;
  jobLevelTitle?: string | null;
  jobTitleName?: string | null;
  employmentCode?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  nationalCode?: string | null;
  gender?: number | null;
  organizationUnitsName: string;

  officePhone?: string[] | null;
  orgMobile?: string[] | null;
  orgEmail?: string[] | null;
}