
// src/modules/Authorization/models/PermissionCommands.ts

import { Action, AssignType, Effect, Scope } from "./PermissionEnum";
import { CreatePermissionRuleCommand, PermissionRuleFormCommand, UpdatePermissionRuleCommand } from "./PermissionRuleCommands";

export interface CreatePermissionCommand {
  resourceId: string;
  assigneeId: string;
  assigneeType: AssignType;       
  action: Action;  
  effect: Effect;
  effectiveFrom?: Date | null;
  expiresAt?: Date | null;
  isActive?: boolean;
  description?: string;
  scopes?: Scope[] | null;
  rules? : CreatePermissionRuleCommand[] | null;
}

export type UpdatePermissionCommand = {
  id: string;
  resourceId?: string | null;
  assigneeId?: string | null;
  assigneeType?: AssignType | null;
  action?: Action | null;
  effect?: Effect | null;
  effectiveFrom?: Date | null;
  expiresAt?: Date | null;
  isActive?: boolean | null;
  description?: string | null;
  scopes?: Scope[] | null;
  rules?: CreatePermissionRuleCommand[]; 
} & Partial<CreatePermissionCommand>;



export type PermissionFormCommand = CreatePermissionCommand | UpdatePermissionCommand;