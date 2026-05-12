// src/modules/Authorization/api/JoinDetailApi.ts
import getAPI from "@/core/api/axiosClient";
import type { JoinDetailDto } from "../models/JoinDetailDto";
import type { CreateJoinDetailCommands , UpdateJoinDetailCommands } from "../models/JoinDetailCommands";
import { GetJoinDetailQuery } from "../models/JoinDetailQuery";

const API_MODULE = "authorization"; // دقت کنید که با حروف کوچک در env تعریف شده باشد

export const JoinDetailApi = {
    // دریافت کاربران (GET)
    GetSelectionList: async (req?: GetJoinDetailQuery | null): Promise<JoinDetailDto[]> => {
      const api = getAPI(API_MODULE);
      
      console.info(req);
      const response = await api.get<JoinDetailDto[]>(
        "/api/authorization/admin/JoinDetail/getSelectionList",
        { params: { req }, withCredentials: true }
      );
      console.warn(response);
      console.log(response);
      return response.data;
    },
  
};