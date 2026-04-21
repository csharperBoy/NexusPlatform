// modules/identity/pages/PermissionsManagementPage.tsx
import React from 'react';
import { PermissionManagementForm, RenderFormProps } from '../../Interface/Permission/IPermissionManagementPage';
import { Table, ColumnDef } from '@/core/components/Table';
import { PermissionDto } from '../../models/PermissionDto'; 
import { actionMap, assignTypeMap, effectMap } from '../../models/PermissionEnum';
const PermissionsManagementPage: React.FC = () => {

  // تعریف ستون‌های جدول برای نمایش اطلاعات کاربران
  const permissionColumns: ColumnDef<PermissionDto>[] = [
    {
      id: 'action',
      label: 'عملیات',
      accessor: (row) =>actionMap[ row.action],
    },
     {
      id: 'resourceKey',
      label: 'منبع',
      accessor: (row) => row.resourceKey,
    },
    {
      id: 'description',
      label: 'توضیحات ',
      accessor: (row) => row.description,
    },
    {
      id: 'assigneeType',
      label: 'نوع گیرنده مجوز',
      accessor: (row) =>assignTypeMap[ row.assigneeType],
    },
    {
      id: 'effect',
      label: 'مجاز یا غیرمجاز',
      accessor: (row) => effectMap[ row.effect],
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
