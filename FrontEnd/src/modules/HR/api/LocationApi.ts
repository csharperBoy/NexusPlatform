// modules/hr/api/locationApi.ts
import getAPI from "@/core/api/axiosClient";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { LocationInfoView } from "../models/LocationInfoView";
import {  CreateLocationCommand, UpdateLocationCommand } from "../models/LocationCommand";
const API_MODULE = "hr";

export const locationApi = {
    // دریافت لیست جهت نمایش در dropdown ها
GetSelectionList: async (): Promise<SelectionListDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/hr/Location/GetSelectionList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
  
 // دریافت لیست (GET)
  GetList: async (): Promise<LocationInfoView[]> => {
    
    const api = getAPI(API_MODULE);
    
    const response = await api.get<LocationInfoView[]>(
      "/api/HR/Location/GetList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
   // ایجاد
    create: async (data: CreateLocationCommand): Promise<string> => {
      const api = getAPI(API_MODULE);
      console.info("data= " , data);
      const response = await api.post<string>(
        "/api/hr/Location/create",
        data,
        { withCredentials: true }
      );
      return response.data;
    },
   
  // به‌روزرسانی گروهی
  batchUpdatelocations: async (commands: UpdateLocationCommand[]): Promise<string[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.put<string[]>(
      `/api/hr/Location/batch`,
      { locations: commands },
      { withCredentials: true }
    );
    return response.data; // آرایه‌ای از GUIDهای به‌روز شده
  },
  
  
// ویرایش  (PUT)
  updatelocation: async (data: UpdateLocationCommand): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.put<boolean>(
      `/api/hr/Location/${data.id}`, data,
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
  
  // حذف  (Delete)
  delete: async (Id?: string): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.delete<boolean>(
      `/api/hr/Location/${Id}`,
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },

};