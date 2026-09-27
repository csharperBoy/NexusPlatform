export interface CreateAccountCommand {
  broker: number;      // ← جدید
  name: string;
  username: string;
  password: string;
}

export interface UpdateAccountCommand {
  id: string;
  name: string;
  username: string;
  password?: string;
}
export interface LoginAccountCommand {
  id: string;
}

export interface ActivateAccountCommand {
  id: string;
}