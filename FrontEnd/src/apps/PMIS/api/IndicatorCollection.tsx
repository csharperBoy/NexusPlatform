// src/api/IndicatorCollection.ts
import apiClient from './apiClient';
import type { GetIndicatorValueRequest,GetIndicatorValueResponse } from '../models/Indicator/GetIndicatorValue';
import type { GetIndicatorListResponse } from '../models/Indicator/GetIndicatorList';
import type { SaveIndicatorChangeResponse, SaveIndicatorBulkChangeRequest ,SaveIndicatorBulkChangeResponse, SaveIndicatorChangeRequest} from '../models/Indicator/SaveIndicatorChange';

import type { GetIndicatorDraftRequest,GetIndicatorDraftResponse } from '../models/Indicator/GetIndicatorDraft';
import { IndicatorModel } from '../models/Indicator/IndicatorModel';
import { CategoryModel } from '../models/Category/CategoryModel';
import { SaveIndicatorCategoriesRequest } from '../models/Indicator/SaveIndicatorCategories';
import { SaveIndicatorClaimsRequest } from '../models/Indicator/SaveIndicatorClaims';
import { GetLookupListResponse } from '../models/Lookup';
import { SaveIndicatorValueBulkChangeRequest, SaveIndicatorValueBulkChangeResponse, SaveIndicatorValueChangeRequest, SaveIndicatorValueChangeResponse } from '../models/Indicator/SaveIndicatorValueChange';


// خواندن متغیر محیطی برای فعال‌سازی Mock
const useMock = import.meta.env.VITE_USE_MOCK === 'true';

// Lazy import برای جلوگیری از وارد کردن Mock در حالت غیر فعال
let mock: typeof import('./mock/IndicatorCollectionMock') | null = null;
if (useMock) {
  import('./mock/IndicatorCollectionMock').then((module) => {
    mock = module;
  });
}

export async function GetIndicatorDraft(
  request: GetIndicatorDraftRequest
): Promise<GetIndicatorDraftResponse[]> { // تغییر نوع بازگشتی به GetIndicatorAddInfoResponse (بدون آرایه)
  try {
      if (useMock && mock?.GetIndicatorDraft) {
        return mock.GetIndicatorDraft(request);
      }

    const response = await apiClient.post<GetIndicatorDraftResponse[]>('/api/Indicator/GetIndicatorDraft', request);
    return response.data;
  } catch (error) {
    console.error('Error in GetIndicatorDraft:', error);
    throw error; // یا می‌توانید یک مقدار پیش‌فرض برگردانید
  }
}

export async function GetIndicatorValue(
  request: GetIndicatorValueRequest
): Promise<GetIndicatorValueResponse[]> {
 if (useMock && mock?.GetIndicatorValue) {
        return mock.GetIndicatorValue(request);
      }
  const response = await apiClient.post<GetIndicatorValueResponse[]>('/api/Indicator/GetIndicatorValue', request);
   //      console.warn('GetIndicatorValue = ' + response);
  return response.data;
}

