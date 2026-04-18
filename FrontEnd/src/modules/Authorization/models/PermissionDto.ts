// src/modules/Authorization/models/PermissionDto.ts
export interface PermissionDto {
  id: string;
  AssigneeType: string;
  AssigneeId: string;
  description: string;
  ResourceId?: string;
  ResourceKey?: string;
  Action: number;
  Effect: number;
  EffectiveFrom: Date;
  ExpiresAt: Date;
  isActive: boolean;
} 
 
