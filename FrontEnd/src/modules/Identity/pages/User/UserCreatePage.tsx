// src/pages/UserCreatePage.tsx
import { useNavigate } from 'react-router-dom';
import { useUserCreateUpdateForm } from '../../hooks/Forms/User/useUserCreateUpdateForm';
import { UserCreateUpdateForm } from './UserCreateUpdateForm';

export default function UserCreatePage() {
  const navigate = useNavigate();
  const formProps = useUserCreateUpdateForm(undefined, () => navigate('/users')); // بازگشت به لیست کاربران پس از موفقیت

  return <UserCreateUpdateForm {...formProps} />;
}