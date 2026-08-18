// models/EmploymentCommand.ts



export interface UpdateEmploymentCommand {
  id: string; // Guid
  
  officePhone?: string | null;
  orgEmail?: string | null;
  orgMobile?: string | null;
}


export interface CreateEmploymentCommand {
  id: string; // Guid
  
  officePhone?: string | null;
  orgEmail?: string | null;
  orgMobile?: string | null;
}
