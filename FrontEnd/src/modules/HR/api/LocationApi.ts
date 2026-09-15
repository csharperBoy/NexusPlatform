// modules/hr/api/locationApi.ts
import getAPI from "@/core/api/axiosClient";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { LocationInfoView } from "../models/LocationInfoView";
import {  CreateLocationCommand, UpdateLocationCommand } from "../models/LocationCommand";
import { ApiOptions, DEFAULT_OPTIONS } from "@/core/api/apiOptions";
const API_MODULE = "hr";

export const locationApi = {
    // دریافت لیست جهت نمایش در dropdown ها
getSelectionList: async (): Promise<SelectionListDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/hr/Location/GetSelectionList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
  
 // دریافت لیست (GET)
  getList: async (): Promise<LocationInfoView[]> => {
    
    const api = getAPI(API_MODULE);
    
    const response = await api.get<LocationInfoView[]>(
      "/api/HR/Location/GetList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
   // ایجاد
    create: async (data: CreateLocationCommand, 
  options?: ApiOptions): Promise<string | any> => {
      const api = getAPI(API_MODULE);
    const { offlineStrategy } = { ...DEFAULT_OPTIONS, ...options };
      console.info("data= " , data);
      const response = await api.post<string | any>(
        "/api/hr/Location/create",
        data,
        { withCredentials: true,
        offlineStrategy,
        moduleName: API_MODULE }
      );
      return response.data;
    },
   
  // به‌روزرسانی گروهی
  batchUpdate: async (commands: UpdateLocationCommand[], 
  options?: ApiOptions): Promise<string[] | any> => {
    const api = getAPI(API_MODULE);
    const { offlineStrategy } = { ...DEFAULT_OPTIONS, ...options };
    const response = await api.put<string[] | any>(
      `/api/hr/Location/batch`,
      { locations: commands },
      { withCredentials: true ,
        offlineStrategy,
        moduleName: API_MODULE}
    );
    return response.data; // آرایه‌ای از GUIDهای به‌روز شده
  },
  
  
// ویرایش  (PUT)
  update: async (data: UpdateLocationCommand, 
  options?: ApiOptions): Promise<boolean | any> => {
    const api = getAPI(API_MODULE);
    const { offlineStrategy } = { ...DEFAULT_OPTIONS, ...options };
    
    const response = await api.put<boolean | any>(
      `/api/hr/Location/${data.id}`, data,
      {  withCredentials: true ,
        offlineStrategy,
        moduleName: API_MODULE}
    );
    console.log(response)
    return response.data;
  },
  
  // حذف  (Delete)
  delete: async (Id?: string, 
  options?: ApiOptions): Promise<boolean | any> => {
    const api = getAPI(API_MODULE);
    const { offlineStrategy } = { ...DEFAULT_OPTIONS, ...options };
    const response = await api.delete<boolean | any>(
      `/api/hr/Location/${Id}`,
      {  withCredentials: true ,
        offlineStrategy,
        moduleName: API_MODULE

      }
    );
    console.log(response)
    return response.data;
  },

};