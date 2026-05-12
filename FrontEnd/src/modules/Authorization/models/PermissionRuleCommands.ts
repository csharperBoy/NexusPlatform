// src/modules/Authorization/models/PermissionRuleCommands.ts
import { ComparisonOperator ,LogicalOperator} from "./PermissionRuleEnum";


export interface CreatePermissionRuleCommand {
    fieldName?: string;             
  joinDetailId?: string;          
  operator?: ComparisonOperator; 
  value?: string;               
  logicalOperator?: LogicalOperator;
  groupOrder?: number;           
  permissionId: string;     
}

export type UpdatePermissionRuleCommand = {
  id: string;
   fieldName?: string | null;
  joinDetailId?: string | null;
  operator?: ComparisonOperator | null;
  value?: string | null;
  logicalOperator?: LogicalOperator | null;
  groupOrder?: number | null;
  permissionId?: string | null;    
} & Partial<CreatePermissionRuleCommand>;



export type PermissionRuleFormCommand = CreatePermissionRuleCommand | UpdatePermissionRuleCommand;