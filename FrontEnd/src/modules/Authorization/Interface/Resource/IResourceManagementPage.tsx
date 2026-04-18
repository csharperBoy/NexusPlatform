// src/modules/Authorization/components/Interface/IResourceManagementPage.tsx
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/modules/Identity';
import { useResourceManagement } from '../../hooks/Forms/Resource/useResourceManagementForm';
import LoadingIndicator from '@/core/components/LoadingIndicator';
import type { ResourceDto } from '../../models/ResourceDto';

export interface RenderFormProps {
  treeData: ResourceDto[];
  loading: boolean;
  error: string | null;
  refresh: (rootId?: string) => Promise<void>;
  deleteNode: (id: string) => Promise<void>; 
  editNode: (id: string) => Promise<void>;   
  addNode: (id: string) => Promise<void>; 
}


export interface IResourceManagementPageProps {
  redirectTo?: string;
  renderForm: (props: RenderFormProps) => React.ReactNode;
  loadingComponent?: React.ReactNode;
}

export const ResourceManagementForm: React.FC<IResourceManagementPageProps> = ({
  redirectTo = '/dashboard',
  renderForm,
  loadingComponent,
}) => {
  const { isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();
  const managementProps = useResourceManagement();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      navigate('/login');
    }
  }, [isAuthenticated, isLoading, navigate]);

  if (isLoading) {
    return loadingComponent ? <>{loadingComponent}</> : <LoadingIndicator />;
  }

  return <>{renderForm(managementProps)}</>;
};