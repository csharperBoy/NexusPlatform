// src/modules/Authorization/api/ResourcesApi.ts
import getAPI from "@/core/api/axiosClient";
import type { ResourceTreeDto } from "../models/ResourceTreeDto";
import type { CreateResourceRequest } from "../models/CreateResourceRequest";

const API_MODULE = "authorization"; // دقت کنید که با حروف کوچک در env تعریف شده باشد

export const resourceApi = {
  // دریافت درخت منابع (GET)
  getTree: async (rootId?: string): Promise<ResourceTreeDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<ResourceTreeDto[]>(
      "/api/authorization/admin/resources/tree",
      { params: { rootId }, withCredentials: true }
    );
    return response.data;
  },

  // ایجاد منبع جدید (POST)
  createResource: async (data: CreateResourceRequest): Promise<string> => {
    const api = getAPI(API_MODULE);
    const response = await api.post<string>(
      "/api/authorization/admin/resources",
      data,
      { withCredentials: true }
    );
    return response.data;
  },
};