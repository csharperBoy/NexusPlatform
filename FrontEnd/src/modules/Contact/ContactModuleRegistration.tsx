// src/modules/PhoneBook/PhoneBookModuleRegistration.tsx
import { useEffect } from 'react';
import { useMenu, usePlugin } from '@/modules/DashboardCore';
import { lazy } from 'react';

const PhoneBookStatsWidget = lazy(() => import('./widgets/PhoneBookStatsWidget'));

const HrIcon = () => (
  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
  </svg>
);

export const PhoneBookModuleRegistration = () => {
  const { registerMenuItem } = useMenu();
  const { registerWidget } = usePlugin();

  useEffect(() => {
    
    // ثبت ویجت
    registerWidget({
      id: 'hr-PhoneBook-stats',
      area: 'widgets',
      priority: 30,
      component: PhoneBookStatsWidget,
      title: 'آمار کارمندان',
    });
  }, []);

  return null;
};