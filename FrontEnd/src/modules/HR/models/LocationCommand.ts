// models/LocationCommand.ts



export interface UpdateLocationCommand {
  id: string; // Guid
  title: string | null;
  officePhone?: string | null;
  orgEmail?: string | null;
  orgMobile?: string | null;
}


