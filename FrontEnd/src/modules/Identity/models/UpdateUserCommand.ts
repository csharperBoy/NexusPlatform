// src/modules/Identity/models/UpdateUserCommand.ts
export interface UpdateUserCommand {
  Id: string;
  UserName: string;
  FirstName?: string | null;
  LastName?: string | null;
  Password?: string | null;
  Email?: string | null;
  phoneNumber: string | null;
}

  