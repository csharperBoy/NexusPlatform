import { LogicalOperator ,ComparisonOperator } from "./PermissionRuleEnum";

// src/modules/Authorization/models/PermissionRuleDto.ts
export interface PermissionRuleDto {
  id: string;
  permissionId: string;
  fieldName?: string;
  joinDetailId?: string;
  operator: ComparisonOperator;
  value: string;
  logicalOperator: LogicalOperator;
  groupOrder: number;

   joinLocalKey: string;
  joinForeignKey: string;
  joinEntity: string;
} 
 
