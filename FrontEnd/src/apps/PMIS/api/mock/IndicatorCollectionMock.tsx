// src/api/IndicatorCollectionMock.tsx

import type {
  GetIndicatorValueRequest,
  GetIndicatorValueResponse,
} from '../../models/Indicator/GetIndicatorValue';

import type { GetIndicatorListResponse } from '../../models/Indicator/GetIndicatorList';

import type {
  SaveIndicatorChangeRequest,
  SaveIndicatorChangeResponse,
  SaveIndicatorBulkChangeRequest,
  SaveIndicatorBulkChangeResponse,
} from '../../models/Indicator/SaveIndicatorChange';

import type {
  GetIndicatorDraftRequest,
  GetIndicatorDraftResponse,
} from '../../models/Indicator/GetIndicatorDraft';
import { IndicatorModel } from '../../models/Indicator/IndicatorModel';
import { CategoryModel } from '../../models/Category/CategoryModel';
import { SaveIndicatorCategoriesRequest } from '../../models/Indicator/SaveIndicatorCategories';
import { SaveIndicatorClaimsRequest } from '../../models/Indicator/SaveIndicatorClaims';
import { GetLookupListResponse } from '../../models/Lookup';
import { SaveIndicatorValueBulkChangeRequest, SaveIndicatorValueBulkChangeResponse, SaveIndicatorValueChangeRequest, SaveIndicatorValueChangeResponse } from '../../models/Indicator/SaveIndicatorValueChange';

// 📌 موک تابع GetIndicatorList
export async function GetIndicatorList(): Promise<GetIndicatorListResponse[]> {
  console.warn('Mock: GetIndicatorList');

  return Promise.resolve([
    { id: 'ind-001', title: 'مصرف انرژی' },
    { id: 'ind-002', title: 'دما' },
    { id: 'ind-003', title: 'فشار' },
  ]);
}
export async function GetIndicatorCategories(request: string): Promise<CategoryModel[]> {
  console.warn('Mock: GetIndicatorCategories' , request);
 return Promise.resolve([    
    { 
        id: 'cat-002',
        FklkpTypeId: 1,
        FkParentId: null,
        Code: 'b',
        Title: 'test2',
        OrderNum: 2,
        Description: 'test des',
    },    
    { 
        id: 'cat-004',
        FklkpTypeId: 1,
        FkParentId: 'cat-001',
        Code: 'c',
        Title: 'test1-2',
        OrderNum: 2,
        Description: 'test des',
    },
  ]);
}
// 📌 موک تابع GetIndicatorValue
export async function GetIndicatorValue(
  request: GetIndicatorValueRequest
): Promise<GetIndicatorValueResponse[]> {
  console.warn('Mock: GetIndicatorValue with', request);

  const result = [
    {
      id: 'val-001',
      fkIndicatorId: request.fkIndicatorId?.[0] ?? 'ind-001',
      fkLkpValueTypeId: 1,
      fkLkpShiftId: 2,
      dateTime: new Date(),
      value: 75.3,
      valueCumulative: 120.0,
      description: 'مقدار تستی',
    },
    {
      id: 'val-002',
      fkIndicatorId: request.fkIndicatorId?.[0] ?? 'ind-002',
      fkLkpValueTypeId: 2,
      fkLkpShiftId: 1,
      dateTime: new Date(),
      value: 70.3,
      valueCumulative: 20.0,
      description: 'مقدار تستی',
    },
    {
      id: 'val-003',
      fkIndicatorId: request.fkIndicatorId?.[0] ?? 'ind-003',
      fkLkpValueTypeId: 2,
      fkLkpShiftId: 1,
      dateTime: new Date(),
      value: 5.3,
      valueCumulative: 10.0,
      description: 'مقدار تستی',
    },
    {
      id: 'val-004',
      fkIndicatorId: request.fkIndicatorId?.[0] ?? 'ind-001',
      fkLkpValueTypeId: 2,
      fkLkpShiftId: 1,
      dateTime: new Date(),
      value: 75.3,
      valueCumulative: 120.0,
      description: 'مقدار تستی',
    },
  ];

  console.warn('Mock: GetIndicatorValue result', result);
  console.log(result.map(r => r.id)); // باید هر کد یکتا باشه و غیر خالی

  return Promise.resolve(result);
}

