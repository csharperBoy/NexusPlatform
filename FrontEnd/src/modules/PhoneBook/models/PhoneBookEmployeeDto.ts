// models/postInfoView.ts

export interface PostInfoView {
  id: string;
  postCode: string;
  // parentId?: string | null;
  fkParentId?: string | null;
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
  employeeCode?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  nationalCode?: string | null;
  gender?: number | null;
  assignmentsAssigneeType?: number | null;
  organizationUnitsName: string;
}