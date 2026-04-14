// modules/identity/api/userApi.ts
import getAPI from "@/core/api/axiosClient";
import type { LoginRequest } from "../models/LoginRequest";
import type { RegisterRequest } from "../models/RegisterRequest";
import type { AuthResponse } from "../models/AuthResponse";
import { UserDto } from "../models/UserDto";
import { GetUsersQuery } from "../models/GetUsersQuery";

const API_MODULE = "identity";

export const userApi = {

 // دریافت کاربران (GET)
  getUsers: async (req?: GetUsersQuery | null): Promise<UserDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<UserDto[]>(
      "/api/identity/Users/GetUsers",
      { params: { req }, withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
};