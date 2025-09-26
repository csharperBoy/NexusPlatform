// modules/auth/api/authApi.ts
import axiosClient from "../../../core/api/axiosClient";
import type { LoginRequest } from "../models/LoginRequest";
import type { RegisterRequest } from "../models/RegisterRequest";
import type { AuthResponse } from "../models/AuthResponse";

export const authApi = {
  login: (data: LoginRequest) =>
    axiosClient.post<AuthResponse>("/auth/login", data).then((res: any) => res.data),

  register: (data: RegisterRequest) =>
    axiosClient.post<AuthResponse>("/auth/register", data).then((res: any) => res.data),
};
