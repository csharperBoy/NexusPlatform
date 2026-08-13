// models/EmploymentContactCommand.ts



export interface UpdateEmploymentContactCommand {
  id: string; // Guid
  
  officePhone?: string | null;
  orgEmail?: string | null;
  orgMobile?: string | null;
}


