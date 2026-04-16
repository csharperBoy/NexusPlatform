// src/modules/Identity/models/UpdateRoleCommand.ts
export interface UpdateRoleCommand {
  Id: string;
  Name?: string | null;
  Description?: string | null;
  OrderNum?: number | null;
}
  