//src/modules/Identity/components/CustomPage/LoginPage.tsx
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useLoginForm } from '../../hooks/Forms/useLoginForm';
import LoadingIndicator from '@/core/components/LoadingIndicator'; 

export interface RenderFormProps {
  identifier: string;
  password: string;
  loading: boolean;
  error: string | null;
  setIdentifier: (value: string) => void;
  setPassword: (value: string) => void;
  handleSubmit: (e: React.FormEvent) => Promise<void>;
}

export interface LoginPageWithCustomFormProps {
  /** مسیر هدایت پس از لاگین موفق (پیش‌فرض: "/dashboard") */
  redirectTo?: string;
  /** تابعی که فرم سفارشی را با props داده شده رندر می‌کند */
  renderForm: (props: RenderFormProps) => React.ReactNode;
  /** کامپوننت بارگذاری سفارشی (اختیاری) */
  loadingComponent?: React.ReactNode;
}

export const LoginPageWithCustomForm: React.FC<LoginPageWithCustomFormProps> = ({
  redirectTo = '/dashboard',
  renderForm,
   loadingComponent,
}) => {
  const { isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();
  const formProps = useLoginForm(); // بدون onSuccess

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      navigate(redirectTo);
    }
  }, [isAuthenticated, isLoading, navigate, redirectTo]);

  if (isLoading) {
    return loadingComponent ? <>{loadingComponent}</> : <LoadingIndicator />;
  }
  
  if (isAuthenticated) {
    return null; // یا می‌توانید یک اسپینر نشان دهید
  }

  return <>{renderForm(formProps)}</>;
};