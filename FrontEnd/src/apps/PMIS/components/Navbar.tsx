import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { HiBars3CenterLeft } from 'react-icons/hi2';
import { RxEnterFullScreen, RxExitFullScreen } from 'react-icons/rx';
import { useAuth } from '../contexts/AuthContext';
import ChangeThemes from './ChangesThemes';
import { getMenus } from '../api/SystemCollection';
import type { MenuCatalogDto, MenuItemDto } from '../models/System/GetMenuResponse';
import MenuItem from './menu/MenuItem';
import * as HiIcons from 'react-icons/hi2';
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

const Navbar = () => {
  const [menu, setMenu] = React.useState<MenuCatalogDto[]>([]);
  const [isFullScreen, setIsFullScreen] = React.useState(false); // تغییر به false
  const [isDrawerOpen, setDrawerOpen] = React.useState(false);
  const [collapsed, setCollapsed] = React.useState(false);
  const { logout } = useAuth(); 
  const navigate = useNavigate();

  const toggleDrawer = () => setDrawerOpen(!isDrawerOpen);
  
  const toggleFullScreen = () => {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen().then(() => {
        setIsFullScreen(true);
      });
    } else {
      document.exitFullscreen().then(() => {
        setIsFullScreen(false);
      });
    }
  };

  React.useEffect(() => {
    const fetchMenu = async () => {
      try {
        const data = await getMenus();
        setMenu(Array.isArray(data) ? data : []); // اضافه کردن بررسی آرایه
      } catch (error) {
        console.error('خطا در دریافت منو:', error);
        setMenu([]); // تنظیم آرایه خالی در صورت خطا
      }
    };

    fetchMenu();
    
    if (collapsed) {
      console.log('Sidebar collapsed');
      setCollapsed(collapsed); 
    }
  }, [collapsed]);

  return (
    <div className="fixed z-[3] top-0 left-0 right-0 bg-base-100 shadow-md border-b border-base-300 w-full flex justify-between px-3 xl:px-4 py-3 xl:py-5 gap-4 xl:gap-0">
      {/* Left container */}
      <div className="flex gap-3 items-center">
        {/* Mobile drawer */}
        <div className="drawer w-auto p-0 mr-1 xl:hidden">
          <input
            id="drawer-navbar-mobile"
            type="checkbox"
            className="drawer-toggle"
            checked={isDrawerOpen}
            onChange={toggleDrawer}
          />
          <div className="p-0 w-auto drawer-content">
            <label
              htmlFor="drawer-navbar-mobile"
              className="p-0 btn btn-ghost drawer-button"
            >
              <HiBars3CenterLeft className="text-2xl" />
            </label>
          </div>
          <div className="drawer-side z-[99]">
            <label
              htmlFor="drawer-navbar-mobile"
              aria-label="close sidebar"
              className="drawer-overlay"
            ></label>
            <div className="menu p-4 w-auto min-h-full bg-base-200 text-base-content">
              <Link
                to="/"
                className="flex items-center gap-1 xl:gap-2 mt-1 mb-5"
                onClick={toggleDrawer}
              >
                <img
                  src="/logo.png"
                  alt="PMIS Logo"
                  className="w-8 h-8 sm:w-10 sm:h-10 xl:w-10 xl:h-10 text-primary"
                />
                <span className="text-[16px] sm:text-lg xl:text-xl 2xl:text-2xl font-semibold">
                  PMIS
                </span>
              </Link>
              {/* اضافه کردن بررسی وجود منو */}
              {Array.isArray(menu) && menu.map((item, index) => (
                <MenuItem
                  key={index}
                  catalog={item.catalog}
                  listItems={item.listItems?.map((item: MenuItemDto) => ({
                    ...item,
                    icon: iconMap[item.icon] ?? HiIcons.HiOutlineHome,
                  })) || []}
                  onClick={toggleDrawer}
                  collapsed={collapsed}
                />
              ))}
            </div>
          </div>
        </div>

        {/* Logo */}
        <Link to="/" className="flex items-center gap-1 xl:gap-2">
          <img
            src="/logo.png"
            alt="PMIS Logo"
            className="w-8 h-8 sm:w-10 sm:h-10 xl:w-10 xl:h-10 text-primary"
          />
          <span className="text-[16px] sm:text-lg xl:text-xl 2xl:text-2xl font-semibold">
            PMIS
          </span>
        </Link>
      </div>

      {/* Right navbar items */}
      <div className="flex items-center gap-0 xl:gap-1 2xl:gap-2 3xl:gap-5">
        <button
          onClick={toggleFullScreen}
          className="hidden xl:inline-flex btn btn-circle btn-ghost"
        >
          {isFullScreen ? (
            <RxExitFullScreen className="xl:text-xl 2xl:text-2xl 3xl:text-3xl" />
          ) : (
            <RxEnterFullScreen className="xl:text-xl 2xl:text-2xl 3xl:text-3xl" />
          )}
        </button>

        <div className="px-0 xl:px-auto btn btn-circle btn-ghost xl:mr-1">
          <ChangeThemes />
        </div>

        <div className="dropdown dropdown-end">
          <div
            tabIndex={0}
            role="button"
            className="btn btn-ghost btn-circle avatar"
          >
            <div className="w-9 rounded-full">
              <img
                src="https://avatars.githubusercontent.com/u/74099030?v=4"
                alt="avatar"
              />
            </div>
          </div>
          <ul
            tabIndex={0}
            className="mt-3 z-[1] p-2 shadow menu menu-sm dropdown-content bg-base-100 rounded-box w-40"
          >
            <li>
              <Link to="/ChangePassword" onClick={() => (document.activeElement as HTMLElement)?.blur()}>
                تغییر رمز عبور
              </Link>
            </li>
            <li>
              <a onClick={async () => {
                await logout();
                navigate('/login');
              }}>
                خروج
              </a>
            </li>
          </ul>
        </div>
      </div>
    </div>
  );
};

export default Navbar;