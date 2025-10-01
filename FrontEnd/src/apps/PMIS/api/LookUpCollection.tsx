// src/api/LookupCollection.ts
import apiClient from './apiClient';
import type { GetLookupListResponse as GetLookupListResponseModel } from '../models/Lookup/GetLookupList';

// خواندن متغیر محیطی برای فعال‌سازی Mock
const useMock = import.meta.env.VITE_USE_MOCK === 'true';

// Lazy import برای جلوگیری از وارد کردن Mock در حالت غیر فعال
let mock: typeof import('./mock/LookUpCollectionMock') | null = null;
if (useMock) {
  import('./mock/LookUpCollectionMock').then((module) => {
    mock = module;
  });
}
export async function GetLookupList(): Promise<GetLookupListResponseModel[]> {
  if (useMock && mock?.GetLookupList) {
            return mock.GetLookupList();
          }
  const response = await apiClient.get< GetLookupListResponseModel[] >('/api/Lookup/GetLookupList');  
  console.warn('GetLookupList = ' + response);
  return response.data;
}

// ...import ها
export async function getShifts(): Promise<GetLookupListResponseModel[]> {
   if (useMock && mock?.getShifts) {
            return mock.getShifts();
          }
  const res = await apiClient.get<GetLookupListResponseModel[]>('/api/Lookup/GetShiftList');
   console.warn('getShifts = ' + res);
  return res.data;
}

export async function getForms(): Promise<GetLookupListResponseModel[]> {
     if (useMock && mock?.getForms) {
            return mock.getForms();
          }
  const res = await apiClient.get<GetLookupListResponseModel[]>('/api/Lookup/GetFormList');
   console.warn('GetFormList = ' + res);
  return res.data;
}

export async function getPeriods(): Promise<GetLookupListResponseModel[]> {
       if (useMock && mock?.getPeriods) {
            return mock.getPeriods();
          }
  const res = await apiClient.get<GetLookupListResponseModel[]>('/api/Lookup/GetPeriodList');
     console.warn('getPeriods = ' + res);
  return res.data;
}

export async function getValueTypes(): Promise<GetLookupListResponseModel[]> {
         if (useMock && mock?.getValueTypes) {
            return mock.getValueTypes();
          }
  const res = await apiClient.get<GetLookupListResponseModel[]>('/api/Lookup/GetValueTypeList');
       console.warn('getValueTypes = ' + res);
  return res.data;
}
export async function getIndicatorClaims(): Promise<GetLookupListResponseModel[]> {
       if (useMock && mock?.getIndicatorClaims) {
            return mock.getIndicatorClaims();
          }
  const res = await apiClient.get<GetLookupListResponseModel[]>('/api/Lookup/GetIndicatorClaims');
     console.warn('getIndicatorClaims = ' + res);
  return res.data;
}


