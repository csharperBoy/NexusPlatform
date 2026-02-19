// modules/DashboardCore/components/Sidebar.tsx
import React from 'react';
import { NavLink } from 'react-router-dom';
import { useMenu } from '../contexts/MenuContext';
import { MenuItem } from '../types';

export interface SidebarProps {
  className?: string;
  /** امکان سفارشی‌سازی کامل سایدبار با دریافت لیست آیتم‌های منو */
  render?: (menuItems: MenuItem[]) => React.ReactNode;
}

export const Sidebar: React.FC<SidebarProps> = ({ className = '', render }) => {
  const { getMenuItems } = useMenu();
  const menuItems = getMenuItems();

  if (render) {
    return <>{render(menuItems)}</>;
  }

  return (
    <aside className={`bg-gray-800 text-white w-64 flex-shrink-0 ${className}`}>
      <div className="p-4 text-xl font-bold border-b border-gray-700">
        پنل مدیریت
      </div>
      <nav className="mt-4">
        {menuItems.map((item: MenuItem) => (
          <NavLink
            key={item.id}
            to={item.path}
            className={({ isActive }) =>
              `flex items-center py-3 px-4 hover:bg-gray-700 transition-colors ${
                isActive ? 'bg-gray-700' : ''
              }`
            }
          >
            {item.icon && <span className="ml-3">{item.icon}</span>}
            <span>{item.title}</span>
          </NavLink>
        ))}
      </nav>
    </aside>
  );
};