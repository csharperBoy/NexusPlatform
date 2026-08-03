// modules/DashboardCore/components/Header.tsx
import React, { useState, useRef, useEffect } from 'react';
import { useAuth } from '@/modules/Identity';
import { LogoutButton } from '@/modules/Identity/components/Buttons/LogoutButton';
import { useActiveModules } from "@/core/context/ModuleContext";

// آیکون‌ها (با هر کتابخانه‌ای می‌توانید جایگزین کنید)
import { 
  Search, Bell, User, Settings, LogOut, 
  ChevronDown, Menu 
} from 'lucide-react';

export interface HeaderProps {
  className?: string;
  render?: (user: ReturnType<typeof useAuth>['user']) => React.ReactNode;
}

export const Header: React.FC<HeaderProps> = ({ className = '', render }) => {
  const { activeModules } = useActiveModules();
  const authEnabled = activeModules.has("Authorization");
  const identityEnabled = activeModules.has("Identity");

  const { user } = useAuth();

  // State برای منوی پروفایل و اعلان‌ها
  const [isProfileOpen, setIsProfileOpen] = useState(false);
  const [isNotifOpen, setIsNotifOpen] = useState(false);
  const profileRef = useRef<HTMLDivElement>(null);
  const notifRef = useRef<HTMLDivElement>(null);

  // بستن منوها با کلیک بیرون
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (profileRef.current && !profileRef.current.contains(e.target as Node)) {
        setIsProfileOpen(false);
      }
      if (notifRef.current && !notifRef.current.contains(e.target as Node)) {
        setIsNotifOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  // اگر رندر سفارشی داده شده باشد
  if (render) {
    return <>{render(user)}</>;
  }

  // نام کاربر یا ایمیل برای آواتار
  const displayName = user?.userName || 'کاربر';
  const avatarLetter = displayName.charAt(0).toUpperCase();

  // تعداد اعلان‌های نمونه (می‌توانید از context یا props بگیرید)
  const notifCount = 3;

  return (
    <header className={`bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 shadow-sm px-4 sm:px-6 py-3 ${className}`}>
      <div className="flex items-center justify-between">
        {/* بخش چپ: لوگو / برند */}
        <div className="flex items-center gap-4">
          <div className="text-xl font-bold text-gray-800 dark:text-white">
            {/* لوگوی خود را قرار دهید */}
            <span className="hidden sm:inline">پنل مدیریت</span>
            <span className="sm:hidden">پنل</span>
          </div>
          {/* دکمه همبرگر برای منوی موبایل (اختیاری) */}
          <button className="lg:hidden p-2 rounded-md hover:bg-gray-100 dark:hover:bg-gray-700">
            <Menu className="w-5 h-5 text-gray-600 dark:text-gray-300" />
          </button>
        </div>

        {/* بخش میانی: جستجو (اختیاری) */}
        <div className="hidden md:flex items-center flex-1 max-w-md mx-4">
          <div className="relative w-full">
            <input
              type="text"
              placeholder="جستجو..."
              className="w-full pr-10 pl-4 py-2 bg-gray-100 dark:bg-gray-700 border border-transparent rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:text-white placeholder-gray-500 dark:placeholder-gray-400"
            />
            <Search className="absolute right-3 top-2.5 w-4 h-4 text-gray-400 dark:text-gray-500" />
          </div>
        </div>

        {/* بخش راست: اعلان‌ها + پروفایل */}
        <div className="flex items-center gap-3 sm:gap-4">
          {/* دکمه اعلان‌ها */}
          <div className="relative" ref={notifRef}>
            <button
              onClick={() => setIsNotifOpen(!isNotifOpen)}
              className="relative p-2 rounded-full hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
            >
              <Bell className="w-5 h-5 text-gray-600 dark:text-gray-300" />
              {notifCount > 0 && (
                <span className="absolute -top-0.5 -right-0.5 flex items-center justify-center w-4 h-4 text-[10px] font-bold text-white bg-red-500 rounded-full shadow-sm">
                  {notifCount}
                </span>
              )}
            </button>
            {/* منوی اعلان‌ها (نمونه) */}
            {isNotifOpen && (
              <div className="absolute left-0 mt-2 w-72 bg-white dark:bg-gray-800 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700 py-2 z-50">
                <div className="px-4 py-2 border-b border-gray-200 dark:border-gray-700">
                  <span className="font-semibold text-gray-700 dark:text-gray-200">اعلان‌ها</span>
                </div>
                <div className="max-h-60 overflow-y-auto">
                  <div className="px-4 py-3 hover:bg-gray-50 dark:hover:bg-gray-700 cursor-pointer">
                    <p className="text-sm text-gray-800 dark:text-gray-200">پیام جدید از ادمین</p>
                    <p className="text-xs text-gray-500 dark:text-gray-400">۲ دقیقه قبل</p>
                  </div>
                  <div className="px-4 py-3 hover:bg-gray-50 dark:hover:bg-gray-700 cursor-pointer">
                    <p className="text-sm text-gray-800 dark:text-gray-200">سیستم بروزرسانی شد</p>
                    <p className="text-xs text-gray-500 dark:text-gray-400">۱ ساعت قبل</p>
                  </div>
                </div>
                <div className="px-4 py-2 border-t border-gray-200 dark:border-gray-700 text-center">
                  <button className="text-sm text-blue-600 dark:text-blue-400 hover:underline">مشاهده همه</button>
                </div>
              </div>
            )}
          </div>

          {/* پروفایل کاربر */}
          <div className="relative" ref={profileRef}>
            <button
              onClick={() => setIsProfileOpen(!isProfileOpen)}
              className="flex items-center gap-2 px-2 py-1 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors focus:outline-none"
            >
              <div className="w-8 h-8 rounded-full bg-gradient-to-br from-blue-500 to-purple-600 flex items-center justify-center text-white font-bold text-sm shadow-sm">
                {avatarLetter}
              </div>
              <span className="hidden sm:inline-block text-sm font-medium text-gray-700 dark:text-gray-200">
                {displayName}
              </span>
              <ChevronDown className={`w-4 h-4 text-gray-500 dark:text-gray-400 transition-transform ${isProfileOpen ? 'rotate-180' : ''}`} />
            </button>

            {/* منوی کشویی پروفایل */}
            {isProfileOpen && (
              <div className="absolute left-0 mt-2 w-56 bg-white dark:bg-gray-800 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700 py-1 z-50">
                <div className="px-4 py-3 border-b border-gray-200 dark:border-gray-700">
                  <p className="text-sm font-medium text-gray-800 dark:text-white">{displayName}</p>
                </div>
                <button className="flex items-center gap-3 w-full px-4 py-2.5 text-sm text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                  <User className="w-4 h-4" />
                  <span>پروفایل</span>
                </button>
                <button className="flex items-center gap-3 w-full px-4 py-2.5 text-sm text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                  <Settings className="w-4 h-4" />
                  <span>تنظیمات</span>
                </button>
                <div className="border-t border-gray-200 dark:border-gray-700 mt-1 pt-1">
                  <LogoutButton className="flex items-center gap-3 w-full px-4 py-2.5 text-sm text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors" />
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  );
};