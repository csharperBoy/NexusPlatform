// src/apps/Trader/Server/pages/DashboardPage.tsx
import React from 'react';
import { usePlugin, WidgetRenderer } from '@/modules/DashboardCore';

const DashboardPage: React.FC = () => {
  const { getWidgetsByArea } = usePlugin();

  // دریافت ویجت‌های ناحیه main و widgets
  const mainWidgets = getWidgetsByArea('main');
  const widgets = getWidgetsByArea('widgets');

  return (
    <div>
      <h1 className="text-2xl font-bold mb-4">داشبورد اصلی</h1>
      <p>به پنل مدیریت خوش آمدید.</p>

      {/* ویجت‌های ناحیه main */}
      {mainWidgets.length > 0 && (
        <div className="mt-6 space-y-4">
          {mainWidgets.map(widget => (
            <WidgetRenderer key={widget.id} widget={widget} />
          ))}
        </div>
      )}

      {/* ویجت‌های ناحیه widgets (کارت‌ها) */}
      {widgets.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mt-6">
          {widgets.map(widget => (
            <WidgetRenderer key={widget.id} widget={widget} />
          ))}
        </div>
      )}
    </div>
  );
};

export default DashboardPage;