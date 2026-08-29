// models/EmploymentContactCommand.ts



export interface UpdateEmploymentContactCommand {
  id: string; // Guid
  
  officePhones?: string[] | null;
  orgEmails?: string[] | null;
  orgMobiles?: string[] | null;
}


