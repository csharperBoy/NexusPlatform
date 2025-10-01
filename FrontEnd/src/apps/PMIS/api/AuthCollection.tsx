// src/api/AuthCollection.ts
import apiClient from './apiClient';
import type { LoginRequest, LoginResponse, ValidateTokenResponse } from '../models/auth';

// آدرس مشترک API
const PREFIX = '/api/Auth';

// خواندن متغیر محیطی برای فعال‌سازی Mock
const useMock = import.meta.env.VITE_USE_MOCK === 'true';

// Lazy import برای جلوگیری از وارد کردن Mock در حالت غیر فعال
let mock: typeof import('./mock/AuthCollectionMock') | null = null;
if (useMock) {
  import('./mock/AuthCollectionMock').then((module) => {
    mock = module;
  });
}

// 🟢 ورود کاربر
export const login = async (payload: LoginRequest): Promise<LoginResponse> => {
  if (useMock && mock?.login) {
    return mock.login(payload);
  }

  const res = await apiClient.post<LoginResponse>(`${PREFIX}/login`, payload);
  return res.data;
};

// 🟡 خروج کاربر
export const logout = async (): Promise<void> => {
  if (useMock && mock?.logout) {
    return mock.logout();
  }

  await apiClient.post(`${PREFIX}/logout`);
};

// 🔵 بررسی توکن
export const validateToken = async (): Promise<ValidateTokenResponse> => {
  if (useMock && mock?.validateToken) {
    return mock.validateToken();
  }

  const res = await apiClient.get<ValidateTokenResponse>(`${PREFIX}/validateToken`);
  return res.data;
};

// 🟣 دریافت نام کاربری
export const getUserName = async (): Promise<string> => {
  if (useMock && mock?.getUserName) {
    return mock.getUserName();
  }

  const res = await apiClient.get<string>(`${PREFIX}/GetUserName`);
  return res.data;
};

