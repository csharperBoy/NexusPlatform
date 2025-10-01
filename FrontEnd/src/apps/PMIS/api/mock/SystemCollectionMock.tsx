// src/api/SystemCollectionMock.tsx

import type { MenuCatalogDto } from '../../models/System/GetMenuResponse';

export async function getMenus(): Promise<MenuCatalogDto[]> {
  console.warn('Mock: getMenus');

  return Promise.resolve([
    {
      catalog: 'داشبورد',
      listItems: [
        {
          isLink: true,
          url: '/',
          icon: 'HiOutlineHome',
          label: 'صفحه اصلی',
        },
      ],
    },
    {
      catalog: 'لیست‌ها',
      listItems: [
        {
          isLink: true,
          url: '/IndicatorValueInput',
          icon: 'HiOutlineClipboardDocumentList',
          label: 'ورود مقادیر',
        },
        {
          isLink: true,
          url: '/IndicatorForm',
          icon: 'HiOutlineClipboardDocumentList',
         label: 'شناسنامه شاخص',
        },
         {
          isLink: true,
          url: '/IndicatorAssignToCategory',
          icon: 'HiOutlineClipboardDocumentList',
         label: ' شاخص / دسته بندی',
        },{
          isLink: true,
          url: '/IndicatorAssignToClaim',
          icon: 'HiOutlineClipboardDocumentList',
         label: ' شاخص / ادعا',
        },
        
        {
          isLink: true,
          url: '/RegisterIndicatorForm',
          icon: 'HiOutlineClipboardDocumentList',
         label: ' شاخص',
        },
         {
          isLink: true,
          url: '/IndicatorWizardStepper',
          icon: 'HiOutlineClipboardDocumentList',
         label: ' 2شاخص',
        },
        
      ],
    },
    {
      catalog: 'تنظیمات',
      listItems: [
        {
          isLink: true,
          url: '/settings',
          icon: 'HiOutlineCog',
          label: 'پیکربندی سیستم',
        },
      ],
    },
  ]);
}