export async function GetIndicatorList(): Promise<GetIndicatorListResponse[]> {
  if (useMock && mock?.GetIndicatorList) {
        return mock.GetIndicatorList();
      }
  const response = await apiClient.get<GetIndicatorListResponse[]>('/api/Indicator/GetIndicatorList');
  console.warn('GetIndicatorList data = ' +  response.data);
  return response.data;
}
export async function SaveIndicatorValueChange(
  request: SaveIndicatorValueChangeRequest
): Promise<SaveIndicatorValueChangeResponse> {
   if (useMock && mock?.SaveIndicatorValueChange) {
        return mock.SaveIndicatorValueChange(request);
      }
  const response = await apiClient.post<SaveIndicatorValueChangeResponse>('/api/Indicator/SaveIndicatorValueChange', request);
     //    console.warn('AddIndicatorValue = ' + response);
  return response.data;
}
export async function SaveIndicatorChange(
  request: SaveIndicatorChangeRequest
): Promise<SaveIndicatorChangeResponse> {
   if (useMock && mock?.SaveIndicatorChange) {
        return mock.SaveIndicatorChange(request);
      }
  const response = await apiClient.post<SaveIndicatorChangeResponse>('/api/Indicator/SaveIndicatorChange', request);
     //    console.warn('AddIndicatorValue = ' + response);
  return response.data;
}
export async function SaveIndicatorBulkChange(
  request: SaveIndicatorBulkChangeRequest
): Promise<SaveIndicatorBulkChangeResponse> {
     if (useMock && mock?.SaveIndicatorBulkChange) {
        return mock.SaveIndicatorBulkChange(request);
      }
    console.info('request:',request);
    const response = await apiClient.post<SaveIndicatorBulkChangeResponse>('/api/Indicator/SaveIndicatorBulkChange', request);
     //    console.warn('AddIndicatorValue = ' + response);
  return response.data;
}
export async function SaveIndicatorValueBulkChange(
  request: SaveIndicatorValueBulkChangeRequest
): Promise<SaveIndicatorValueBulkChangeResponse> {
     if (useMock && mock?.SaveIndicatorValueBulkChange) {
        return mock.SaveIndicatorValueBulkChange(request);
      }
    console.info('request:',request);
    const response = await apiClient.post<SaveIndicatorValueBulkChangeResponse>('/api/Indicator/SaveIndicatorValueBulkChange', request);
     //    console.warn('AddIndicatorValue = ' + response);
  return response.data;
}
export async function DeleteIndicatorValue(
  request: string
): Promise<boolean> {
     if (useMock && mock?.DeleteIndicatorValue) {
        return mock.DeleteIndicatorValue(request);
      }
    console.info('request:',request);
    const response = await apiClient.post<boolean>('/api/Indicator/DeleteIndicatorValue', request);
     //    console.warn('AddIndicatorValue = ' + response);
  return response.data;
}

export async function GetIndicatorById(
  request: string
): Promise<IndicatorModel> {
     if (useMock && mock?.GetIndicatorById) {
        return mock.GetIndicatorById(request);
      }
    console.info('request:',request);
    const response = await apiClient.post<IndicatorModel>('/api/Indicator/GetIndicatorById', request);
     //    console.warn('AddIndicatorValue = ' + response);
  return response.data;
}

export async function CreateIndicator(
  request: IndicatorModel
): Promise<boolean> {
     if (useMock && mock?.CreateIndicator) {
        return mock.CreateIndicator(request);
      }
    console.info('request:',request);
    const response = await apiClient.post<boolean>('/api/Indicator/CreateIndicator', request);
     //    console.warn('AddIndicatorValue = ' + response);
  return response.data;
}


export async function UpdateIndicator(
  request: IndicatorModel
): Promise<boolean> {
     if (useMock && mock?.UpdateIndicator) {
        return mock.UpdateIndicator(request);
      }
    console.info('request:',request);
    const response = await apiClient.post<boolean>('/api/Indicator/UpdateIndicator', request);
     //    console.warn('AddIndicatorValue = ' + response);
  return response.data;
}
///////////////////

export async function GetIndicatorClaimsForIndicator(request: string): Promise<GetLookupListResponse[]> {
  if (useMock && mock?.GetIndicatorClaimsForIndicator) {
        return mock.GetIndicatorClaimsForIndicator(request);
      }
    const response = await apiClient.post<GetLookupListResponse[]>('/api/Indicator/GetIndicatorClaimsForIndicator', request);
     //    console.warn('AddIndicatorValue = ' + response);
  return response.data;
}
export async function GetIndicatorCategories(request: string): Promise<CategoryModel[]> {
  if (useMock && mock?.GetIndicatorCategories) {
        return mock.GetIndicatorCategories(request);
      }
    const response = await apiClient.post<CategoryModel[]>('/api/Indicator/GetIndicatorCategories', request);
     //    console.warn('AddIndicatorValue = ' + response);
  return response.data;
}

export async function SaveIndicatorCategories(request: SaveIndicatorCategoriesRequest) : Promise<boolean> {
   if (useMock && mock?.SaveIndicatorCategories) {
        return mock.SaveIndicatorCategories(request);
      }
    const response = await apiClient.post<boolean>('/api/Indicator/SaveIndicatorCategories', request);
     //    console.warn('AddIndicatorValue = ' + response);
  return response.data;
}
export async function SaveIndicatorClaims(request: SaveIndicatorClaimsRequest) : Promise<boolean> {
   if (useMock && mock?.SaveIndicatorClaims) {
        return mock.SaveIndicatorClaims(request);
      }
    const response = await apiClient.post<boolean>('/api/Indicator/SaveIndicatorCategories', request);
     //    console.warn('AddIndicatorValue = ' + response);
  return response.data;
}