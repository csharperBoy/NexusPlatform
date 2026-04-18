// modules/identity/api/roleApi.ts
import getAPI from "@/core/api/axiosClient";
import type { LoginRequest } from "../models/LoginRequest";
import type { RegisterRequest } from "../models/RegisterRequest";
import type { AuthResponse } from "../models/AuthResponse";
import { RoleDto } from "../models/RoleDto";
import { GetRolesQuery } from "../models/RoleQuery";
import { UpdateRoleCommand } from "../models/UpdateRoleCommand";
import { CreateRoleCommand } from "../models/CreateRoleCommand";
const API_MODULE = "identity";

export const roleApi = {

 // دریافت کاربران (GET)
  getRoles: async (req?: GetRolesQuery | null): Promise<RoleDto[]> => {
    const api = getAPI(API_MODULE);
    
    console.info(req);
    const response = await api.get<RoleDto[]>(
      "/api/identity/Roles/GetRoles",
      { params: { req }, withCredentials: true }
    );
    console.warn(response);
    console.log(response);
    return response.data;
  },

  // دریافت کاربر (GET)
    getById: async (Id?: string): Promise<RoleDto> => {
      
      const api = getAPI(API_MODULE);
      
      const response = await api.get<RoleDto>(
        `/api/identity/Roles/${Id}`,
        {  withCredentials: true }
      );
      console.info(response)
      return response.data;
    },
// ایجاد کاربر جدید (POST)
createRole: async (data: CreateRoleCommand): Promise<string> => {
  const api = getAPI(API_MODULE);
  console.info("data= " , data);
  const response = await api.post<string>(
    "/api/identity/Roles/create",
    data,
    { withCredentials: true }
  );
  return response.data;
},
    // ویرایش کاربر (PUT)
      updateRole: async (data: UpdateRoleCommand): Promise<boolean> => {
        const api = getAPI(API_MODULE);
         console.warn(data);
        const response = await api.put<boolean>(
          `/api/identity/Roles/${data.Id}`, data,
          {  withCredentials: true }
        );
        console.log(response)
        return response.data;
      },
      // حذف کاربر (Delete)
        deleteRole: async (Id?: string): Promise<boolean> => {
          const api = getAPI(API_MODULE);
          const response = await api.delete<boolean>(
            `/api/identity/Roles/${Id}`,
            {  withCredentials: true }
          );
          console.log(response)
          return response.data;
        },
        

      
};