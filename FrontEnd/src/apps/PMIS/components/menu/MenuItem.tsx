//src/components/menu/MenuItem.tsx
import React, { useState } from 'react';
import { NavLink } from 'react-router-dom';
import { IconType } from 'react-icons';
import { FaChevronDown } from 'react-icons/fa';

interface MenuItemProps {
  onClick?: () => void;
  catalog: string;
  collapsed: boolean;
  listItems: Array<{
    isLink: boolean;
    url?: string;
    icon: IconType;
    label: string;
    onClick?: () => void;
  }>;
}

const MenuItem: React.FC<MenuItemProps> = ({
  onClick,
  catalog,
  listItems,
  collapsed,
}) => {
  const [open, setOpen] = useState(true);

  const toggle = () => setOpen((prev) => !prev);

  return (
    <div className="w-full flex flex-col items-stretch">
      {/* عنوان دسته‌بندی به‌صورت آکاردئونی */}
      <button
        onClick={toggle}
        aria-expanded={open}
        aria-controls={`menu-items-${catalog}`}
        aria-label={`${open ? 'بستن' : 'باز کردن'} دسته‌بندی ${catalog}`}
        className={`flex items-center px-2 py-2 text-xs font-bold uppercase
          text-gray-500 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800
          transition ${collapsed ? 'justify-center' : 'justify-between'}`}
      >
        {!collapsed ? (
          <>
            <span>{catalog}</span>
            <FaChevronDown
              className={`transition-transform duration-300 text-sm ${
                open ? 'rotate-0' : '-rotate-90'
              }`}
              aria-hidden="true"
            />
          </>
        ) : (
          <FaChevronDown
            className={`transition-transform duration-300 text-xs ${
              open ? 'rotate-0' : '-rotate-90'
            }`}
            aria-label={`${open ? 'باز' : 'بسته'} شدن دسته‌بندی`}
            aria-hidden="true"
          />
        )}
      </button>

      {/* آیتم‌های داخل آکاردئون */}
      <div
        id={`menu-items-${catalog}`}
        className={`flex flex-col gap-2 px-1 transition-all duration-300 overflow-hidden ${
          open ? 'max-h-[1000px] opacity-100' : 'max-h-0 opacity-0'
        }`}
      >
        {listItems.map((listItem, index) => {
          const Icon = listItem.icon;

          const commonClasses = `
            btn btn-ghost btn-block px-2 
            2xl:min-h-[52px] 3xl:min-h-[64px] 
            ${collapsed ? 'justify-center gap-0' : 'justify-start gap-3'}
            dark:text-gray-300 dark:hover:bg-gray-700
          `;

          const iconContainer = (
            <div className="min-w-[36px] h-[36px] flex items-center justify-center">
              <Icon className="text-[22px] 2xl:text-[26px] 3xl:text-[30px]" />
            </div>
          );

          const labelSpan = !collapsed && (
            <span className="text-sm 2xl:text-base 3xl:text-lg capitalize">
              {listItem.label}
            </span>
          );

          const tooltip = collapsed ? { title: listItem.label } : {};

          if (listItem.isLink) {
            return (
              <NavLink
                key={index}
                onClick={onClick}
                to={listItem.url || ''}
                className={({ isActive }) =>
                  isActive
                    ? `${commonClasses} btn-active`
                    : commonClasses
                }
                {...tooltip}
              >
                {iconContainer}
                {labelSpan}
              </NavLink>
            );
          } else {
            return (
              <button
                key={index}
                onClick={listItem.onClick}
                className={commonClasses}
                {...tooltip}
              >
                {iconContainer}
                {labelSpan}
              </button>
            );
          }
        })}
      </div>
    </div>
  );
};

export default MenuItem;
