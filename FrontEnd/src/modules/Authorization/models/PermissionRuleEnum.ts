// src/modules/Authorization/models/PermissionRuleEnum.ts

import { enumToSelectionList } from "@/core/helpers/enumHelpers";
import { SelectionListDto } from "@/core/models/SelectionListDto";

// =====================ComparisonOperator==================
    export enum ComparisonOperator {
  Equal = 1,         
  GreaterThan = 2,    
  LessThan = 3,       
  NotEqual = 4,      
}

export const comparisonOperatorText: Record<ComparisonOperator, string> = {
  [ComparisonOperator.Equal]: 'Equal',
  [ComparisonOperator.GreaterThan]: 'GreaterThan',
  [ComparisonOperator.LessThan]: 'LessThan',
  [ComparisonOperator.NotEqual]: 'NotEqual',
};

export const comparisonOperatorFromText: Record<string, ComparisonOperator> = {
  'Equal': ComparisonOperator.Equal,
  'GreaterThan': ComparisonOperator.GreaterThan,
  'LessThan': ComparisonOperator.LessThan,
  'NotEqual': ComparisonOperator.NotEqual,
};
export const ComparisonOperatorDisplayMap: Record<keyof typeof ComparisonOperator, string> = {
  Equal: '=',
  GreaterThan: '<',
  LessThan: '<',
  NotEqual: '!=',
};


export const ComparisonOperatorOptions: SelectionListDto[] = enumToSelectionList(ComparisonOperator, ComparisonOperatorDisplayMap);
// ==============LogicalOperator=================
export enum LogicalOperator {
  And = 1,          
  Or = 2,             
}

export const logicalOperatorText: Record<LogicalOperator, string> = {
  [LogicalOperator.And]: 'AND',
  [LogicalOperator.Or]: 'OR',
};


export const logicalOperatorFromText: Record<string, LogicalOperator> = {
  'AND': LogicalOperator.And,
  'OR': LogicalOperator.Or,
};
export const LogicalOperatorDisplayMap: Record<keyof typeof LogicalOperator, string> = {
  And: 'و',
  Or: 'یا',
};


export const LogicalOperatorOptions: SelectionListDto[] = enumToSelectionList(LogicalOperator, LogicalOperatorDisplayMap);
// =======================