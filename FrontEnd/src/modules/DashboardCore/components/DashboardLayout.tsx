// modules/DashboardCore/components/DashboardLayout.tsx
import React, { ReactNode } from 'react';
import { Sidebar } from './Sidebar';
import { Header } from './Header';
import { usePlugin } from '../contexts/PluginContext';
import WidgetRenderer from './WidgetRenderer';
import { DashboardWidget } from '../types'; 

export interface DashboardLayoutProps {
  children: ReactNode;
  /** کلاس‌های اضافی برای بخش‌های مختلف */
  sidebarClassName?: string;
  headerClassName?: string;
  contentClassName?: string;
  /** امکان سفارشی‌سازی کامل کل layout */
  render?: (props: {
    sidebar: ReactNode;
    header: ReactNode;
    content: ReactNode;
    widgets: ReactNode;
  }) => ReactNode;
}

export const DashboardLayout: React.FC<DashboardLayoutProps> = ({ children, sidebarClassName, headerClassName, contentClassName = 'p-4', render }) => {
  const { getWidgetsByArea } = usePlugin();

  const sidebar = <Sidebar className={sidebarClassName} />;
  const header = <Header className={headerClassName} />;
  
  const content = (
    <main className={`flex-1 overflow-y-auto ${contentClassName}`}>
      {getWidgetsByArea('main').map((widget: DashboardWidget) => ( // <-- اضافه کردن نوع
        <WidgetRenderer key={widget.id} widget={widget} />
      ))}
      {children}
    </main>
  );

  const widgets = getWidgetsByArea('widgets').length > 0 && (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mt-4">
      {getWidgetsByArea('widgets').map((widget: DashboardWidget) => ( // <-- اضافه کردن نوع
        <WidgetRenderer key={widget.id} widget={widget} />
      ))}
    </div>
  );

  if (render) {
    return <>{render({ sidebar, header, content, widgets })}</>;
  }

  return (
    <div className="flex h-screen bg-gray-100">
      {sidebar}
      <div className="flex-1 flex flex-col overflow-hidden">
        {header}
        {content}
        {widgets}
      </div>
    </div>
  );
};