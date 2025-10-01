// src/api/AuthCollectionMock.tsx

import type {
  LoginRequest,
  LoginResponse,
  ValidateTokenResponse,
} from '../../models/auth';

// const PREFIX = '/api/Auth';

// 🟢 موک تابع login
export const login = async (payload: LoginRequest): Promise<LoginResponse> => {
  console.warn('Mock: login with', payload);

  return Promise.resolve({
    accessToken: 'mock-token-1234567890',
    expiresAt: new Date(Date.now() + 60 * 60 * 1000).toISOString(), // یک ساعت بعد
  });
};

// 🟡 موک تابع logout
export const logout = async (): Promise<void> => {
  console.warn('Mock: logout');
  return Promise.resolve();
};

// 🔵 موک تابع validateToken
export const validateToken = async (): Promise<ValidateTokenResponse> => {
  console.warn('Mock: validateToken');

  return Promise.resolve({
    isValid: true,
    userId: 'user-001',
    roles: ['Admin', 'Operator'],
  });
};

// 🟣 موک تابع getUserName
export const getUserName = async (): Promise<string> => {
  console.warn('Mock: getUserName');
  return Promise.resolve('تست‌کاربر');
};
