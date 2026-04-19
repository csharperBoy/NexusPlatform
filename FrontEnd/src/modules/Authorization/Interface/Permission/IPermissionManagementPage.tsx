// src/modules/Authorization/components/Interface/IPermissionManagementPage.tsx
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/modules/Identity';
import { usePermissionManagement } from '../../hooks/Forms/Permission/usePermissionManagementForm';
import LoadingIndicator from '@/core/components/LoadingIndicator';
import type { PermissionDto } from '../../models/PermissionDto';
import { GetPermissionsQuery } from '../../models/PermissionQuery';

export interface RenderFormProps {
  FormData: PermissionDto[];
  
   filters: GetPermissionsQuery | null;
  loading: boolean;
  error: string | null;
  refresh: (req?: GetPermissionsQuery) => Promise<void>;
  deleteNode: (id: string) => Promise<void>; 
  editNode: (id: string) => Promise<void>;   
  addNode: (id: string) => Promise<void>; 
}


export interface IPermissionManagementPageProps {
  redirectTo?: string;
  renderForm: (props: RenderFormProps) => React.ReactNode;
  loadingComponent?: React.ReactNode;
}

export const PermissionManagementForm: React.FC<IPermissionManagementPageProps> = ({
  redirectTo = '/dashboard',
  renderForm,
  loadingComponent,
}) => {
  const { isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();
  const managementProps = usePermissionManagement();

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