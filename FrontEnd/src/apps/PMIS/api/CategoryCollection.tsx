// src/api/IndicatorCollection.ts
import apiClient from './apiClient';
import { CategoryModel } from '../models/Category/CategoryModel';


// خواندن متغیر محیطی برای فعال‌سازی Mock
const useMock = import.meta.env.VITE_USE_MOCK === 'true';

// Lazy import برای جلوگیری از وارد کردن Mock در حالت غیر فعال
let mock: typeof import('./mock/CategoryCollectionMock') | null = null;
if (useMock) {
  import('./mock/CategoryCollectionMock').then((module) => {
    mock = module;
  });
}

export async function GetCategoryList(): Promise<CategoryModel[]> {
  if (useMock && mock?.GetCategoryList) {
        return mock.GetCategoryList();
      }
  const response = await apiClient.get<CategoryModel[]>('/api/Category/GetCategoryList');
  console.warn('GetIndicatorList data = ' +  response.data);
  return response.data;
}