// src/modules/Authorization/models/PermissionDto.ts
export interface PermissionDto {
  id: string;
  assigneeType: number;
  assigneeId: string;
  description: string;
  resourceId?: string;
  resourceKey?: string;
  action: number;
  effect: number;
  effectiveFrom: Date;
  expiresAt: Date;
  isActive: boolean;
  scopes: number[];
} 
 
