// src/modules/Authorization/pages/Resource/ResourceCreatePage.tsx
import { useNavigate } from 'react-router-dom';
import { useResourceCreateUpdateForm } from '../../hooks/Forms/Resource/useResourceCreateUpdateForm';
import { ResourceCreateUpdateForm } from './ResourceCreateUpdateForm';

export default function ResourceCreatePage() {
  const navigate = useNavigate();
  const formProps = useResourceCreateUpdateForm(undefined, () => navigate('/resources')); // بازگشت به لیست کاربران پس از موفقیت

  return <ResourceCreateUpdateForm {...formProps} />;
}