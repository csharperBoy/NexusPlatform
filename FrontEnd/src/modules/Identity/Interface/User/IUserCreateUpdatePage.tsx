// src/modules/Authorization/components/Interface/IUserCreateUpdatePage.tsx
import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '@/modules/Identity'; // فرض بر اینکه مسیر درست است
import { useUserCreateUpdateForm } from '../../hooks/Forms/User/useUserCreateUpdateForm'; // هوک یکپارچه شده
import LoadingIndicator from '@/core/components/LoadingIndicator'; // کامپوننت لودینگ
import { CreateUserCommand, UpdateUserCommand, UserFormCommand } from '../../models/UserCommands'; // مدل‌های یکپارچه شده

// --- تعریف اینترفیس‌ها ---

// اینترفیس Props مربوط به رندر کردن خود فرم
// دقت کنید که formData می‌تواند CreateUserCommand یا UpdateUserCommand باشد،
// اما هوک یکپارچه ما UserFormCommand را برمی‌گرداند که Union Type است.
// برای انعطاف‌پذیری بیشتر، اجازه می‌دهیم formData از نوع Union باشد.
// در کامپوننت فرم فرزند، بر اساس isEdit می‌توان نوع دقیق را مشخص کرد.
export interface RenderFormProps {
  formData: UserFormCommand; // هوک ما این نوع را برمی‌گرداند
  rolesList: { id: string; name: string }[];
  loading: boolean;
  error: string | null;
  handleChange: <K extends keyof UserFormCommand>(field: K, value: UserFormCommand[K]) => void;
  handleRolesChange: (roleId: string, checked: boolean) => void;
  handleSubmit: (e: React.FormEvent) => Promise<void>;
  isEdit: boolean; // اضافه شد برای اینکه بدانیم در حالت ویرایش هستیم یا ایجاد
}

// اینترفیس Props کامپوننت والد IUserCreateUpdatePage
export interface IUserCreateUpdatePageProps {
  formMode: 'create' | 'update'; // مشخص می‌کند که صفحه برای ایجاد است یا ویرایش
  redirectTo?: string; // مسیر بازگشت پس از عملیات موفق
  renderForm: (props: RenderFormProps) => React.ReactNode; // تابعی که فرم را رندر می‌کند
  loadingComponent?: React.ReactNode; // کامپوننت لودینگ سفارشی
}

// --- کامپوننت والد ---

export const IUserCreateUpdatePage: React.FC<IUserCreateUpdatePageProps> = ({
  formMode,
  redirectTo = '/users',
  renderForm,
  loadingComponent,
}) => {
  const { isAuthenticated, isLoading: isAuthLoading } = useAuth(); // استفاده از useAuth برای وضعیت احراز هویت
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>(); // دریافت id از URL در صورت وجود

  // تعیین userId بر اساس formMode
  const userId = formMode === 'update' ? id : undefined;

  // استفاده از هوک یکپارچه شده
  const formProps = useUserCreateUpdateForm(userId, () => navigate(redirectTo));

  // مدیریت نمایش لودینگ کلی صفحه
  // هم لودینگ احراز هویت و هم لودینگ بارگذاری اولیه فرم
  const isLoadingPage = isAuthLoading || (formMode === 'update' && formProps.loading);

  useEffect(() => {
    // بررسی وضعیت احراز هویت. اگر کاربر لاگین نیست، شاید بخواهید او را به صفحه لاگین هدایت کنید.
    // در این مثال، فرض می‌کنیم صفحه فقط برای کاربران لاگین شده قابل دسترسی است.
    if (!isAuthLoading && !isAuthenticated) {
       // navigate('/login'); // هدایت به صفحه لاگین
       console.warn("User is not authenticated. Redirecting to login might be needed.");
    }
  }, [isAuthenticated, isAuthLoading, navigate]);


  // اگر صفحه در حال بارگذاری اولیه است (احراز هویت یا داده‌های فرم)
  if (isLoadingPage) {
    return loadingComponent ? <>{loadingComponent}</> : <LoadingIndicator />;
  }

  // اگر خطایی در بارگذاری اولیه رخ داده و داده‌ای نداریم (مخصوصاً در حالت ویرایش)
  if (formProps.error && formMode === 'update' && !formProps.formData) {
     return <div className="text-red-500 p-4">خطا در بارگذاری اطلاعات کاربر: {formProps.error}</div>;
  }

  // اگر در حالت ویرایش هستیم ولی userId نداریم (اشکال در URL یا منطق)
  if (formMode === 'update' && !id) {
      return <div className="text-red-500 p-4">شناسه کاربر نامعتبر است.</div>;
  }

  // رندر کردن فرم با استفاده از تابع renderForm که از props دریافت شده
  // توجه: formData از هوک ما UserFormCommand است.
  // اگر renderForm انتظار نوع دقیق‌تری داشت، باید مدیریت می‌کردیم،
  // اما چون هوک ما هر دو حالت را پوشش می‌دهد، این Union Type مناسب است.
  return <>{renderForm({ ...formProps, isEdit: formMode === 'update' })}</>;
};
