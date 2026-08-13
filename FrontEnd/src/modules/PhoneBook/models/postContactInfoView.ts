// models/postContactInfoView.ts

export interface PostContactInfoView {
  id: string;
  postCode: string;
  fkParentId?: string | null;
  
  costCenterName?: string | null;
  gradeTitle?: string | null;
  jobLevelTitle?: string | null;
  jobTitleName?: string | null;
  officePhone?: string | null;
  orgMobile?: string | null;
  orgEmail?: string | null;
  employmentCode?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  nationalCode?: string | null;
  gender?: number | null;
  organizationUnitsName: string;
}