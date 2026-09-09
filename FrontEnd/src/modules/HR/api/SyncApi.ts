// src/modules/HR/api/SyncApi.ts
import getAPI from "@/core/api/axiosClient";
import { BatchResult } from "@/core/models/apiResults";
import { SyncResult, SyncCommandBundle } from "../models/SyncModels";

const API_MODULE = "hr";

export const SyncApi = {
  // ===================== قدیمی (همگام‌سازی مستقیم) =====================
  syncWithIrisa: async (): Promise<Record<string, BatchResult<SyncResult>>> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<Record<string, BatchResult<SyncResult>>>(
      `/api/hr/IrisaSync/syncWithIrisa`,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  SyncEmployement: async (): Promise<BatchResult<SyncResult>> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<BatchResult<SyncResult>>(
      `/api/hr/IrisaSync/SyncEmployement`,
      { withCredentials: true, timeout: 540000 }
    );
    return response.data;
  },

  SyncJobTitle: async (): Promise<BatchResult<SyncResult>> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<BatchResult<SyncResult>>(
      `/api/hr/IrisaSync/SyncJobTitle`,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  SyncJobLevel: async (): Promise<BatchResult<SyncResult>> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<BatchResult<SyncResult>>(
      `/api/hr/IrisaSync/SyncJobLevel`,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  SyncOrganizationUnit: async (): Promise<BatchResult<SyncResult>> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<BatchResult<SyncResult>>(
      `/api/hr/IrisaSync/SyncOrganizationUnit`,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  SyncPost: async (): Promise<BatchResult<SyncResult>> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<BatchResult<SyncResult>>(
      `/api/hr/IrisaSync/SyncPost`,
      { withCredentials: true, timeout: 540000 }
    );
    return response.data;
  },

  SyncAssignments: async (): Promise<BatchResult<SyncResult>> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<BatchResult<SyncResult>>(
      `/api/hr/IrisaSync/SyncAssignments`,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  // ===================== Preview (GET) =====================
  SyncEmploymentsPreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<BatchResult<SyncCommandBundle>>(
      `/api/hr/IrisaSync/SyncEmploymentsPreview`,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  SyncJobTitlePreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<BatchResult<SyncCommandBundle>>(
      `/api/hr/IrisaSync/SyncJobTitlePreview`,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  SyncJobLevelPreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<BatchResult<SyncCommandBundle>>(
      `/api/hr/IrisaSync/SyncJobLevelPreview`,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  SyncOrganizationUnitPreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<BatchResult<SyncCommandBundle>>(
      `/api/hr/IrisaSync/SyncOrganizationUnitPreview`,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  SyncPostPreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<BatchResult<SyncCommandBundle>>(
      `/api/hr/IrisaSync/SyncPostPreview`,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  SyncAssignmentsPreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<BatchResult<SyncCommandBundle>>(
      `/api/hr/IrisaSync/SyncAssignmentsPreview`,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  // ===================== Apply (POST) =====================
  ApplyEmployments: async (
    bundle: SyncCommandBundle
  ): Promise<BatchResult<SyncResult>> => {
    const api = getAPI(API_MODULE);
    const response = await api.post<BatchResult<SyncResult>>(
      `/api/hr/IrisaSync/ApplyEmployments`,
      bundle,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  ApplyJobTitle: async (
    bundle: SyncCommandBundle
  ): Promise<BatchResult<SyncResult>> => {
    const api = getAPI(API_MODULE);
    const response = await api.post<BatchResult<SyncResult>>(
      `/api/hr/IrisaSync/ApplyJobTitle`,
      bundle,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  ApplyJobLevel: async (
    bundle: SyncCommandBundle
  ): Promise<BatchResult<SyncResult>> => {
    const api = getAPI(API_MODULE);
    const response = await api.post<BatchResult<SyncResult>>(
      `/api/hr/IrisaSync/ApplyJobLevel`,
      bundle,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  ApplyOrganizationUnit: async (
    bundle: SyncCommandBundle
  ): Promise<BatchResult<SyncResult>> => {
    const api = getAPI(API_MODULE);
    const response = await api.post<BatchResult<SyncResult>>(
      `/api/hr/IrisaSync/ApplyOrganizationUnit`,
      bundle,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  ApplyPost: async (
    bundle: SyncCommandBundle
  ): Promise<BatchResult<SyncResult>> => {
    const api = getAPI(API_MODULE);
    const response = await api.post<BatchResult<SyncResult>>(
      `/api/hr/IrisaSync/ApplyPost`,
      bundle,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },

  ApplyAssignments: async (
    bundle: SyncCommandBundle
  ): Promise<BatchResult<SyncResult>> => {
    const api = getAPI(API_MODULE);
    const response = await api.post<BatchResult<SyncResult>>(
      `/api/hr/IrisaSync/ApplyAssignments`,
      bundle,
      { withCredentials: true, timeout: 240000 }
    );
    return response.data;
  },
};