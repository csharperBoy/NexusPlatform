// core/api/settingApi.ts
import getAPI from "@/core/api/axiosClient";
import { ModuleItem } from "../models/ModuleItem";
const API_MODULE = "core";

export const settingApi = {

 // دریافت ماژول های فعال (GET)
  GetActiveModules: async (): Promise<ModuleItem[]> => {
    const api = getAPI(API_MODULE);
    
    const response = await api.get<ModuleItem[]>(
      "/api/core/setting/GetActiveModules",
      {  withCredentials: true }
    );
    console.warn(response);
    console.log(response);
    return response.data;
  }
      
};