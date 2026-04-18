// src/modules/Authorization/pages/Resource/ResourceUpdatePage.tsx
import { useParams, useNavigate } from 'react-router-dom';
import { useResourceCreateUpdateForm } from '../../hooks/Forms/Resource/useResourceCreateUpdateForm';
import { ResourceCreateUpdateForm } from './ResourceCreateUpdateForm';

export default function ResourceEditPage() {
  const { id } = useParams<{ id: string }>(); // دریافت ID از URL
  const navigate = useNavigate();
  const formProps = useResourceCreateUpdateForm(id, () => navigate('/resources')); // بازگشت به لیست کاربران پس از موفقیت

  return <ResourceCreateUpdateForm {...formProps} />;
}