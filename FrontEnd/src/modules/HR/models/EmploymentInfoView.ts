import {LocationInfoView as Location } from "./LocationInfoView";

 //src/modules/HR/models/EmploymentInfoView.ts
 export interface EmploymentInfoView {
  id: string; // Guid
    
profileId?: string | null ; // Guid
partyProfileId?: string | null; // Guid

nationalCode?: string | null;
firstName?: string | null;
lastName?: string | null;

employmentCode?: string | null;
employmentEffectiveFrom?: Date | null;
employmentEffectiveTo?: Date | null;
partyId?: string | null;
employmentStatusName?: string | null;
employmentTypeName?: string | null;
assignmentsAssigneeType?: number | null;
assignmentsEffectiveFrom?: Date | null;
assignmentsEffectiveTo?: Date | null;
postCode?: string | null;
gradeTitle?: string | null;
costCenterName?: string | null;
jobLevelTitle?: string | null;
jobTitleName?: string | null;
organizationUnitsName?: string | null;
locations?: Location[] | null; // آرایه
}


