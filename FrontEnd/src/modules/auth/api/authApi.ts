// modules/auth/api/authApi.ts
import axiosClient from "@/core/api/axiosClient";
import { LoginRequest } from "../models/LoginRequest";
import { RegisterRequest } from "../models/RegisterRequest";
import { AuthResponse } from "../models/AuthResponse";

export const authApi = {
  login: (data: LoginRequest) =>
    axiosClient.post<AuthResponse>("/auth/login", data).then((res) => res.data),

  register: (data: RegisterRequest) =>
    axiosClient.post<AuthResponse>("/auth/register", data).then((res) => res.data),
};
