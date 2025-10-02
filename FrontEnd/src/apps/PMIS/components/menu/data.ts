// src\apps\PMIS\components\menu\data.ts
import {
  HiOutlineHome,
  HiOutlineUser,
  HiOutlineUsers,
  HiOutlineCube,
  HiOutlineClipboardDocumentList,
  // HiOutlineDocumentChartBar,
  HiOutlinePencilSquare,
  HiOutlineCalendarDays,
  HiOutlinePresentationChartBar,
  // HiOutlineDocumentText,
  HiOutlineArrowLeftOnRectangle,
} from 'react-icons/hi2';
// import { IoSettingsOutline } from 'react-icons/io5';

export const menu = [
  {
    catalog: 'اصلی',
    listItems: [
      {
        isLink: true,
        url: '/',
        icon: HiOutlineHome,
        label: 'صفحه اصلی',
      },
      {
        isLink: true,
        url: '/profile',
        icon: HiOutlineUser,
        label: 'پروفایل',
      },
    ],
  },
  {
    catalog: 'لیست‌ها',
    listItems: [
      {
        isLink: true,
        url: '/users',
        icon: HiOutlineUsers,
        label: 'کاربران',
      },
       {
         isLink: true,
         url: '/products',
         icon: HiOutlineCube,
         label: 'products',
       },
      {
        isLink: true,
        url: '/orders',
        icon: HiOutlineClipboardDocumentList,
        label: 'ورود مقادیر',
      },
      // {
      //   isLink: true,
      //   url: '/posts',
      //   icon: HiOutlineDocumentChartBar,
      //   label: 'posts',
      // },
    ],
  },
  {
    catalog: 'عمومی',
    listItems: [
      {
        isLink: true,
        url: '/notes',
        icon: HiOutlinePencilSquare,
        label: 'یادداشت ها',
      },
      {
        isLink: true,
        url: '/calendar',
        icon: HiOutlineCalendarDays,
        label: 'تقویم',
      },
    ],
  },
  {
    catalog: 'آنالیز',
    listItems: [
      {
        isLink: true,
        url: '/charts',
        icon: HiOutlinePresentationChartBar,
        label: 'نمودارها',
      },
      // {
      //   isLink: true,
      //   url: '/logs',
      //   icon: HiOutlineDocumentText,
      //   label: 'لاگ‌ها',
      // },
    ],
  },
  {
    catalog: 'متفرقه',
    listItems: [
      // {
      //   isLink: true,
      //   url: '/settings',
      //   icon: IoSettingsOutline,
      //   label: 'settings',
      // },
      {
        isLink: true,
        url: '/login',
        icon: HiOutlineArrowLeftOnRectangle,
        label: 'خروج',
      },
    ],
  },
];
