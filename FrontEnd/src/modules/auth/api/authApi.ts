// modules/auth/api/authApi.ts
import getAPI from "../../../core/api/axiosClient";
import type { LoginRequest } from "../models/LoginRequest";
import type { RegisterRequest } from "../models/RegisterRequest";
import type { AuthResponse } from "../models/AuthResponse";

// نام ماژول API - می‌توانید از env بگیرید یا مستقیماً مشخص کنید
const API_MODULE = import.meta.env.VITE_API_MODULE || "auth";

export const authApi = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const axiosClient = getAPI(API_MODULE);
    const url = "/api/auth/login";
    
    console.log(`[authApi] POST ${url}`, data);
    console.log(`[authApi] BaseURL: ${axiosClient.defaults.baseURL}`);

    const response = await axiosClient.post<AuthResponse>(url, data);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const axiosClient = getAPI(API_MODULE);
    const url = "/api/auth/register";
    
    console.log(`[authApi] POST ${url}`, data);
    console.log(`[authApi] BaseURL: ${axiosClient.defaults.baseURL}`);

    const response = await axiosClient.post<AuthResponse>(url, data);
    return response.data;
  },

  logout: async (): Promise<void> => {
    const axiosClient = getAPI(API_MODULE);
    const url = "/api/auth/logout";
    
    console.log(`[authApi] POST ${url}`);
    
    await axiosClient.post(url);
  },

  refreshToken: async (refreshToken: string): Promise<AuthResponse> => {
    const axiosClient = getAPI(API_MODULE);
    const url = "/api/auth/refresh";
    
    console.log(`[authApi] POST ${url}`);

    const response = await axiosClient.post<AuthResponse>(url, { refreshToken });
    return response.data;
  },
};