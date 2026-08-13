//src/modules/PhoneBook/api/PhoneBookApi.ts
import getAPI from "@/core/api/axiosClient";


import { PhoneBookEmploymentDto } from "../models/PhoneBookEmploymentDto";
const API_MODULE = "contact";

export const phonebookApi = {

 // دریافت (GET)
  GetList: async (organUnitId?: string): Promise<PhoneBookEmploymentDto[]> => {
    
    const api = getAPI(API_MODULE);
    
    const response = await api.get<PhoneBookEmploymentDto[]>(
      "/api/Contact/PhoneBook/GetList",
      { 
        params: { organUnitId },
        withCredentials: true 
      }
    );
    console.log(response)
    return response.data;
  }

};

