// modules/identity/pages/RolesManagementPage.tsx
import React from 'react';
import { RoleManagementForm, RenderFormProps } from '../../Interface/Role/IRoleManagementPage';
import { Table, ColumnDef } from '@/core/components/Table';
import { RoleDto } from '../../models/RoleDto'; 
const RolesManagementPage: React.FC = () => {

  // تعریف ستون‌های جدول برای نمایش اطلاعات کاربران
  const roleColumns: ColumnDef<RoleDto>[] = [
    {
      id: 'Name',
      label: 'نام ',
      accessor: (row) => row.name,
    },
    {
      id: 'Description',
      label: 'توضیح ',
      accessor: (row) => row.description,
    },
    {
      id: 'OrderNum',
      label: 'ترتیب',
      accessor: (row) => row.orderNum,
    },
    {
      id: 'actions',      
      label: 'عملیات',
      
    },
  ];

  return (
    <RoleManagementForm
      redirectTo="/dashboard"
      renderForm={({ Data, filters, loading, error, refresh, deleteAction, editAction, addAction }: RenderFormProps) => (
        <div className="p-4">
          {/* دکمه اضافه کردن کاربر جدید */}
          <button onClick={() => addAction('')} className="mb-4 px-4 py-2 bg-green-500 text-black rounded hover:bg-green-600">
            افزودن کاربر جدید
          </button>
          
          {/* کامپوننت جدول */}
          <Table
            data={Data}
            columns={roleColumns}
            onEdit={editAction}
            onDelete={deleteAction}
          />
        </div>
      )}
    />
  );
};

export default RolesManagementPage;
