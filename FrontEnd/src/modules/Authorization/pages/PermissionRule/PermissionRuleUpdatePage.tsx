// src/pages/PermissionRuleUpdatePage.tsx
import { useParams, useNavigate } from 'react-router-dom';
import { usePermissionRuleCreateUpdateForm } from '../../hooks/Forms/PermissionRule/usePermissionRuleCreateUpdateForm';
import { PermissionRuleCreateUpdateForm } from './PermissionRuleCreateUpdateForm';

export default function PermissionRuleUpdatePage() {
  const { id } = useParams<{ id: string }>(); // دریافت ID از URL
  const navigate = useNavigate();
  const formProps = usePermissionRuleCreateUpdateForm(id, () => navigate('/PermissionRules')); // بازگشت به لیست کاربران پس از موفقیت

  return <PermissionRuleCreateUpdateForm {...formProps} />;
}