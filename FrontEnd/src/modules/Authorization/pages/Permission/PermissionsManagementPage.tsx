// modules/identity/pages/PermissionsManagementPage.tsx
import React from 'react';
import { PermissionManagementForm, RenderFormProps } from '../../Interface/Permission/IPermissionManagementPage';
import { Table, ColumnDef } from '@/core/components/Table';
import { PermissionDto } from '../../models/PermissionDto'; 
const PermissionsManagementPage: React.FC = () => {

  // تعریف ستون‌های جدول برای نمایش اطلاعات کاربران
  const permissionColumns: ColumnDef<PermissionDto>[] = [
    {
      id: 'Action',
      label: 'عملیات',
      accessor: (row) => row.Action,
    },
     {
      id: 'ResourceKey',
      label: 'منبع',
      accessor: (row) => row.ResourceKey,
    },
    {
      id: 'description',
      label: 'توضیحات ',
      accessor: (row) => row.description,
    },
    {
      id: 'AssigneeType',
      label: 'نوع گیرنده مجوز',
      accessor: (row) => row.AssigneeType,
    },
    {
      id: 'Effect',
      label: 'مجاز یا غیرمجاز',
      accessor: (row) => row.Effect,
    },
    {
      id: 'actions',      
      label: 'عملیات',
      
    },
  ];

  return (
    <PermissionManagementForm
      redirectTo="/dashboard"
      renderForm={({ FormData,filters, loading, error, refresh, deleteNode, editNode, addNode }: RenderFormProps) => (
        <div className="p-4">
          {/* دکمه اضافه کردن کاربر جدید */}
          <button onClick={() => addNode('')} className="mb-4 px-4 py-2 bg-green-500 text-black rounded hover:bg-green-600">
            افزودن مجوز جدید
          </button>
          
          {/* کامپوننت جدول */}
          <Table
            data={FormData}
            columns={permissionColumns}
            onEdit={editNode}
            onDelete={deleteNode}
          />
        </div>
      )}
    />
  );
};

export default PermissionsManagementPage;
