// src/api/SystemCollection.ts
import apiClient from './apiClient';
import type { MenuCatalogDto } from '../models/System/GetMenuResponse';

/**
 * فراخوانی سرویس GetMenus از سیستم (دریافت منوهای مجاز برای کاربر)
 * متد GET روی /api/System/GetMenus
 */
// خواندن متغیر محیطی برای فعال‌سازی Mock
const useMock = import.meta.env.VITE_USE_MOCK === 'true';

// Lazy import برای جلوگیری از وارد کردن Mock در حالت غیر فعال
let mock: typeof import('./mock/SystemCollectionMock') | null = null;
if (useMock) {
  import('./mock/SystemCollectionMock').then((module) => {
    mock = module;
  });
}

export async function getMenus(): Promise<MenuCatalogDto[]> {
    if (useMock && mock?.getMenus) {
          return mock.getMenus();
        }
  const response = await apiClient.get<{ menuList: MenuCatalogDto[] }>('/api/System/GetMenus');
 
  return response.data.menuList;
}

