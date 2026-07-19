// modules/identity/api/userApi.ts
import getAPI from "@/core/api/axiosClient";


import { SelectionListDto } from "@/core/models/SelectionListDto";
const API_MODULE = "identity";

export const positionApi = {

 // دریافت پست ها (GET)
  GetSelectionList: async (): Promise<SelectionListDto[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/hr/Posts/GetSelectionList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  },

};