import { ChangePasswordRequest } from "../../models/User/ChangePassword";
import { UserModel } from "../../models/User/UserModel";

export async function getUserList(): Promise<UserModel[]> {
         console.warn('Mock: getUserList');
  return Promise.resolve([
    {id:1, displayName: 'مهدی عباسیان', userName: '868' },
    {id:2, displayName: 'حمیدرضا عباسی ', userName: '867' },
  ]);
}

export async function ChangePassword(request: ChangePasswordRequest) : Promise<boolean> {
         console.warn('Mock: ChangePassword');
  return Promise.resolve(true);
}
