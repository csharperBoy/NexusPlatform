// modules/identity/api/userApi.ts
import getAPI from "@/core/api/axiosClient";
import type { LoginRequest } from "../models/LoginRequest";
import type { RegisterRequest } from "../models/RegisterRequest";
import type { AuthResponse } from "../models/AuthResponse";
import { UserDto } from "../models/UserDto";
import { GetUsersQuery } from "../models/GetUsersQuery";
import { UpdateUserCommand } from "../models/UpdateUserCommand";

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

  // دریافت کاربر (GET)
    getById: async (Id?: string): Promise<UserDto> => {
      
      const api = getAPI(API_MODULE);
      
      const response = await api.get<UserDto>(
        `/api/identity/Users/${Id}`,
        {  withCredentials: true }
      );
      console.info(response)
      return response.data;
    },

    // ویرایش کاربر (PUT)
      updateUser: async (data: UpdateUserCommand): Promise<boolean> => {
        const api = getAPI(API_MODULE);
         console.warn(data);
        const response = await api.put<boolean>(
          `/api/identity/Users/${data.Id}`, data,
          {  withCredentials: true }
        );
        console.log(response)
        return response.data;
      },
};