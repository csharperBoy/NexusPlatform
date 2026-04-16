// src/modules/Authorization/components/Interface/IRoleManagementPage.tsx
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/modules/Identity';
import { useRoleManagement } from '../../hooks/Forms/Role/useRoleManagementForm';
import LoadingIndicator from '@/core/components/LoadingIndicator';
import type { RoleDto } from '../../models/RoleDto';
import  { GetRolesQuery } from '../../models/GetRolesQuery';

export interface RenderFormProps {
  Data: RoleDto[];
 filters: GetRolesQuery | null;
  loading: boolean;
  error: string | null;
  refresh: (req?: GetRolesQuery) => Promise<void>;
  deleteAction: (id: string) => Promise<void>; 
  editAction: (id: string) => Promise<void>;   
  addAction: (id: string) => Promise<void>; 
}


export interface IRoleManagementPageProps {
  redirectTo?: string;
  renderForm: (props: RenderFormProps) => React.ReactNode;
  loadingComponent?: React.ReactNode;
}

export const RoleManagementForm: React.FC<IRoleManagementPageProps> = ({
  redirectTo = '/dashboard',
  renderForm,
  loadingComponent,
}) => {
  const { isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();
  const managementProps = useRoleManagement();

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