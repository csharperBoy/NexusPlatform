import { JoinDetailDto } from "./JoinDetailDto";

// src/modules/Authorization/models/PermissionRuleDto.ts
export interface PermissionRuleDto {
  id: string;
  permissionId: string;
  fieldName?: string;
  joinDetailId?: string;
  operator: number;
  value: string;
  logicalOperator: number;
  groupOrder: number;
  // joinDetail: JoinDetailDto | null;

   joinLocalKey: string;
  joinForeignKey: string;
  joinEntity: string;
} 
 
