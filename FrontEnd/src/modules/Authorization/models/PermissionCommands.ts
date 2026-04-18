
// src/modules/Authorization/models/PermissionRequest.ts

export interface CreatePermissionCommand {
  key: string;
  name: string;
  type: number;       // 0=Module, 1=Ui, 2=Data
  category: number;   // 0=General, 1=System, 2=Module, 3=Menu, 4=Page, 5=Component, 6=DatabaseTable, 7=RowInTable
  parentId?: string | null;
  description?: string;
  displayOrder: number;
  icon?: string;
  route?: string;

}



// export interface UpdatePermissionApiRequest {
//   id: string;
//   key: string;
//   name: string;
//   type: number;       // 0=Module, 1=Ui, 2=Data
//   category: number;   // 0=General, 1=System, 2=Module, 3=Menu, 4=Page, 5=Component, 6=DatabaseTable, 7=RowInTable
//   parentId?: string | null;
//   description?: string;
//   displayOrder: number;
//   icon?: string;
//   route?: string;
// }
  

  export interface UpdatePermissionCommand extends CreatePermissionCommand {  
   Id: string;
}


// برای اینکه هوک بتواند هر دو حالت را مدیریت کند، از Union Type استفاده می‌کنیم:
export type PermissionFormCommand = CreatePermissionCommand | UpdatePermissionCommand;