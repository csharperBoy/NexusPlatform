// src/modules/Authorization/components/Interface/IPermissionCreateUpdatePage.tsx
import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '@/modules/Identity'; // فرض بر اینکه مسیر درست است
import { usePermissionCreateUpdateForm } from '../../hooks/Forms/Permission/usePermissionCreateUpdateForm'; // هوک یکپارچه شده
import LoadingIndicator from '@/core/components/LoadingIndicator'; // کامپوننت لودینگ
import { CreatePermissionCommand, UpdatePermissionCommand, PermissionFormCommand } from '../../models/PermissionCommands'; // مدل‌های یکپارچه شده
import { SelectionListDto } from '@/core/models/SelectionListDto';

// --- تعریف اینترفیس‌ها ---

// اینترفیس Props مربوط به رندر کردن خود فرم
// دقت کنید که formData می‌تواند CreatePermissionCommand یا UpdatePermissionCommand باشد،
// اما هوک یکپارچه ما PermissionFormCommand را برمی‌گرداند که Union Type است.
// برای انعطاف‌پذیری بیشتر، اجازه می‌دهیم formData از نوع Union باشد.
// در کامپوننت فرم فرزند، بر اساس isEdit می‌توان نوع دقیق را مشخص کرد.
export interface RenderFormProps {
  formData: PermissionFormCommand; // هوک ما این نوع را برمی‌گرداند
  scopesList: { value: number; display: string }[];
  resourceList: SelectionListDto[];  
  assignList  : SelectionListDto[];  
  loading: boolean;
  error: string | null;
  handleChange: <K extends keyof PermissionFormCommand>(field: K, value: PermissionFormCommand[K]) => void;
  handleScopesChange: (scopeValue: number, checked: boolean) => void;
  handleSubmit: (e: React.FormEvent) => Promise<void>;
  handleAssignTypeChange: (newAssignType: number)  => void;
  isEdit: boolean; // اضافه شد برای اینکه بدانیم در حالت ویرایش هستیم یا ایجاد
}

// اینترفیس Props کامپوننت والد IPermissionCreateUpdatePage
export interface IPermissionCreateUpdatePageProps {
  formMode: 'create' | 'update'; // مشخص می‌کند که صفحه برای ایجاد است یا ویرایش
  redirectTo?: string; // مسیر بازگشت پس از عملیات موفق
  renderForm: (props: RenderFormProps) => React.ReactNode; // تابعی که فرم را رندر می‌کند
  loadingComponent?: React.ReactNode; // کامپوننت لودینگ سفارشی
}

// --- کامپوننت والد ---

export const IPermissionCreateUpdatePage: React.FC<IPermissionCreateUpdatePageProps> = ({
  formMode,
  redirectTo = '/permissions',
  renderForm,
  loadingComponent,
}) => {
  const { isAuthenticated, isLoading: isAuthLoading } = useAuth(); // استفاده از useAuth برای وضعیت احراز هویت
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>(); // دریافت id از URL در صورت وجود

  // تعیین permissionId بر اساس formMode
  const permissionId = formMode === 'update' ? id : undefined;

  // استفاده از هوک یکپارچه شده
  const formProps = usePermissionCreateUpdateForm(permissionId, () => navigate(redirectTo));

  // مدیریت نمایش لودینگ کلی صفحه
  // هم لودینگ احراز هویت و هم لودینگ بارگذاری اولیه فرم
  const isLoadingPage = isAuthLoading || (formMode === 'update' && formProps.loading);

  useEffect(() => {
    // بررسی وضعیت احراز هویت. اگر کاربر لاگین نیست، شاید بخواهید او را به صفحه لاگین هدایت کنید.
    // در این مثال، فرض می‌کنیم صفحه فقط برای کاربران لاگین شده قابل دسترسی است.
    if (!isAuthLoading && !isAuthenticated) {
       // navigate('/login'); // هدایت به صفحه لاگین
       console.warn("Permission is not authenticated. Redirecting to login might be needed.");
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

  // اگر در حالت ویرایش هستیم ولی permissionId نداریم (اشکال در URL یا منطق)
  if (formMode === 'update' && !id) {
      return <div className="text-red-500 p-4">شناسه کاربر نامعتبر است.</div>;
  }

  // رندر کردن فرم با استفاده از تابع renderForm که از props دریافت شده
  // توجه: formData از هوک ما PermissionFormCommand است.
  // اگر renderForm انتظار نوع دقیق‌تری داشت، باید مدیریت می‌کردیم،
  // اما چون هوک ما هر دو حالت را پوشش می‌دهد، این Union Type مناسب است.
  return <>{renderForm({ ...formProps, isEdit: formMode === 'update' })}</>;
};
