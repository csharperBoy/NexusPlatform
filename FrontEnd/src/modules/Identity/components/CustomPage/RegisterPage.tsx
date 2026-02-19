//src/modules/Identity/components/CustomPage/RegisterPage.tsx
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useRegisterForm } from '../../hooks/Forms/useRegisterForm';
import LoadingIndicator from '@/core/components/LoadingIndicator'; 

export interface RenderRegisterFormProps {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  displayName: string;
  loading: boolean;
  error: string | null;
  setUsername: (value: string) => void;
  setEmail: (value: string) => void;
  setPassword: (value: string) => void;
  setConfirmPassword: (value: string) => void;
  setDisplayName: (value: string) => void;
  handleSubmit: (e: React.FormEvent) => Promise<void>;
}

export interface RegisterPageWithCustomFormProps {
  redirectTo?: string;
  renderForm: (props: RenderRegisterFormProps) => React.ReactNode;
    /** کامپوننت بارگذاری سفارشی (اختیاری) */
    loadingComponent?: React.ReactNode;
}

export const RegisterPageWithCustomForm: React.FC<RegisterPageWithCustomFormProps> = ({
  redirectTo = '/dashboard',
  renderForm,
   loadingComponent,
}) => {
  const { isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();
  const formProps = useRegisterForm();

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      navigate(redirectTo);
    }
  }, [isAuthenticated, isLoading, navigate, redirectTo]);

  if (isLoading) {
    return loadingComponent ? <>{loadingComponent}</> : <LoadingIndicator />;
  }
  
  if (isAuthenticated) return null;

  return <>{renderForm(formProps)}</>;
};