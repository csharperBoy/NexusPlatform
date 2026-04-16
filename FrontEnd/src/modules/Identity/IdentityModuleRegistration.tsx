// modules/identity/IdentityModuleRegistration.tsx

import { useEffect } from 'react';
import { useMenu, usePlugin } from '@/modules/DashboardCore';
import { lazy } from 'react';

// ویجت آمار کاربران (برای صفحه اصلی داشبورد)
const UserStatsWidget = lazy(() => import('./widgets/UserStatsWidget'));

// آیکون ساده برای منو
const UserIcon = () => (
  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
  </svg>
);

export const IdentityModuleRegistration = () => {
  const { registerMenuItem } = useMenu();
  const { registerWidget } = usePlugin();

  useEffect(() => {
    // ثبت منوی مدیریت کاربران
    registerMenuItem({
      id: 'users',
      title: 'مدیریت کاربران',
      path: '/users',
      icon: <UserIcon />,
      order: 10,
    });
 registerMenuItem({
      id: 'roles',
      title: 'مدیریت نقش ها',
      path: '/roles',
      icon: <UserIcon />,
      order: 20,
    });

    // ثبت ویجت برای صفحه اصلی داشبورد
    registerWidget({
      id: 'identity-user-stats',
      area: 'widgets', // یا 'main' بسته به جایگاه
      priority: 10,
      component: UserStatsWidget,
      title: 'آمار کاربران',
    });
  }, []);

  return null;
};