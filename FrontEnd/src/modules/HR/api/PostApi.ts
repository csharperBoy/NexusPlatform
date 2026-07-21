// modules/hr/api/postApi.ts
import getAPI from "@/core/api/axiosClient";


import { SelectionListDto } from "@/core/models/SelectionListDto";
import { PostInfoView } from "../models/postInfoView";
import { CreatePostCommand, UpdatePostCommand } from "../models/postCommand";
const API_MODULE = "HR";

export const postApi = {

 // دریافت پست ها (GET)
  GetList: async (): Promise<PostInfoView[]> => {
    
console.log(API_MODULE);
    const api = getAPI(API_MODULE);
    
console.log('4');
    const response = await api.get<PostInfoView[]>(
      "/api/HR/OrgChart/GetList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
   GetSelectionList: async (): Promise<SelectionListDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/hr/OrgChart/GetSelectionList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
// ویرایش منبع (PUT)
  updatePost: async (data: UpdatePostCommand): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.put<boolean>(
      `/api/hr/OrgChart/${data.id}`, data,
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
 createPost: async (data: CreatePostCommand): Promise<string> => {
   const api = getAPI(API_MODULE);
   console.info("data= " , data);
   const response = await api.post<string>(
     "/api/hr/OrgChart/create",
     data,
     { withCredentials: true }
   );
   return response.data;
 },

};