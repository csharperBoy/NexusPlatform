// modules/hr/api/postApi.ts
import getAPI from "@/core/api/axiosClient";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { PostInfoView } from "../models/postInfoView";
import { CreatePostCommand, UpdatePostCommand } from "../models/postCommand";
const API_MODULE = "hr";

export const postApi = {

GetJobTitleSelectionList: async (): Promise<SelectionListDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/hr/OrgChart/JobTitle/GetSelectionList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
  GetGradeSelectionList: async (): Promise<SelectionListDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/hr/OrgChart/Grade/GetSelectionList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
  GetOrganizationUnitSelectionList: async (): Promise<SelectionListDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/hr/OrgChart/OrganizationUnit/GetSelectionList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
GetJobLevelSelectionList: async (): Promise<SelectionListDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/hr/OrgChart/JobLevel/GetSelectionList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
 // دریافت پست ها (GET)
  gtList: async (): Promise<PostInfoView[]> => {
    console.log('get list post:')
    const api = getAPI(API_MODULE);
    
    const response = await api.get<PostInfoView[]>(
      "/api/HR/OrgChart/GetList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
   
  // به‌روزرسانی گروهی
  batchUpdatePosts: async (commands: UpdatePostCommand[]): Promise<string[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.put<string[]>(
      `/api/hr/OrgChart/batch`,
      { posts: commands },
      { withCredentials: true }
    );
    return response.data; // آرایه‌ای از GUIDهای به‌روز شده
  },
  
  getSelectionList: async (): Promise<SelectionListDto[]> => {
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
// حذف  (Delete)
  delete: async (Id?: string): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.delete<boolean>(
      `/api/hr/OrgChart/${Id}`,
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },
};