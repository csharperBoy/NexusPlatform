// models/postContactCommand.ts


export interface UpdatePostContactCommand {
  id: string; // Guid
 
  officePhone?: string | null;
  orgEmail?: string | null;
  orgMobile?: string | null;
}


