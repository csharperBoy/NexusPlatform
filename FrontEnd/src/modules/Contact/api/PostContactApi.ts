// modules/hr/api/postContactApi.ts
import getAPI from "@/core/api/axiosClient";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { PostContactInfoView } from "../models/postContactInfoView";
import {  UpdatePostContactCommand } from "../models/postContactCommand";
const API_MODULE = "contact";

export const postContactApi = {

 // دریافت پست ها (GET)
  GetList: async (): Promise<PostContactInfoView[]> => {
    
    const api = getAPI(API_MODULE);
    
    const response = await api.get<PostContactInfoView[]>(
      "/api/Contact/PostContact/GetList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
   
  // به‌روزرسانی گروهی
  batchUpdatePostsContact: async (commands: UpdatePostContactCommand[]): Promise<string[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.put<string[]>(
      `/api/Contact/PostContact/batch`,
      { posts: commands },
      { withCredentials: true }
    );
    return response.data; // آرایه‌ای از GUIDهای به‌روز شده
  },
  
  GetSelectionList: async (): Promise<SelectionListDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/Contact/PostContact/GetSelectionList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
  
// ویرایش منبع (PUT)
  updatePostContact: async (data: UpdatePostContactCommand): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.put<boolean>(
      `/api/Contact/PostContact/${data.id}`, data,
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
  

};