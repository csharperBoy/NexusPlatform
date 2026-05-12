// src/pages/PermissionRuleCreatePage.tsx
import { useNavigate } from 'react-router-dom';
import { usePermissionRuleCreateUpdateForm } from '../../hooks/Forms/PermissionRule/usePermissionRuleCreateUpdateForm';
import { PermissionRuleCreateUpdateForm } from './PermissionRuleCreateUpdateForm';

export default function PermissionRuleCreatePage() {
  const navigate = useNavigate();
  const formProps = usePermissionRuleCreateUpdateForm(undefined, () => navigate('/permissionRules')); // بازگشت به لیست کاربران پس از موفقیت

  return <PermissionRuleCreateUpdateForm {...formProps} />;
}