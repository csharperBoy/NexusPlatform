// modules/identity/api/identityApi.ts
import getAPI from "@/core/api/axiosClient";
import type { LoginRequest } from "../models/LoginRequest";
import type { RegisterRequest } from "../models/RegisterRequest";
import type { AuthResponse } from "../models/AuthResponse";

const API_MODULE = "identity";

export const identityApi = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const api = getAPI(API_MODULE);
    const response = await api.post<AuthResponse>(
      "/api/identity/auth/login",
      data,
      { withCredentials: true }
    );
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const api = getAPI(API_MODULE);
    const response = await api.post<AuthResponse>(
      "/api/identity/auth/register",
      data,
      { withCredentials: true }
    );
    return response.data;
  },

  logout: async (): Promise<void> => {
    const api = getAPI(API_MODULE);
    await api.post(
      "/api/identity/auth/logout",
      {},
      { withCredentials: true }
    );
  },

  refresh: async (): Promise<AuthResponse> => {
    const api = getAPI(API_MODULE);
    const response = await api.post<AuthResponse>(
      "/api/identity/auth/refresh",
      {},
      { withCredentials: true }
    );
    return response.data;
  },
};