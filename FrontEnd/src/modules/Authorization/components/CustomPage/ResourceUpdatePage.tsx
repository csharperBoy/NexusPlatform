// src/modules/Authorization/components/CustomPage/ResourceUpdatePage.tsx
import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '@/modules/Identity';
import { useResourceUpdateForm } from '../../hooks/Forms/useResourceUpdateForm';
import LoadingIndicator from '@/core/components/LoadingIndicator';
import type { UpdateResourceRequest } from '../../models/UpdateResourceRequest';

export interface RenderFormProps {
  formData: UpdateResourceRequest | null; 
  loading: boolean;
  error: string | null;
  initialLoading: boolean; // اضافه شد
  handleChange: (field: keyof UpdateResourceRequest, value: any) => void;
  handleSubmit: (e: React.FormEvent) => Promise<void>;
}

export interface ResourceUpdatePageProps { // نام به ResourceUpdatePage تغییر یافت
  redirectTo?: string;
  renderForm: (props: RenderFormProps) => React.ReactNode;
  loadingComponent?: React.ReactNode;
}

export const ResourceUpdatePage: React.FC<ResourceUpdatePageProps> = ({ // نام به ResourceUpdatePage تغییر یافت
  redirectTo = '/resources',
  renderForm,
  loadingComponent,
}) => {
  const { isAuthenticated, isLoading: isAuthLoading } = useAuth();
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>(); // دریافت id از URL

  // هوک را با id پاس می‌دهیم (اگر وجود داشته باشد)
  const formProps = useResourceUpdateForm(() => navigate(redirectTo));

  // این useEffect دیگر لازم نیست، منطق بارگذاری در هوک انجام می‌شود
  // useEffect(() => {
  //   if (!isAuthLoading && isAuthenticated) {
  //     //
  //   }
  // }, [isAuthenticated, isAuthLoading, navigate]);

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

// نام کامپوننت wrapper را هم می‌توانیم تغییر دهیم اگر لازم است
 export const ResourceUpdateWithCustomForm: React.FC<ResourceUpdatePageProps> = ResourceUpdatePage;
