// src/modules/Identity/models/CreateUserCommand.ts
export interface CreateUserCommand {
  UserName: string;
  NickName?: string | null;
  Password?: string | null;
  Email?: string | null;
  phoneNumber: string | null;
  personId:string | null;
}

  