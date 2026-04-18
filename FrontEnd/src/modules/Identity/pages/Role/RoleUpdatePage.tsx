// src/modules/Authorization/pages/RoleUpdatePage.tsx
import { useParams, useNavigate } from 'react-router-dom';
import { useRoleCreateUpdateForm } from '../../hooks/Forms/Role/useRoleCreateUpdateForm';
import { RoleCreateUpdateForm } from './RoleCreateUpdateForm';

export default function RoleEditPage() {
  const { id } = useParams<{ id: string }>(); // دریافت ID از URL
  const navigate = useNavigate();
  const formProps = useRoleCreateUpdateForm(id, () => navigate('/roles')); // بازگشت به لیست کاربران پس از موفقیت

  return <RoleCreateUpdateForm {...formProps} />;
}