// src/modules/Authorization/components/IPermissionRuleCreateUpdatePage.tsx
import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '@/modules/Identity';
import { usePermissionRuleCreateUpdateForm } from '../../hooks/Forms/PermissionRule/usePermissionRuleCreateUpdateForm';
import { PermissionRuleFormCommand } from '../../models/PermissionRuleCommands';
import { ComparisonOperator , LogicalOperator } from '@/modules/Authorization/models/PermissionRuleEnum';
import LoadingIndicator from '@/core/components/LoadingIndicator';

export interface RenderPermissionRuleFormProps {
  formData: PermissionRuleFormCommand;
  joinDetailList: { value: string; display: string }[];
  operatorOptions: { value: ComparisonOperator; display: string }[];
  logicalOperatorOptions: { value: LogicalOperator; display: string }[];
  loading: boolean;
  error: string | null;
  handleChange: <K extends keyof PermissionRuleFormCommand>(field: K, value: PermissionRuleFormCommand[K]) => void;
  handleSubmit: (e: React.FormEvent) => void;
  isEdit: boolean;
}

export interface IPermissionRuleCreateUpdatePageProps {
  formMode: 'create' | 'update';
  redirectTo?: string;
  renderForm: (props: RenderPermissionRuleFormProps) => React.ReactNode;
  loadingComponent?: React.ReactNode;
}

export const IPermissionRuleCreateUpdatePage: React.FC<IPermissionRuleCreateUpdatePageProps> = ({
  formMode,
  redirectTo = '/permissions',
  renderForm,
  loadingComponent,
}) => {
  const { isAuthenticated, isLoading: isAuthLoading } = useAuth();
  const navigate = useNavigate();
  const { permissionId, id: ruleId } = useParams<{ permissionId: string; id?: string }>();

  if (!permissionId) {
    return <div className="text-red-500 p-4">شناسه‌ی منبع نامعتبر است.</div>;
  }

  const formProps = usePermissionRuleCreateUpdateForm(
    formMode === 'update' ? ruleId : undefined,
    // permissionId,
    () => navigate(redirectTo)
  );

  const isLoadingPage = isAuthLoading || (formMode === 'update' && formProps.loading);

  useEffect(() => {
    if (!isAuthLoading && !isAuthenticated) {
      console.warn('User not authenticated – consider redirecting to /login');
    }
  }, [isAuthenticated, isAuthLoading]);

  if (isLoadingPage) {
    return loadingComponent ? <>{loadingComponent}</> : <LoadingIndicator />;
  }

  if (formMode === 'update' && formProps.error && !formProps.formData) {
    return (
      <div className="text-red-500 p-4">
        خطا در بارگذاری اطلاعات قانون مجوز: {formProps.error}
      </div>
    );
  }

  return <>{renderForm({ ...formProps, isEdit: formMode === 'update' })}</>;
};
