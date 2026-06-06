import { Action, AssignTyp, Effect, Scope } from "./PermissionEnum";
import { PermissionRuleDto } from "./PermissionRuleDto";

// src/modules/Authorization/models/PermissionDto.ts
export interface PermissionDto {
  id: string;
  assigneeType: AssignTyp;
  assigneeId: string;
  description: string;
  resourceId?: string;
  resourceKey?: string;
  action: Action;
  effect: Effect;
  effectiveFrom: Date;
  expiresAt: Date;
  isActive: boolean;
  scopes: Scope[];
  
  rules: PermissionRuleDto[]; 
} 
 
