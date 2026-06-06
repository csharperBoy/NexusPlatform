// src/modules/Authorization/models/PermissionRuleCommands.ts
import { ComparisonOperator ,LogicalOperator} from "./PermissionRuleEnum";


export interface CreatePermissionRuleCommand {
    fieldName?: string;            
  operator?: ComparisonOperator; 
  value?: string;               
  logicalOperator?: LogicalOperator;
  groupOrder?: number;         

  joinLocalKey: string;
  joinForeignKey: string;
  joinEntity: string;
}

export type UpdatePermissionRuleCommand = {
  id: string;
   fieldName?: string | null;
  operator?: ComparisonOperator | null;
  value?: string | null;
  logicalOperator?: LogicalOperator | null;
  groupOrder?: number | null;

  
  joinLocalKey?: string | null; 
  joinForeignKey?: string | null; 
  joinEntity?: string | null; 
} & Partial<CreatePermissionRuleCommand>;



export type PermissionRuleFormCommand = CreatePermissionRuleCommand | UpdatePermissionRuleCommand;