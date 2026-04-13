// modules/DashboardCore/components/Header.tsx
import React from 'react';
import { useAuth } from '@/modules/Identity';
import { LogoutButton } from '@/modules/Identity/components/Buttons/LogoutButton';

export interface HeaderProps {
  className?: string;
  /** امکان سفارشی‌سازی کامل هدر */
  render?: (user: ReturnType<typeof useAuth>['user']) => React.ReactNode;
}

export const Header: React.FC<HeaderProps> = ({ className = '', render }) => {
  const { user } = useAuth();

  if (render) {
    return <>{render(user)}</>;
  }

  return (
    <header className={`bg-white shadow-sm p-4 flex justify-between items-center ${className}`}>
      <div className="text-gray-700">
        خوش آمدید، <span className="font-semibold">{user?.userName || 'کاربر'}</span>
      </div>
      <div className="flex items-center gap-4">
        {/* می‌توانید آیتم‌های دیگری مثل اعلان‌ها اضافه کنید */}
        <LogoutButton className="px-4 py-2 bg-red-500 text-red rounded hover:bg-red-600" />
      </div>
    </header>
  );
};