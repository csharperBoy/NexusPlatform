
// src/modules/Authorization/models/PermissionCommands.ts

export interface CreatePermissionCommand {
  ResourceId: string;
  AssigneeId: string;
  AssigneeType: number;       
  Action: number;  
  effect: number;
  EffectiveFrom?: Date | null;
  ExpiresAt?: Date | null;
  IsActive?: boolean;
  Description?: string;
  scopes?: number[] | null;
}

export type UpdatePermissionCommand = {
  Id: string;
  ResourceId?: string | null;
  AssigneeId?: string | null;
   AssigneeType?: number|null;   
   Action?: number|null;     
   effect?: number|null;
} & Partial<CreatePermissionCommand>;



export type PermissionFormCommand = CreatePermissionCommand | UpdatePermissionCommand;