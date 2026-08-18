 //src/modules/HR/models/EmploymentContactInfoView.ts
 export interface EmploymentContactInfoView {
  id: string; // Guid
  nationalCode: string;
  employmentCode: string;
 firstName: string;
 lastName: string;
  employmentContactPhone?: string[] | null;
  employmentContactMobile?: string[] | null;
}


