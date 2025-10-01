// src/api/LookUpCollectionMock.tsx
import type { GetLookupListResponse } from '../../models/Lookup/GetLookupList';

// توابع موک‌شده

export async function GetLookupList(): Promise<GetLookupListResponse[]> {
  console.warn('Mock: GetLookupList');
   return Promise.resolve([
    { id: 1, display: 'گزینه اول' },
  { id: 2, display: 'گزینه دوم' },
  { id: 3, display: 'گزینه سوم' },
  ]);
}

export async function getShifts(): Promise<GetLookupListResponse[]> {
  console.warn('Mock: getShifts');
  return Promise.resolve([
    { id: 1, display: 'شیفت صبح' },
    { id: 2, display: 'شیفت عصر' },
    { id: 3, display: 'شیفت شب' },
  ]);
}

export async function getForms(): Promise<GetLookupListResponse[]> {
  console.warn('Mock: getForms');
  return Promise.resolve([
    { id: 1, display: 'تخصصی تضمین' },
    { id: 2, display: 'مالی' },
  ]);
}

export async function getPeriods(): Promise<GetLookupListResponse[]> {
  console.warn('Mock: getPeriods');
  return Promise.resolve([
    { id: 1, display: 'هفتگی' },
    { id: 2, display: 'ماهانه' },
  ]);
}

export async function getValueTypes(): Promise<GetLookupListResponse[]> {
  console.warn('Mock: getValueTypes');
  return Promise.resolve([
    { id: 1, display: 'هدف' },
    { id: 2, display: 'عملکرد' },
    { id: 3, display: 'پیش بینی' },
  ]);
}

export async function getIndicatorClaims(): Promise<GetLookupListResponse[]> {
  console.warn('Mock: getIndicatorClaims');
  return Promise.resolve([
    { id: 1, display: 'مالک' },
    { id: 2, display: 'خواندن' },
    { id: 3, display: 'نویسنده' },
  ]);
}