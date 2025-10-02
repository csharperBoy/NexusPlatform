// src\apps\PMIS\components\menu\Menu.tsx
import React, { useState, useEffect } from 'react';
// import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import MenuItem from './MenuItem';
import { FaBars } from 'react-icons/fa';
import * as HiIcons from 'react-icons/hi2';
import { getMenus } from '../../api/SystemCollection';
import type { MenuCatalogDto, MenuItemDto } from '../../models/System/GetMenuResponse';
import type { IconType } from 'react-icons';

const iconMap: Record<string, IconType> = {
  HiOutlineHome: HiIcons.HiOutlineHome,
  HiOutlineUser: HiIcons.HiOutlineUser,
  HiOutlineUsers: HiIcons.HiOutlineUsers,
  HiOutlineCube: HiIcons.HiOutlineCube,
  HiOutlineClipboardDocumentList: HiIcons.HiOutlineClipboardDocumentList,
  HiOutlineCalendarDays: HiIcons.HiOutlineCalendarDays,
  HiOutlinePresentationChartBar: HiIcons.HiOutlinePresentationChartBar,
  HiOutlineDocumentText: HiIcons.HiOutlineDocumentText,
  HiOutlinePencilSquare: HiIcons.HiOutlinePencilSquare,
  HiOutlineArrowLeftOnRectangle: HiIcons.HiOutlineArrowLeftOnRectangle,
};

const Menu: React.FC = () => {
  const { isAuthenticated } = useAuth();
  // const navigate = useNavigate();
  const [collapsed, setCollapsed] = useState(false);
  const [menus, setMenus] = useState<MenuCatalogDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!isAuthenticated) return;
    setLoading(true);
    getMenus()
      .then((data) => {
        setMenus(Array.isArray(data) ? data : []);
      })
      .catch((err) => {
        console.error('Failed to load menus', err);
      })
      .finally(() => setLoading(false));
      console.log(menus);
  }, [isAuthenticated]);

  return (
      <div
        className={`transition-all duration-300 ${collapsed ? 'w-[60px]' : 'w-[240px]'} min-h-screen bg-white shadow-lg dark:bg-gray-900 dark:shadow-xl`}
      >
     {/* <div
   className={`hidden xl:block fixed right-0 top-[64px] transition-all duration-300 ${collapsed ? 'w-[60px]' : 'w-[240px]'} h-[calc(100vh-64px)] bg-white shadow-lg dark:bg-gray-900 dark:shadow-xl z-[2]`}
 > */}

      {/* Toggle Button */}
      <div className="flex justify-start p-3">
        <button
          onClick={() => setCollapsed(!collapsed)}
          className="text-gray-600 hover:text-black dark:text-gray-300 dark:hover:text-white p-2"
          aria-label="Toggle menu collapse"
        >
          <FaBars size={20} />
        </button>
      </div>

      {/* Menu List */}
      <div className="flex flex-col gap-4 px-2">
        {!loading &&
          menus.map((cat, idx) => (
            <MenuItem
              key={idx}
              catalog={cat.catalog}
              collapsed={collapsed}
              listItems={cat.listItems.map((item: MenuItemDto) => ({
                ...item,
                icon: iconMap[item.icon] ?? HiIcons.HiOutlineHome,
              }))}
            />
          ))}
      </div>
    </div>
  );
};

export default Menu;
