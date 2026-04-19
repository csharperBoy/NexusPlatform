// src/pages/PermissionUpdatePage.tsx
import { useParams, useNavigate } from 'react-router-dom';
import { usePermissionCreateUpdateForm } from '../../hooks/Forms/Permission/usePermissionCreateUpdateForm';
import { PermissionCreateUpdateForm } from './PermissionCreateUpdateForm';

export default function PermissionUpdatePage() {
  const { id } = useParams<{ id: string }>(); // دریافت ID از URL
  const navigate = useNavigate();
  const formProps = usePermissionCreateUpdateForm(id, () => navigate('/permissions')); // بازگشت به لیست کاربران پس از موفقیت

  return <PermissionCreateUpdateForm {...formProps} />;
}