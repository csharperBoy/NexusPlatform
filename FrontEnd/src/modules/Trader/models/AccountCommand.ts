export interface CreateAccountCommand {
  name: string;
  username: string;
  password: string;
}

export interface UpdateAccountCommand {
  id: string;
  name: string;
  username: string;
  /** اگه `undefined` باشه، رمز تغییر نمی‌کنه */
  password?: string | null;
}

export interface LoginAccountCommand {
  id: string;
}

export interface ActivateAccountCommand {
  id: string;
}