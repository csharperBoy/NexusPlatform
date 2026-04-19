// src/modules/Authorization/api/PermissionApi.ts
import getAPI from "@/core/api/axiosClient";
import type { PermissionDto } from "../models/PermissionDto";
import type { CreatePermissionCommand , UpdatePermissionCommand } from "../models/PermissionCommands";
import { GetPermissionsQuery } from "../models/PermissionQuery";

const API_MODULE = "authorization"; // دقت کنید که با حروف کوچک در env تعریف شده باشد

export const permissionApi = {
    // دریافت کاربران (GET)
    getPermissions: async (req?: GetPermissionsQuery | null): Promise<PermissionDto[]> => {
      const api = getAPI(API_MODULE);
      
      console.info(req);
      const response = await api.get<PermissionDto[]>(
        "/api/authorization/admin/Permissions/GetPermissions",
        { params: { req }, withCredentials: true }
      );
      console.warn(response);
      console.log(response);
      return response.data;
    },
  // دریافت منبع (GET)
  getById: async (Id?: string): Promise<PermissionDto> => {
    
    const api = getAPI(API_MODULE);
    
    const response = await api.get<PermissionDto>(
      `/api/authorization/admin/permissions/${Id}`,
      {  withCredentials: true }
    );
    console.info(response)
    return response.data;
  },
  // ایجاد منبع جدید (POST)
createPermission: async (data: CreatePermissionCommand): Promise<string> => {
  const api = getAPI(API_MODULE);
  console.info("data= " , data);
  const response = await api.post<string>(
    "/api/authorization/admin/permissions/create",
    data,
    { withCredentials: true }
  );
  return response.data;
},

// ویرایش منبع (PUT)
  updatePermission: async (data: UpdatePermissionCommand): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.put<boolean>(
      `/api/authorization/admin/permissions/${data.Id}`, data,
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },

  // حذف منبع (Delete)
  deletePermission: async (Id?: string): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.delete<boolean>(
      `/api/authorization/admin/permissions/${Id}`,
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
};