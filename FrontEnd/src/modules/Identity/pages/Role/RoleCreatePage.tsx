// src/modules/Identity/pages/RoleCreatePage.tsx
import { useNavigate } from 'react-router-dom';
import { useRoleCreateUpdateForm } from '../../hooks/Forms/Role/useRoleCreateUpdateForm';
import { RoleCreateUpdateForm } from './RoleCreateUpdateForm';

export default function RoleCreatePage() {
  const navigate = useNavigate();
  const formProps = useRoleCreateUpdateForm(undefined, () => navigate('/roles')); // بازگشت به لیست کاربران پس از موفقیت

  return <RoleCreateUpdateForm {...formProps} />;
}