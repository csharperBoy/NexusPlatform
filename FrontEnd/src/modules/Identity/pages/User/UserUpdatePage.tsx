// src/pages/UserEditPage.tsx
import { useParams, useNavigate } from 'react-router-dom';
import { useUserCreateUpdateForm } from '../../hooks/Forms/User/useUserCreateUpdateForm';
import { UserCreateUpdateForm } from './UserCreateUpdateForm';

export default function UserEditPage() {
  const { id } = useParams<{ id: string }>(); // دریافت ID از URL
  const navigate = useNavigate();
  const formProps = useUserCreateUpdateForm(id, () => navigate('/users')); // بازگشت به لیست کاربران پس از موفقیت

  return <UserCreateUpdateForm {...formProps} />;
}