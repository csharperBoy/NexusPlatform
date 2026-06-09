// modules/identity/pages/UsersManagementPage.tsx
import React from 'react';
import { UserManagementForm, RenderFormProps } from '../../Interface/User/IUserManagementPage';
import { Table, ColumnDef } from '@/core/components/Table';
import { UserDto } from '../../models/UserDto'; 
const UsersManagementPage: React.FC = () => {

  // تعریف ستون‌های جدول برای نمایش اطلاعات کاربران
  const userColumns: ColumnDef<UserDto>[] = [
    {
      id: 'UserName',
      header: 'نام کاربری',
      accessor: (row) => row.userName,
    },
    {
      id: 'NickName',
      header: 'نام ',
      accessor: (row) => row.nickName,
    },
    {
      id: 'phoneNumber',
      header: 'شماره تلفن',
      accessor: (row) => row.phoneNumber,
    },
    {
      id: 'email',
      header: 'ایمیل',
      accessor: (row) => row.email,
    },
    
  ];

  return (
    <UserManagementForm
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
            columns={userColumns}
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

export default UsersManagementPage;
