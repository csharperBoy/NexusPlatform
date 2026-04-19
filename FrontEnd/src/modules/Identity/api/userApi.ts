// modules/identity/api/userApi.ts
import getAPI from "@/core/api/axiosClient";
import type { LoginRequest } from "../models/LoginRequest";
import type { RegisterRequest } from "../models/RegisterRequest";
import type { AuthResponse } from "../models/AuthResponse";
import { UserDto } from "../models/UserDto";
import { GetUsersQuery } from "../models/UserQuery"; 
import { UpdateUserCommand ,CreateUserCommand} from "../models/UserCommands";
import { RoleDto } from "../models/RoleDto";
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
// ایجاد کاربر جدید (POST)
createUser: async (data: CreateUserCommand): Promise<string> => {
  const api = getAPI(API_MODULE);
  console.info("data= " , data);
  const response = await api.post<string>(
    "/api/identity/Users/create",
    data,
    { withCredentials: true }
  );
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
      // حذف کاربر (Delete)
        deleteUser: async (Id?: string): Promise<boolean> => {
          const api = getAPI(API_MODULE);
          const response = await api.delete<boolean>(
            `/api/identity/Users/${Id}`,
            {  withCredentials: true }
          );
          console.log(response)
          return response.data;
        },
        

         // دریافت نقش های کاربر (GET)
  getRoles: async (userId?: string | null): Promise<RoleDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<RoleDto[]>(
      `/api/identity/Users/GetRoles/${userId}`,
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
};