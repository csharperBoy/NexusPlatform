// modules/DashboardCore/contexts/DashboardProvider.tsx

import React, { ReactNode } from 'react';
import { PluginProvider } from './PluginContext';
import { MenuProvider } from './MenuContext';

export const DashboardProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  return (
    <PluginProvider>
      <MenuProvider>
        {children}
      </MenuProvider>
    </PluginProvider>
  );
};

export { usePlugin } from './PluginContext';
export { useMenu } from './MenuContext';