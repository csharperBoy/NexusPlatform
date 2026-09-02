// src/modules/HR/api/SyncApi.ts
import getAPI from "@/core/api/axiosClient";
import { SyncResult } from "../models/SyncModels";

const API_MODULE = "hr";

export const SyncApi = {
  syncWithIrisa: async (): Promise<Record<string, SyncResult>> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<Record<string, SyncResult>>(
      `/api/hr/IrisaSync/syncWithIrisa`,
      { withCredentials: true  ,timeout: 240000}
    );
    return response.data;  
  },
  SyncEmployement: async (): Promise< SyncResult> => {
    const api = getAPI(API_MODULE);
    const response = await api.get< SyncResult>(
      `/api/hr/IrisaSync/SyncEmployement`,
      { withCredentials: true  ,timeout: 240000}
    );
    return response.data;  
  },
  SyncJobTitle: async (): Promise< SyncResult> => {
    const api = getAPI(API_MODULE);
    const response = await api.get< SyncResult>(
      `/api/hr/IrisaSync/SyncJobTitle`,
      { withCredentials: true  ,timeout: 240000}
    );
    return response.data;  
  },
  SyncJobLevel: async (): Promise< SyncResult> => {
    const api = getAPI(API_MODULE);
    const response = await api.get< SyncResult>(
      `/api/hr/IrisaSync/SyncJobLevel`,
      { withCredentials: true  ,timeout: 240000}
    );
    return response.data;  
  },
  SyncOrganizationUnit: async (): Promise<SyncResult> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<SyncResult>(
      `/api/hr/IrisaSync/SyncOrganizationUnit`,
      { withCredentials: true  ,timeout: 240000}
    );
    return response.data;  
  },
  SyncPost: async (): Promise<SyncResult> => {
    const api = getAPI(API_MODULE);
    const response = await api.get< SyncResult>(
      `/api/hr/IrisaSync/SyncPost`,
      { withCredentials: true  ,timeout: 240000}
    );
    return response.data;  
  },
  SyncAssignments: async (): Promise<SyncResult> => {
    const api = getAPI(API_MODULE);
    const response = await api.get< SyncResult>(
      `/api/hr/IrisaSync/SyncAssignments`,
      { withCredentials: true  ,timeout: 240000}
    );
    return response.data;  
  },
  
};

