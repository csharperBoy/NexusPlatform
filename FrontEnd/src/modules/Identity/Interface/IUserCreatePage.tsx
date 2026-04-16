// src/modules/Authorization/components/Interface/IUserCreatePage.tsx
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/modules/Identity';
import { useUserCreateForm } from '../hooks/Forms/useUserCreateForm';
import LoadingIndicator from '@/core/components/LoadingIndicator';
import type { CreateUserCommand } from '../models/CreateUserCommand';

export interface RenderFormProps {
  formData: CreateUserCommand;
  loading: boolean;
  error: string | null;
  handleChange: (field: keyof CreateUserCommand, value: any) => void;
  handleSubmit: (e: React.FormEvent) => Promise<void>;
  rolesList: { id: string; name: string }[];                 // ← اضافه شد
  handleRolesChange: (id: string, checked: boolean) => void;  // ← اضافه شد
}


export interface IUserCreatePageProps {
  redirectTo?: string;
  renderForm: (props: RenderFormProps) => React.ReactNode;
  loadingComponent?: React.ReactNode;
}

export const UserCreateForm: React.FC<IUserCreatePageProps> = ({
  redirectTo = '/users',
  renderForm,
  loadingComponent,
}) => {
  const { isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();
  const formProps = useUserCreateForm(() => navigate(redirectTo));

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      // اگر کاربر احراز هویت شده، نیازی به کار خاصی نیست
    }
  }, [isAuthenticated, isLoading, navigate]);

  if (isLoading) {
    return loadingComponent ? <>{loadingComponent}</> : <LoadingIndicator />;
  }

  return <>{renderForm(formProps)}</>;
};