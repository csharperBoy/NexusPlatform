// modules/DashboardCore/index.ts
export * from './types';
export { DashboardProvider } from './contexts/DashboardProvider';
export { usePlugin, useMenu } from './contexts/DashboardProvider';
export { MainLayout, Header, Sidebar, WidgetRenderer } from './components';