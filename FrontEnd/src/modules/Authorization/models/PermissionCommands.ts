
// src/modules/Authorization/models/PermissionRequest.ts

export interface CreatePermissionCommand {
  ResourceId: string;
  AssigneeId: string;
  AssigneeType: number;       // 0=Module, 1=Ui, 2=Data
  Action: number;   // 0=General, 1=System, 2=Module, 3=Menu, 4=Page, 5=Component, 6=DatabaseTable, 7=RowInTable
  effect?: number;
  EffectiveFrom?: Date;
  ExpiresAt: Date;
  IsActive?: string;
  Description?: string;
  scopes?: number[];
}

  export interface UpdatePermissionCommand extends CreatePermissionCommand {  
   Id: string;
}


// برای اینکه هوک بتواند هر دو حالت را مدیریت کند، از Union Type استفاده می‌کنیم:
export type PermissionFormCommand = CreatePermissionCommand | UpdatePermissionCommand;