// ========================AssignTyp====================

import { SelectionListDto } from "@/core/models/SelectionListDto";

import { enumToSelectionList } from '@/core/helpers/enumHelpers';

export enum AssignType {
  Person = 0,         
  Position = 1,    
  Role = 2,       
  User = 3,      
}

export const assignTypeText: Record<AssignType, string> = {
  [AssignType.Person]: 'Person',
  [AssignType.Position]: 'Position',
  [AssignType.Role]: 'Role',
  [AssignType.User]: 'User',
};

export const comparisonAssignTypeFromText: Record<string, AssignType> = {
  'Person': AssignType.Person,
  'Position': AssignType.Position,
  'Role': AssignType.Role,
  'User': AssignType.User,
};

export const AssignTypeDisplayMap: Record<keyof typeof AssignType, string> = {
  Person: 'شخص',
  Position: 'موقعیت شغلی',
  Role:'نقش',
  User:'کاربر'
};

export const AssignTypeOptions: SelectionListDto[] = enumToSelectionList(AssignType, AssignTypeDisplayMap);
// ====================== Action ==========================

      export enum Action {
  View = 0,         
  Create = 1,    
  Edit = 2,       
  Delete = 3,   
  Export = 4,
     Full = 99
}

export const actionText: Record<Action, string> = {
  [Action.View]: 'View',
  [Action.Create]: 'Create',
  [Action.Edit]: 'Edit',
  [Action.Delete]: 'Delete',
  [Action.Export]: 'Export',
  [Action.Full]: 'Full',
};

export const comparisonActionFromText: Record<string, Action> = {
  'View': Action.View,
  'Create': Action.Create,
  'Edit': Action.Edit,
  'Delete': Action.Delete,
  'Export': Action.Export,
  'Full': Action.Full,
};

export const ActionDisplayMap: Record<keyof typeof Action, string> = {
  Create: 'ایجاد',
  Delete: 'حذف',
  Edit:'ویرایش',
  Export:'دریافت خروجی',
  Full:'کامل',
  View:'نمایش',

};


export const ActionOptions: SelectionListDto[] = enumToSelectionList(Action, ActionDisplayMap);
// ================Effect===========================
        export enum Effect {
  allow = 0,         
  Deny = 1,    
  
}

export const effectText: Record<Effect, string> = {
  [Effect.allow]: 'allow',
  [Effect.Deny]: 'Deny',
  
};

export const comparisonEffectFromText: Record<string, Effect> = {
  'allow': Effect.allow,
  'Deny': Effect.Deny,
 
};


export const EffectDisplayMap: Record<keyof typeof Effect, string> = {
  allow:'دسترسی',
  Deny:'عدم دسترسی',
};

export const EffectOptions: SelectionListDto[] = enumToSelectionList(Effect, EffectDisplayMap);
// ===============Scope==================
  
  export enum Scope {
  None = 0,         
  Account = 1,    
  Self = 2,       
  Unit = 3,   
  UnitAndBelow = 4,
  SpecificProperty=5,
     All = 99
}

export const scopeText: Record<Scope, string> = {
  [Scope.None]: 'None',
  [Scope.Account]: 'Account',
  [Scope.Self]: 'Self',
  [Scope.Unit]: 'Unit',
  [Scope.UnitAndBelow]: 'UnitAndBelow',
  [Scope.SpecificProperty]: 'SpecificProperty',
  [Scope.All]: 'All',
};

export const comparisonScopeFromText: Record<string, Scope> = {
  'None': Scope.None,
  'Account': Scope.Account,
  'Self': Scope.Self,
  'Unit': Scope.Unit,
  'UnitAndBelow': Scope.UnitAndBelow,
  'SpecificProperty': Scope.SpecificProperty,
  'All': Scope.All,
};


export const ScopeDisplayMap: Record<keyof typeof Scope, string> = {
  Account:'اکانت',
  All:'همه',
  None:'هیچی',
  Self:'شخص',
  SpecificProperty:'اختصاصی',
  Unit:'واحد',
  UnitAndBelow:'واحد و زیرمجموعه',
};


export const ScopeOptions: SelectionListDto[] = enumToSelectionList(Scope, ScopeDisplayMap);