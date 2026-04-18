// src/modules/Authorization/components/Interface/IRoleCreatePage.tsx
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/modules/Identity';
import { useRoleCreateForm} from '../../hooks/Forms/Role/useRoleCreateForm';
import LoadingIndicator from '@/core/components/LoadingIndicator';
import type { CreateRoleCommand } from '../../models/RoleCommands';

export interface RenderFormProps {
  formData: CreateRoleCommand;
  loading: boolean;
  error: string | null;
  handleChange: (field: keyof CreateRoleCommand, value: any) => void;
  handleSubmit: (e: React.FormEvent) => Promise<void>;
}

export interface IRoleCreatePageProps {
  redirectTo?: string;
  renderForm: (props: RenderFormProps) => React.ReactNode;
  loadingComponent?: React.ReactNode;
}

export const RoleCreateForm: React.FC<IRoleCreatePageProps> = ({
  redirectTo = '/Roles',
  renderForm,
  loadingComponent,
}) => {
  const { isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();
  const formProps = useRoleCreateForm(() => navigate(redirectTo));

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