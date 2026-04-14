// src/modules/Authorization/components/Interface/IUserManagementPage.tsx
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/modules/Identity';
import { useUserManagement } from '../hooks/Forms/useUserManagementForm';
import LoadingIndicator from '@/core/components/LoadingIndicator';
import type { UserDto } from '../models/UserDto';
import  { GetUsersQuery } from '../models/GetUsersQuery';

export interface RenderFormProps {
  Data: UserDto[];
 filters: GetUsersQuery | null;
  loading: boolean;
  error: string | null;
  refresh: (req?: GetUsersQuery) => Promise<void>;
  deleteAction: (id: string) => Promise<void>; 
  editAction: (id: string) => Promise<void>;   
  addAction: (id: string) => Promise<void>; 
}


export interface IUserManagementPageProps {
  redirectTo?: string;
  renderForm: (props: RenderFormProps) => React.ReactNode;
  loadingComponent?: React.ReactNode;
}

export const UserManagementForm: React.FC<IUserManagementPageProps> = ({
  redirectTo = '/dashboard',
  renderForm,
  loadingComponent,
}) => {
  const { isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();
  const managementProps = useUserManagement();

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