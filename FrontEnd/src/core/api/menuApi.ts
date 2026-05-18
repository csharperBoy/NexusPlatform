// core/api/menuApi.ts
import getAPI from "@/core/api/axiosClient";
import { MenuDto } from "../models/Menu";
const API_MODULE = "core";

export const menuApi = {

 // دریافت ماژول های فعال (GET)
  GetMenus: async (): Promise<MenuDto[]> => {
    
    console.info('GetMenus :');
    const api = getAPI(API_MODULE);
    
    const response = await api.get<MenuDto[]>(
      "/api/base/Menu/GetMenuTreeQuery"
    );
    console.warn(response);
    console.log(response);
    return response.data;
  }
      
};