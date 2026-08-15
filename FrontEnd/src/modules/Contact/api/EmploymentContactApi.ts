// modules/hr/api/employmentContactApi.ts
import getAPI from "@/core/api/axiosClient";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { EmploymentContactInfoView } from "../models/EmploymentContactInfoView";
import {  UpdateEmploymentContactCommand } from "../models/EmploymentContactCommand";
const API_MODULE = "contact";

export const employmentContactApi = {

 // دریافت پست ها (GET)
  GetList: async (): Promise<EmploymentContactInfoView[]> => {
    
    const api = getAPI(API_MODULE);
    
    const response = await api.get<EmploymentContactInfoView[]>(
      "/api/contact/EmploymentContact/GetList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
   
  // به‌روزرسانی گروهی
  batchUpdateEmploymentsContact: async (commands: UpdateEmploymentContactCommand[]): Promise<string[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.put<string[]>(
      `/api/contact/EmploymentContact/batch`,
      { employmentsContact: commands },
      { withCredentials: true }
    );
    return response.data; // آرایه‌ای از GUIDهای به‌روز شده
  },
  
  GetSelectionList: async (): Promise<SelectionListDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/contact/EmploymentContact/GetSelectionList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
  
// ویرایش منبع (PUT)
  updateEmploymentContact: async (data: UpdateEmploymentContactCommand): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.put<boolean>(
      `/api/contact/EmploymentContact/${data.id}`, data,
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
  


};