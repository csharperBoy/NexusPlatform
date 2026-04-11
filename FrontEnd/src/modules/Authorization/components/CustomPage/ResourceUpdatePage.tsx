// src/modules/Authorization/components/CustomPage/ResourceUpdatePage.tsx
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/modules/Identity';
import { useResourceUpdateForm } from '../../hooks/Forms/useResourceUpdateForm';
import LoadingIndicator from '@/core/components/LoadingIndicator';
import type { UpdateResourceRequest } from '../../models/UpdateResourceRequest';

export interface RenderFormProps {
  formData: UpdateResourceRequest;
  loading: boolean;
  error: string | null;
  handleChange: (field: keyof UpdateResourceRequest, value: any) => void;
  handleSubmit: (e: React.FormEvent) => Promise<void>;
}

export interface ResourceUpdatePageWithCustomFormProps {
  redirectTo?: string;
  renderForm: (props: RenderFormProps) => React.ReactNode;
  loadingComponent?: React.ReactNode;
}

export const ResourceUpdateWithCustomForm: React.FC<ResourceUpdatePageWithCustomFormProps> = ({
  redirectTo = '/resources',
  renderForm,
  loadingComponent,
}) => {
  const { isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();
  const formProps = useResourceUpdateForm(() => navigate(redirectTo));

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