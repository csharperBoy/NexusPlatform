// src/modules/Authorization/components/CustomPage/ResourceCreatePage.tsx
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/modules/Identity';
import { useResourceCreateForm } from '../../hooks/Forms/useResourceCreateForm';
import LoadingIndicator from '@/core/components/LoadingIndicator';
import type { CreateResourceRequest } from '../../models/CreateResourceRequest';

export interface RenderFormProps {
  formData: CreateResourceRequest;
  loading: boolean;
  error: string | null;
  handleChange: (field: keyof CreateResourceRequest, value: any) => void;
  handleSubmit: (e: React.FormEvent) => Promise<void>;
}

export interface ResourceCreatePageWithCustomFormProps {
  redirectTo?: string;
  renderForm: (props: RenderFormProps) => React.ReactNode;
  loadingComponent?: React.ReactNode;
}

export const ResourceCreateWithCustomForm: React.FC<ResourceCreatePageWithCustomFormProps> = ({
  redirectTo = '/resources',
  renderForm,
  loadingComponent,
}) => {
  const { isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();
  const formProps = useResourceCreateForm(() => navigate(redirectTo));

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