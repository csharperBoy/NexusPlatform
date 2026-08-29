// src/modules/HR/api/SyncApi.ts
import getAPI from "@/core/api/axiosClient";
import { SyncResult } from "../models/SyncModels";

const API_MODULE = "hr";

export const SyncApi = {
  syncWithIrisa: async (): Promise<Record<string, SyncResult>> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<Record<string, SyncResult>>(
      `/api/hr/IrisaSync/syncWithIrisa`,
      { withCredentials: true }
    );
    return response.data; // مستقیماً دیکشنری
  },
};