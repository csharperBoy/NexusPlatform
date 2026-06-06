
// src/modules/Authorization/models/PermissionCommands.ts

import { Action, AssignType, Effect, Scope } from "./PermissionEnum";
import { CreatePermissionRuleCommand, PermissionRuleFormCommand, UpdatePermissionRuleCommand } from "./PermissionRuleCommands";

export interface CreatePermissionCommand {
  ResourceId: string;
  AssigneeId: string;
  AssigneeType: AssignType;       
  Action: Action;  
  effect: Effect;
  EffectiveFrom?: Date | null;
  ExpiresAt?: Date | null;
  IsActive?: boolean;
  Description?: string;
  scopes?: Scope[] | null;
  rules? : CreatePermissionRuleCommand[] | null;
}

export type UpdatePermissionCommand = {
  Id: string;
  ResourceId?: string | null;
  AssigneeId?: string | null;
  AssigneeType?: AssignType | null;
  Action?: Action | null;
  effect?: Effect | null;
  EffectiveFrom?: Date | null;
  ExpiresAt?: Date | null;
  IsActive?: boolean | null;
  Description?: string | null;
  scopes?: Scope[] | null;
  rules?: CreatePermissionRuleCommand[]; 
} & Partial<CreatePermissionCommand>;



export type PermissionFormCommand = CreatePermissionCommand | UpdatePermissionCommand;