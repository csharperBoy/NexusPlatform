// src/modules/HR/api/SyncApi.ts
import getAPI from "@/core/api/axiosClient";
import { BatchResult } from "@/core/models/apiResults";
import { SyncResult, SyncCommandBundle } from "../models/SyncModels";
import { mockSyncApi } from "./mockSyncApi";

//  تغییر این متغیر برای سوئیچ بین Mock و Server واقعی
const IS_MOCK_MODE = false; 

const API_MODULE = "hr";

export const SyncApi = {
  // ===================== Preview =====================
  SyncOrganizationUnitPreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    if (IS_MOCK_MODE) return mockSyncApi.SyncOrganizationUnitPreview();
    const api = getAPI(API_MODULE);
    const res = await api.get<BatchResult<SyncCommandBundle>>(`/api/hr/IrisaSync/SyncOrganizationUnitPreview`);
    return res.data;
  },

  SyncJobLevelPreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    if (IS_MOCK_MODE) return mockSyncApi.SyncJobLevelPreview();
    const api = getAPI(API_MODULE);
    const res = await api.get<BatchResult<SyncCommandBundle>>(`/api/hr/IrisaSync/SyncJobLevelPreview`);
    return res.data;
  },

  SyncJobTitlePreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    if (IS_MOCK_MODE) return mockSyncApi.SyncJobTitlePreview();
    const api = getAPI(API_MODULE);
    const res = await api.get<BatchResult<SyncCommandBundle>>(`/api/hr/IrisaSync/SyncJobTitlePreview`);
    return res.data;
  },

  SyncEmploymentsPreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    if (IS_MOCK_MODE) return mockSyncApi.SyncEmploymentsPreview();
    const api = getAPI(API_MODULE);
    const res = await api.get<BatchResult<SyncCommandBundle>>(`/api/hr/IrisaSync/SyncEmploymentsPreview`);
    return res.data;
  },

  SyncPostPreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    if (IS_MOCK_MODE) return mockSyncApi.SyncPostPreview();
    const api = getAPI(API_MODULE);
    const res = await api.get<BatchResult<SyncCommandBundle>>(`/api/hr/IrisaSync/SyncPostPreview`);
    return res.data;
  },

  SyncAssignmentsPreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    if (IS_MOCK_MODE) return mockSyncApi.SyncAssignmentsPreview();
    const api = getAPI(API_MODULE);
    const res = await api.get<BatchResult<SyncCommandBundle>>(`/api/hr/IrisaSync/SyncAssignmentsPreview`);
    return res.data;
  },

  // ===================== Apply =====================
  ApplyOrganizationUnit: async (bundle: SyncCommandBundle): Promise<BatchResult<SyncResult>> => {
    if (IS_MOCK_MODE) return mockSyncApi.ApplyGeneric(bundle);
    const api = getAPI(API_MODULE);
    const res = await api.post<BatchResult<SyncResult>>(`/api/hr/IrisaSync/ApplyOrganizationUnit`, bundle);
    return res.data;
  },

  ApplyJobLevel: async (bundle: SyncCommandBundle): Promise<BatchResult<SyncResult>> => {
    if (IS_MOCK_MODE) return mockSyncApi.ApplyGeneric(bundle);
    const api = getAPI(API_MODULE);
    const res = await api.post<BatchResult<SyncResult>>(`/api/hr/IrisaSync/ApplyJobLevel`, bundle);
    return res.data;
  },

  ApplyJobTitle: async (bundle: SyncCommandBundle): Promise<BatchResult<SyncResult>> => {
    if (IS_MOCK_MODE) return mockSyncApi.ApplyGeneric(bundle);
    const api = getAPI(API_MODULE);
    const res = await api.post<BatchResult<SyncResult>>(`/api/hr/IrisaSync/ApplyJobTitle`, bundle);
    return res.data;
  },

  ApplyEmployments: async (bundle: SyncCommandBundle): Promise<BatchResult<SyncResult>> => {
    if (IS_MOCK_MODE) return mockSyncApi.ApplyGeneric(bundle);
    const api = getAPI(API_MODULE);
    const res = await api.post<BatchResult<SyncResult>>(`/api/hr/IrisaSync/ApplyEmployments`, bundle);
    return res.data;
  },

  ApplyPost: async (bundle: SyncCommandBundle): Promise<BatchResult<SyncResult>> => {
    if (IS_MOCK_MODE) return mockSyncApi.ApplyGeneric(bundle);
    const api = getAPI(API_MODULE);
    const res = await api.post<BatchResult<SyncResult>>(`/api/hr/IrisaSync/ApplyPost`, bundle);
    return res.data;
  },

  ApplyAssignments: async (bundle: SyncCommandBundle): Promise<BatchResult<SyncResult>> => {
    if (IS_MOCK_MODE) return mockSyncApi.ApplyGeneric(bundle);
    const api = getAPI(API_MODULE);
    const res = await api.post<BatchResult<SyncResult>>(`/api/hr/IrisaSync/ApplyAssignments`, bundle);
    return res.data;
  }
};