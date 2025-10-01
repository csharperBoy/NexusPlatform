import { UserModel } from "../models/User";
import { ChangePasswordRequest } from "../models/User/ChangePassword";
import apiClient from './apiClient';


// آدرس مشترک API
const PREFIX = '/api/User';
// خواندن متغیر محیطی برای فعال‌سازی Mock
const useMock = import.meta.env.VITE_USE_MOCK === 'true';

// Lazy import برای جلوگیری از وارد کردن Mock در حالت غیر فعال
let mock: typeof import('./mock/UserCollectionMock') | null = null;
if (useMock) {
  import('./mock/UserCollectionMock').then((module) => {
    mock = module;
  });
}
export async function getUserList(): Promise<UserModel[]> {
         if (useMock && mock?.getUserList) {
            return mock.getUserList();
          }
  const res = await apiClient.get<UserModel[]>(`${PREFIX}/getUserList`);
       console.warn('getUserList = ' + res);
  return res.data;
}

export const ChangePassword = async (request: ChangePasswordRequest) : Promise<boolean> => {
  if (useMock && mock?.ChangePassword) {
    return mock.ChangePassword(request);
  }

  const res = await apiClient.get<boolean>(`${PREFIX}/ChangePassword`);
  return res.data;
};