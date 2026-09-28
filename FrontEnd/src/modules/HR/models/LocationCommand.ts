// models/LocationCommand.ts
export interface UpdateLocationCommand {
  id: string; // Guid
  title: string | null;
  parentId?:string | null;
}
export interface CreateLocationCommand {
  title: string | null;
  parentId?:string | null;
}

