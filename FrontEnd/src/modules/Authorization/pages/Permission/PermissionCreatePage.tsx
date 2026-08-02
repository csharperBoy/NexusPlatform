import React from 'react';
import { useNavigate } from 'react-router-dom';
import { PermissionCreateUpdate } from './PermissionCreateUpdate';

export default function PermissionCreatePage() {
  const navigate = useNavigate();

  return (
    <PermissionCreateUpdate
      onSuccess={() => navigate('/authorization/permissions')}
    />
  );
}