// src/modules/Identity/models/CreateUserCommand.ts

export interface CreateUserCommand {
  UserName: string;
  Email: string;
  Password: string; // اجباری برای ایجاد
  NickName?: string | null;
  phoneNumber?: string | null;
  personId?: string | null;
  roles?: string[];
}

// src/modules/Identity/models/UpdateUserCommand.ts
// import { CreateUserCommand } from "./CreateUserCommand";
// export interface UpdateUserCommand {
//   Id: string;
//   UserName: string;
//   NickName?: string | null;
//   Password?: string | null;
//   Email?: string | null;
//   phoneNumber: string | null;
//   personId:string | null;
//   roles?: string[] | null;
// }

  export interface UpdateUserCommand extends Omit<CreateUserCommand, "Password"> {
  Password?: string; // اختیاری برای ویرایش
   Id: string;
}


// برای اینکه هوک بتواند هر دو حالت را مدیریت کند، از Union Type استفاده می‌کنیم:
export type UserFormCommand = CreateUserCommand | UpdateUserCommand;