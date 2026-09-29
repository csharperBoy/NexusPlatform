// modules/contact/api/ContactResourceApi.ts
import getAPI from "@/core/api/axiosClient";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { ContactResourceInfoView } from "../models/ContactResourceInfoView";
import {  CreateContactResourceCommand, UpdateContactResourceCommand } from "../models/ContactResourceCommand";
import { ApiOptions, DEFAULT_OPTIONS } from "@/core/api/apiOptions";
import { TreeSelectionListDto } from "@/core/models/TreeSelectionListDto";
const API_MODULE = "hr";

export const ContactResourceApi = {
    // دریافت لیست جهت نمایش در dropdown ها
getSelectionList: async (): Promise<TreeSelectionListDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<TreeSelectionListDto[]>(
      "/api/hr/ContactResource/GetSelectionList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
  
 // دریافت لیست (GET)
  getList: async (): Promise<ContactResourceInfoView[]> => {
    
    const api = getAPI(API_MODULE);
    
    const response = await api.get<ContactResourceInfoView[]>(
      "/api/HR/ContactResource/GetList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
   // ایجاد
    create: async (data: CreateContactResourceCommand, 
  options?: ApiOptions): Promise<string | any> => {
      const api = getAPI(API_MODULE);
    const { offlineStrategy } = { ...DEFAULT_OPTIONS, ...options };
      console.info("data= " , data);
      const response = await api.post<string | any>(
        "/api/hr/ContactResource/create",
        data,
        { withCredentials: true,
        offlineStrategy,
        moduleName: API_MODULE }
      );
      return response.data;
    },
   
  // به‌روزرسانی گروهی
  batchUpdate: async (commands: UpdateContactResourceCommand[], 
  options?: ApiOptions): Promise<string[] | any> => {
    const api = getAPI(API_MODULE);
    const { offlineStrategy } = { ...DEFAULT_OPTIONS, ...options };
    const response = await api.put<string[] | any>(
      `/api/hr/ContactResource/batch`,
      { ContactResources: commands },
      { withCredentials: true ,
        offlineStrategy,
        moduleName: API_MODULE}
    );
    return response.data; // آرایه‌ای از GUIDهای به‌روز شده
  },
  
  
// ویرایش  (PUT)
  update: async (data: UpdateContactResourceCommand, 
  options?: ApiOptions): Promise<boolean | any> => {
    const api = getAPI(API_MODULE);
    const { offlineStrategy } = { ...DEFAULT_OPTIONS, ...options };
    
    const response = await api.put<boolean | any>(
      `/api/hr/ContactResource/${data.id}`, data,
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
      `/api/hr/ContactResource/${Id}`,
      {  withCredentials: true ,
        offlineStrategy,
        moduleName: API_MODULE

      }
    );
    console.log(response)
    return response.data;
  },

};