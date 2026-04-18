// src/modules/Identity/models/CreateRoleCommand.ts

export interface CreateRoleCommand {
  Name?: string | null;
  Description?: string | null;
  OrderNum?: number | null;

}


// src/modules/Identity/models/UpdateRoleCommand.ts
// export interface UpdateRoleCommand {
//   Id: string;
//   Name?: string | null;
//   Description?: string | null;
//   OrderNum?: number | null;
// }
  

  export interface UpdateRoleCommand extends CreateRoleCommand {  
   Id: string;
}


// برای اینکه هوک بتواند هر دو حالت را مدیریت کند، از Union Type استفاده می‌کنیم:
export type RoleFormCommand = CreateRoleCommand | UpdateRoleCommand;