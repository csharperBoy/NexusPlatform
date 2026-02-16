// modules/auth/api/authApi.ts
import getAPI from "../../../core/api/axiosClient";
import type { LoginRequest } from "../models/LoginRequest";
import type { RegisterRequest } from "../models/RegisterRequest";
import type { AuthResponse } from "../models/AuthResponse";

// نام ماژول API - می‌توانید از env بگیرید یا مستقیماً مشخص کنید
const API_MODULE = import.meta.env.VITE_API_MODULE || "identity";

export const authApi = {
  loginWithEmail: async (data: LoginRequest): Promise<AuthResponse> => {
    const axiosClient = getAPI(API_MODULE);
    const url = "/api/identity/auth/login/email";
    
    console.log(`[identity] POST ${url}`, data);
    console.log(`[identity] BaseURL: ${axiosClient.defaults.baseURL}`);

    const response = await axiosClient.post<AuthResponse>(url, data);
    return response.data;
  },
  loginWithUsername: async (data: LoginRequest): Promise<AuthResponse> => {
    const axiosClient = getAPI(API_MODULE);
    const url = "/api/identity/auth/login/username";
    
    console.log(`[identity] POST ${url}`, data);
    console.log(`[identity] BaseURL: ${axiosClient.defaults.baseURL}`);

    const response = await axiosClient.post<AuthResponse>(url, data);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const axiosClient = getAPI(API_MODULE);
    const url = "/api/identity/auth/register";
    
    console.log(`[identity] POST ${url}`, data);
    console.log(`[identity] BaseURL: ${axiosClient.defaults.baseURL}`);

    const response = await axiosClient.post<AuthResponse>(url, data);
    return response.data;
  },

  logout: async (): Promise<void> => {
    const axiosClient = getAPI(API_MODULE);
    const url = "/api/identity/auth/logout";
    
    console.log(`[identity] POST ${url}`);
    
    await axiosClient.post(url);
  },

  refreshToken: async (refreshToken: string): Promise<AuthResponse> => {
    const axiosClient = getAPI(API_MODULE);
    const url = "/api/identity/auth/refresh";
    
    console.log(`[identity] POST ${url}`);

    const response = await axiosClient.post<AuthResponse>(url, { refreshToken });
    return response.data;
  },
};