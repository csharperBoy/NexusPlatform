// modules/hr/api/locationContactApi.ts
import getAPI from "@/core/api/axiosClient";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { LocationContactInfoView } from "../models/LocationContactInfoView";
import {  UpdateLocationContactCommand } from "../models/LocationContactCommand";
const API_MODULE = "contact";

export const locationContactApi = {

 // دریافت پست ها (GET)
  GetList: async (): Promise<LocationContactInfoView[]> => {
    
    const api = getAPI(API_MODULE);
    
    const response = await api.get<LocationContactInfoView[]>(
      "/api/Contact/LocationContact/GetList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
   
  // به‌روزرسانی گروهی
  batchUpdate: async (commands: UpdateLocationContactCommand[]): Promise<string[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.put<string[]>(
      `/api/Contact/LocationContact/batch`,
      { locations: commands },
      { withCredentials: true }
    );
    return response.data; // آرایه‌ای از GUIDهای به‌روز شده
  },
  
  GetSelectionList: async (): Promise<SelectionListDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/Contact/LocationContact/GetSelectionList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
  
// ویرایش منبع (PUT)
  update: async (data: UpdateLocationContactCommand): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.put<boolean>(
      `/api/Contact/LocationContact/${data.id}`, data,
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
  


};