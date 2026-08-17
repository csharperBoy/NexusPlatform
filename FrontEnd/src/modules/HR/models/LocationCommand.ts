// models/LocationCommand.ts
export interface UpdateLocationCommand {
  id: string; // Guid
  title: string | null;
}
export interface CreateLocationCommand {
  title: string | null;
  
}

