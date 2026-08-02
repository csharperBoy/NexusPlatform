import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { PermissionCreateUpdate } from './PermissionCreateUpdate';

export default function PermissionUpdatePage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  return (
    <PermissionCreateUpdate
      permissionId={id}
      onSuccess={() => navigate('/authorization/permissions')}
    />
  );
}