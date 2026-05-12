// src/modules/Authorization/api/PermissionRuleApi.ts
import getAPI from "@/core/api/axiosClient";
import type { PermissionRuleDto } from "../models/PermissionRuleDto";
import type { CreatePermissionRuleCommand , UpdatePermissionRuleCommand } from "../models/PermissionRuleCommands";
import { GetPermissionsRuleQuery } from "../models/PermissionRuleQuery";

const API_MODULE = "authorization"; // دقت کنید که با حروف کوچک در env تعریف شده باشد

export const permissionRuleApi = {
    // دریافت کاربران (GET)
    getPermissionRules: async (req?: GetPermissionsRuleQuery | null): Promise<PermissionRuleDto[]> => {
      const api = getAPI(API_MODULE);
      
      console.info(req);
      const response = await api.get<PermissionRuleDto[]>(
        "/api/authorization/admin/permissionRule/getRules",
        { params: { req }, withCredentials: true }
      );
      console.warn(response);
      console.log(response);
      return response.data;
    },
  // دریافت منبع (GET)
  getById: async (Id?: string): Promise<PermissionRuleDto> => {
    
    const api = getAPI(API_MODULE);
    
    const response = await api.get<PermissionRuleDto>(
      `/api/authorization/admin/permissionRule/${Id}`,
      {  withCredentials: true }
    );
    console.info(response)
    return response.data;
  },
  // ایجاد منبع جدید (POST)
createPermissionRule: async (data: CreatePermissionRuleCommand): Promise<string> => {
  const api = getAPI(API_MODULE);
  console.info("data= " , data);
  const response = await api.post<string>(
    "/api/authorization/admin/permissionRule/create",
    data,
    { withCredentials: true }
  );
  return response.data;
},

// ویرایش منبع (PUT)
  updatePermissionRule: async (data: UpdatePermissionRuleCommand): Promise<boolean> => {
    const api = getAPI(API_MODULE);
     console.info(data);
    const response = await api.put<boolean>(
      `/api/authorization/admin/permissionRule/${data.id}`, data,
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },

  // حذف منبع (Delete)
  deletePermissionRule: async (Id?: string): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.delete<boolean>(
      `/api/authorization/admin/permissionRule/${Id}`,
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
};