// src/modules/Authorization/api/ResourcesApi.ts
import getAPI from "@/core/api/axiosClient";
import type { ResourceDto } from "../models/ResourceDto";
import type { CreateResourceApiRequest } from "../models/CreateResourceRequest";
import type { UpdateResourceApiRequest } from "../models/UpdateResourceRequest";

const API_MODULE = "authorization"; // دقت کنید که با حروف کوچک در env تعریف شده باشد

export const resourceApi = {
  // دریافت درخت منابع (GET)
  getTree: async (rootId?: string): Promise<ResourceDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<ResourceDto[]>(
      "/api/authorization/admin/resources/tree",
      { params: { rootId }, withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
  // دریافت منبع (GET)
  getById: async (Id?: string): Promise<ResourceDto> => {
    
    console.info('1');
    const api = getAPI(API_MODULE);
    
    const response = await api.get<ResourceDto>(
      `/api/authorization/admin/resources/${Id}`,
      {  withCredentials: true }
    );
    console.info(response)
    return response.data;
  },
  // ایجاد منبع جدید (POST)
createResource: async (data: CreateResourceApiRequest): Promise<string> => {
  const api = getAPI(API_MODULE);
  console.info("data= " , data);
  const response = await api.post<string>(
    "/api/authorization/admin/resources/create",
    data,
    { withCredentials: true }
  );
  return response.data;
},

// ویرایش منبع (PUT)
  updateResource: async (data: UpdateResourceApiRequest): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.put<boolean>(
      `/api/authorization/admin/resources/${data.id}`,
      {  params: { data }, withCredentials: true }
    );
    console.log(response)
    return response.data;
  },

  // حذف منبع (Delete)
  deleteResource: async (Id?: string): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.delete<boolean>(
      `/api/authorization/admin/resources/${Id}`,
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
};