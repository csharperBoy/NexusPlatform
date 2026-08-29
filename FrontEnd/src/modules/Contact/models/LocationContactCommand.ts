// models/LocationContactCommand.ts

export interface UpdateLocationContactCommand {
  id: string; // Guid
  officePhone?: string[] | null;
  orgMobile?: string[] | null;
}


