// src/pages/PermissionCreatePage.tsx
import { useNavigate } from 'react-router-dom';
import { usePermissionCreateUpdateForm } from '../../hooks/Forms/Permission/usePermissionCreateUpdateForm';
import { PermissionCreateUpdateForm } from './PermissionCreateUpdateForm';

export default function PermissionCreatePage() {
  const navigate = useNavigate();
  const formProps = usePermissionCreateUpdateForm(undefined, () => navigate('/permissions')); // بازگشت به لیست کاربران پس از موفقیت

  return <PermissionCreateUpdateForm {...formProps} />;
}