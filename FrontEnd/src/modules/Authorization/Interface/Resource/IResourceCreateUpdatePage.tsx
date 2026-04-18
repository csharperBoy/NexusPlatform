// src/modules/Authorization/components/Interface/IResourceCreateUpdatePage.tsx
import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '@/modules/Identity'; // فرض بر اینکه مسیر درست است
import { useResourceCreateUpdateForm } from '../../hooks/Forms/Resource/useResourceCreateUpdateForm'; // هوک یکپارچه شده
import LoadingIndicator from '@/core/components/LoadingIndicator'; // کامپوننت لودینگ
import { CreateResourceCommand, UpdateResourceCommand, ResourceFormCommand } from '../../models/ResourceCommands'; // مدل‌های یکپارچه شده

// --- تعریف اینترفیس‌ها ---


export interface RenderFormProps {
  formData: ResourceFormCommand; // هوک ما این نوع را برمی‌گرداند
  loading: boolean;
  error: string | null;
  handleChange: <K extends keyof ResourceFormCommand>(field: K, value: ResourceFormCommand[K]) => void;
  handleSubmit: (e: React.FormEvent) => Promise<void>;
  isEdit: boolean; // اضافه شد برای اینکه بدانیم در حالت ویرایش هستیم یا ایجاد
}

// اینترفیس Props کامپوننت والد IResourceCreateUpdatePage
export interface IResourceCreateUpdatePageProps {
  formMode: 'create' | 'update'; // مشخص می‌کند که صفحه برای ایجاد است یا ویرایش
  redirectTo?: string; // مسیر بازگشت پس از عملیات موفق
  renderForm: (props: RenderFormProps) => React.ReactNode; // تابعی که فرم را رندر می‌کند
  loadingComponent?: React.ReactNode; // کامپوننت لودینگ سفارشی
}

// --- کامپوننت والد ---

export const IResourceCreateUpdatePage: React.FC<IResourceCreateUpdatePageProps> = ({
  formMode,
  redirectTo = '/resources',
  renderForm,
  loadingComponent,
}) => {
  const { isAuthenticated, isLoading: isAuthLoading } = useAuth(); // استفاده از useAuth برای وضعیت احراز هویت
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>(); // دریافت id از URL در صورت وجود

  // تعیین resourceId بر اساس formMode
  const resourceId = formMode === 'update' ? id : undefined;

  // استفاده از هوک یکپارچه شده
  const formProps = useResourceCreateUpdateForm(resourceId, () => navigate(redirectTo));

  // مدیریت نمایش لودینگ کلی صفحه
  // هم لودینگ احراز هویت و هم لودینگ بارگذاری اولیه فرم
  const isLoadingPage = isAuthLoading || (formMode === 'update' && formProps.loading);

  useEffect(() => {
    // بررسی وضعیت احراز هویت. اگر کاربر لاگین نیست، شاید بخواهید او را به صفحه لاگین هدایت کنید.
    // در این مثال، فرض می‌کنیم صفحه فقط برای کاربران لاگین شده قابل دسترسی است.
    if (!isAuthLoading && !isAuthenticated) {
       // navigate('/login'); // هدایت به صفحه لاگین
       console.warn("Resource is not authenticated. Redirecting to login might be needed.");
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

  // اگر در حالت ویرایش هستیم ولی resourceId نداریم (اشکال در URL یا منطق)
  if (formMode === 'update' && !id) {
      return <div className="text-red-500 p-4">شناسه کاربر نامعتبر است.</div>;
  }

  return <>{renderForm({ ...formProps, isEdit: formMode === 'update' })}</>;
};
