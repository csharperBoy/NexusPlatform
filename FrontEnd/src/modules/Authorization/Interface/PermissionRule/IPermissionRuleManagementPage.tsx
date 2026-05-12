// src/modules/Authorization/components/Interface/IPermissionRuleManagementPage.tsx
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/modules/Identity';
import { usePermissionRuleManagement } from '../../hooks/Forms/PermissionRule/usePermissionRuleManagementForm';
import LoadingIndicator from '@/core/components/LoadingIndicator';
import type { PermissionRuleDto } from '../../models/PermissionRuleDto';
import { GetPermissionsRuleQuery } from '../../models/PermissionRuleQuery';

export interface RenderFormProps {
  FormData: PermissionRuleDto[];
  
   filters: GetPermissionsRuleQuery | null;
  loading: boolean;
  error: string | null;
  refresh: (req?: GetPermissionsRuleQuery) => Promise<void>;
  deleteNode: (id: string) => Promise<void>; 
  editNode: (id: string) => Promise<void>;   
  addNode: (id: string) => Promise<void>; 
}


export interface IPermissionRuleManagementPageProps {
  redirectTo?: string;
  renderForm: (props: RenderFormProps) => React.ReactNode;
  loadingComponent?: React.ReactNode;
}

export const PermissionRuleManagementForm: React.FC<IPermissionRuleManagementPageProps> = ({
  redirectTo = '/dashboard',
  renderForm,
  loadingComponent,
}) => {
  const { isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();
  const managementProps = usePermissionRuleManagement();

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