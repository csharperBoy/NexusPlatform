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
      header: 'نام ',
      accessor: (row) => row.name,
    },
    {
      id: 'Description',
      header: 'توضیح ',
      accessor: (row) => row.description,
    },
    {
      id: 'OrderNum',
      header: 'ترتیب',
      accessor: (row) => row.orderNum,
    },
    
  ];

  return (
    <RoleManagementForm
      redirectTo="/dashboard"
      renderForm={({ Data, filters, loading, error, refresh, deleteAction, editAction, addAction }: RenderFormProps) => (
        <div className="p-4">
          {/* دکمه اضافه کردن کاربر جدید */}
          <button onClick={() => addAction('')} className="mb-4 px-4 py-2 bg-green-500 text-black rounded hover:bg-green-600">
            افزودن نقش جدید
          </button>
          
          {/* کامپوننت جدول */}
          <Table
            data={Data}
            columns={roleColumns}
            onEdit={(row) => editAction(row.id)}
            onDelete={(row) => deleteAction(row.id)}
            keyExtractor={(row) => row.id}
            emptyMessage='هیچ رکوردی یافت نشد'
          />
        </div>
      )}
    />
  );
};

export default RolesManagementPage;
