// src/modules/DashboardCore/components/MainLayout.tsx
import React, { ReactNode } from 'react';
import { Sidebar } from './Sidebar';
import { Header } from './Header';

export interface MainLayoutProps {
  children: ReactNode;
  sidebarClassName?: string;
  headerClassName?: string;
  contentClassName?: string;
  render?: (props: {
    sidebar: ReactNode;
    header: ReactNode;
    content: ReactNode;
  }) => ReactNode;
}

export const MainLayout: React.FC<MainLayoutProps> = ({
  children,
  sidebarClassName,
  headerClassName,
  contentClassName = 'p-4',
  render,
}) => {
  const sidebar = <Sidebar className={sidebarClassName} />;
  const header = <Header className={headerClassName} />;
  const content = (
    <main className={`flex-1 overflow-y-auto ${contentClassName}`}>
      {children}
    </main>
  );

  if (render) {
    return <>{render({ sidebar, header, content })}</>;
  }

  return (
    <div className="flex h-screen bg-gray-100">
      {sidebar}
      <div className="flex-1 flex flex-col overflow-hidden">
        {header}
        {content}
      </div>
    </div>
  );
};