// 📌 موک تابع GetIndicatorDraft
export async function GetIndicatorDraft(
  request: GetIndicatorDraftRequest
): Promise<GetIndicatorDraftResponse[]> {
  console.warn('Mock: GetIndicatorDraft with', request);

  return Promise.resolve([
    {
      fkIndicatorId: request.fkIndicatorId?.[0] ?? 'ind-002',
      fkLkpValueTypeId: 2,
      fkLkpShiftId: 1,
      dateTime: new Date(),
      value: 20,
      valueCumulative: 35,
      description: 'اطلاعات اولیه تست',
    },
  ]);
}

// 📌 موک تابع AddIndicatorValue
export async function SaveIndicatorChange(
  request: SaveIndicatorChangeRequest
): Promise<SaveIndicatorChangeResponse> {
  console.warn('Mock: SaveIndicatorChange', request);

  return Promise.resolve({
    isSuccess: true,
    id: 'ind-002',
  });
}

// 📌 موک تابع SaveIndicatorBulkChange
export async function SaveIndicatorBulkChange(
  request: SaveIndicatorBulkChangeRequest
): Promise<SaveIndicatorBulkChangeResponse> {
  console.warn('Mock: SaveIndicatorBulkChange', request);

  return Promise.resolve({
    isSuccess: true,
  });
}

export async function SaveIndicatorValueChange(
  request: SaveIndicatorValueChangeRequest
): Promise<SaveIndicatorValueChangeResponse> {
  console.warn('Mock: SaveIndicatorChange', request);

  return Promise.resolve({
    isSuccess: true,
    id: 'val-added-001',
  });
}

// 📌 موک تابع SaveIndicatorBulkChange
export async function SaveIndicatorValueBulkChange(
  request: SaveIndicatorValueBulkChangeRequest
): Promise<SaveIndicatorValueBulkChangeResponse> {
  console.warn('Mock: SaveIndicatorBulkChange', request);

  return Promise.resolve({
    isSuccess: true,
  });
}
export async function DeleteIndicatorValue(
  request: string
): Promise<boolean> {
  console.warn('Mock: DeleteIndicatorValue', request);

  return Promise.resolve(true);
}


export async function GetIndicatorById(
  request: string
): Promise<IndicatorModel> {
    
  console.warn('Mock: GetIndicatorById', request);

  return Promise.resolve({
    code: 'test',
    dateTimeFrom: new Date(),
    dateTimeTo:new Date(),
    title:'test title',
    id:'test-guid-001',
    fkLkpDesirabilityId: 1,
    fkLkpFormId:1,
    fkLkpManualityId:1,
    fkLkpMeasureId:1,
    fkLkpPeriodId:1,
    fkLkpTypeId:1,
  });
}

export async function CreateIndicator(
  request: IndicatorModel
): Promise<boolean> {
    
  console.warn('Mock: CreateIndicator', request);

  return Promise.resolve(true);
}


export async function UpdateIndicator(
  request: IndicatorModel
): Promise<boolean> {
     
  console.warn('Mock: UpdateIndicator', request);

  return Promise.resolve(true);
}


export async function SaveIndicatorCategories(request: SaveIndicatorCategoriesRequest) : Promise<boolean> 
{
   
  console.warn('Mock: SaveIndicatorCategories', request);

  return Promise.resolve(true);
}

export function SaveIndicatorClaims(request: SaveIndicatorClaimsRequest): Promise<boolean> {
  console.warn('Mock: SaveIndicatorClaims', request);

  return Promise.resolve(true);
}
export function GetIndicatorClaimsForIndicator(request: string): Promise<GetLookupListResponse[]> {
  console.warn('Mock: GetIndicatorClaimsForIndicator');
  return Promise.resolve([
    { id: 1, display: 'مالک' },
    { id: 3, display: 'نویسنده' },
  ]);
}

