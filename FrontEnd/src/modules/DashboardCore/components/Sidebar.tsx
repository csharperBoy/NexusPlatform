// modules/DashboardCore/components/Sidebar.tsx
import React, { useState } from 'react';
import { NavLink } from 'react-router-dom';
// ۱. ایمپورت از Context به جای hook قدیمی
import { useMenu } from '@/core/context/MenuContext'; 
import { getIconComponent } from '@/core/components/IconMapper';
import { MenuDto } from '@/core/models/Menu';
import { ChevronDown, LayoutDashboard } from 'lucide-react';

const MenuItem: React.FC<{ item: MenuDto; depth: number }> = ({ item, depth }) => {
  const [isOpen, setIsOpen] = useState(true);
  const hasChildren = item.children && item.children.length > 0;

  if (hasChildren) {
    return (
      <div className="mb-1">
        <button
          onClick={() => setIsOpen(!isOpen)}
          className="flex items-center justify-between w-full px-4 py-2.5 text-sm font-medium text-gray-700 dark:text-gray-200 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700/50 transition-colors duration-200"
          style={{ paddingRight: depth * 16 + 16 }}
        >
          <span className="flex items-center gap-3">
            {getIconComponent(item.icon)}
            <span>{item.title}</span>
          </span>
          <ChevronDown
            className={`w-4 h-4 transition-transform duration-200 ${
              isOpen ? 'rotate-180' : ''
            }`}
          />
        </button>
        {isOpen && (
          <div className="mt-1 space-y-1">
            {item.children
              ?.sort((a, b) => a.order - b.order)
              .map((child) => (
                <MenuItem key={child.id} item={child} depth={depth + 1} />
              ))}
          </div>
        )}
      </div>
    );
  }

  return (
    <NavLink
      to={item.path}
      className={({ isActive }) =>
        `flex items-center gap-3 px-4 py-2.5 text-sm font-medium rounded-lg transition-all duration-200 ${
          isActive
            ? 'bg-blue-50 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300 shadow-sm'
            : 'text-gray-700 dark:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-700/50'
        }`
      }
      style={{ paddingRight: depth * 16 + 16 }}
    >
      {getIconComponent(item.icon)}
      <span>{item.title}</span>
    </NavLink>
  );
};

export const Sidebar: React.FC = () => {
  // ۲. دریافت isMenuLoading و تغییر نام آن به loading جهت عدم تغییر باقی کد
  const { menus, isMenuLoading: loading } = useMenu();

  if (loading) {
    return (
      <aside className="w-64 bg-white dark:bg-gray-800 border-l border-gray-200 dark:border-gray-700 min-h-screen p-4">
        <div className="animate-pulse space-y-3">
          <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4"></div>
          <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
          <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-2/3"></div>
        </div>
      </aside>
    );
  }

  return (
    <aside className="w-64 bg-white dark:bg-gray-800 border-l border-gray-200 dark:border-gray-700 min-h-screen flex flex-col">
      <div className="flex items-center gap-3 px-4 py-4 border-b border-gray-200 dark:border-gray-700">
        <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-blue-500 to-purple-600 flex items-center justify-center text-white shadow-sm">
          <LayoutDashboard className="w-5 h-5" />
        </div>
        <span className="text-lg font-bold text-gray-800 dark:text-white">
          پنل مدیریت
        </span>
      </div>

      <nav className="flex-1 overflow-y-auto p-3 space-y-1">
        {menus
          ?.sort((a, b) => a.order - b.order)
          .map((item) => (
            <MenuItem key={item.id} item={item} depth={0} />
          ))}
      </nav>

      <div className="p-4 border-t border-gray-200 dark:border-gray-700 text-xs text-gray-400 dark:text-gray-500 text-center">
        نسخه ۱.۰.۰
      </div>
    </aside>
  );
};