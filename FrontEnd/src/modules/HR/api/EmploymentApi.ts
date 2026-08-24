// modules/hr/api/employmentApi.ts
import getAPI from "@/core/api/axiosClient";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { EmploymentInfoView } from "../models/EmploymentInfoView";
import {  CreateEmploymentCommand, UpdateEmploymentCommand } from "../models/EmploymentCommand";
const API_MODULE = "hr";

export const employmentApi = {

  getSelectionList: async (): Promise<SelectionListDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/hr/Employment/GetSelectionList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
 // دریافت پست ها (GET)
  getList: async (): Promise<EmploymentInfoView[]> => {
    
    const api = getAPI(API_MODULE);
    
    const response = await api.get<EmploymentInfoView[]>(
      "/api/HR/Employment/GetList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
   
  // به‌روزرسانی گروهی
  batchUpdate: async (commands: UpdateEmploymentCommand[]): Promise<string[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.put<string[]>(
      `/api/hr/Employment/batch`,
      { employments: commands },
      { withCredentials: true }
    );
    return response.data; // آرایه‌ای از GUIDهای به‌روز شده
  },
  
  
// ویرایش منبع (PUT)
  update: async (data: UpdateEmploymentCommand): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.put<boolean>(
      `/api/hr/Employment/${data.id}`, data,
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
  
// ایجاد
    create: async (data: CreateEmploymentCommand): Promise<string> => {
      const api = getAPI(API_MODULE);
      console.info("data= " , data);
      const response = await api.post<string>(
        "/api/hr/Employment/create",
        data,
        { withCredentials: true }
      );
      return response.data;
    },
   
// حذف  (Delete)
  delete: async (Id?: string): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.delete<boolean>(
      `/api/hr/Employment/${Id}`,
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
};