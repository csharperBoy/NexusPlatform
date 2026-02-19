// modules/DashboardCore/contexts/PluginContext.tsx
import React, { createContext, useState, useCallback, ReactNode, useContext } from 'react';
import { DashboardWidget, WidgetArea } from '../types';

interface PluginContextType {
  registerWidget: (widget: DashboardWidget) => void;
  getWidgetsByArea: (area: WidgetArea) => DashboardWidget[];
}

const PluginContext = createContext<PluginContextType | undefined>(undefined);

export const PluginProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [widgets, setWidgets] = useState<DashboardWidget[]>([]);

  const registerWidget = useCallback((widget: DashboardWidget) => {
    setWidgets(prev => {
      if (prev.some(w => w.id === widget.id)) {
        console.warn(`Widget with id "${widget.id}" already registered.`);
        return prev;
      }
      return [...prev, widget];
    });
  }, []);

  const getWidgetsByArea = useCallback((area: WidgetArea) => {
    return widgets
      .filter(w => w.area === area)
      .sort((a, b) => (a.priority ?? 0) - (b.priority ?? 0));
  }, [widgets]);

  return (
    <PluginContext.Provider value={{ registerWidget, getWidgetsByArea }}>
      {children}
    </PluginContext.Provider>
  );
};

export const usePlugin = () => {
  const context = useContext(PluginContext);
  if (!context) throw new Error('usePlugin must be used within PluginProvider');
  return context;
};