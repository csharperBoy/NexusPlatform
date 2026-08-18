import { Location } from "./LocationInfoView";

 //src/modules/HR/models/EmploymentInfoView.ts
 export interface EmploymentInfoView {
  id: string; // Guid
  nationalCode: string;
  employmentCode: string;
 firstName: string;
 lastName: string;
  employmentContactPhone?: string | null;
  employmentContactMobile?: string | null;
  
    locations: Location[]; // آرایه
}


