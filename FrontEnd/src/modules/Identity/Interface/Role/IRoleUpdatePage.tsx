// src/modules/Identity/components/Interface/IRoleUpdatePage.tsx
import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '@/modules/Identity';
import { useRoleUpdateForm } from '../../hooks/Forms/Role/useRoleUpdateForm';
import LoadingIndicator from '@/core/components/LoadingIndicator';
import type { UpdateRoleCommand } from '../../models/UpdateRoleCommand';

export interface RenderFormProps {
  formData: UpdateRoleCommand | null; 
  loading: boolean;
  error: string | null;
  initialLoading: boolean; // اضافه شد
  handleChange: (field: keyof UpdateRoleCommand, value: any) => void;
  handleSubmit: (e: React.FormEvent) => Promise<void>;
}

export interface IRoleUpdatePageProps { // نام به RoleUpdatePage تغییر یافت
  redirectTo?: string;
  renderForm: (props: RenderFormProps) => React.ReactNode;
  loadingComponent?: React.ReactNode;
}

export const RoleUpdateForm: React.FC<IRoleUpdatePageProps> = ({ // نام به RoleUpdatePage تغییر یافت
  redirectTo = '/roles',
  renderForm,
  loadingComponent,
}) => {
  const { isAuthenticated, isLoading: isAuthLoading } = useAuth();
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>(); // دریافت id از URL

  // هوک را با id پاس می‌دهیم (اگر وجود داشته باشد)
  const formProps = useRoleUpdateForm(() => navigate(redirectTo));

  // نمایش لودینگ در صورت بارگذاری اولیه اطلاعات یا بارگذاری احراز هویت
  if (isAuthLoading || formProps.initialLoading) {
    return loadingComponent ? <>{loadingComponent}</> : <LoadingIndicator />;
  }

  // نمایش خطا اگر در حین بارگذاری اولیه اتفاق افتاد
  if (formProps.error && !formProps.formData) {
     // می‌توانید یک کامپوننت خطا سفارشی نمایش دهید
     return <div>خطا: {formProps.error}</div>;
  }

  // اگر formData هنوز null است ولی خطا نداریم، همچنان در حالت بارگذاری هستیم (یا id نامعتبر بود)
  if (!formProps.formData && !formProps.error) {
      // این حالت نباید اتفاق بیفتد مگر اینکه id نامعتبر باشد یا مشکلی در منطق باشد
      return <div>در حال آماده‌سازی فرم...</div>;
  }


  // رندر کردن فرم با تمام پراپ‌ها، از جمله formData که اکنون پر شده است
  // اگر id وجود ندارد، یعنی حالت افزودن است و formData مقادیر اولیه خواهد داشت
  return <>{renderForm(formProps)}</>;
};
