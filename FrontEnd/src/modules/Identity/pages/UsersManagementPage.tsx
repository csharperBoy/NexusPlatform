// modules/identity/pages/UsersManagementPage.tsx
import React from 'react';
import { UserManagementForm, RenderFormProps } from '../Interface/IUserManagementPage';
import { Table, ColumnDef } from '@/core/components/Table';
import { UserDto } from '../models/UserDto'; 
const UsersManagementPage: React.FC = () => {

  // تعریف ستون‌های جدول برای نمایش اطلاعات کاربران
  const userColumns: ColumnDef<UserDto>[] = [
    {
      id: 'UserName',
      label: 'نام کاربری',
      accessor: (row) => row.userName,
    },
    {
      id: 'FullName',
      label: 'نام کامل',
      accessor: (row) => row.fullName,
    },
    {
      id: 'phoneNumber',
      label: 'شماره تلفن',
      accessor: (row) => row.phoneNumber,
    },
    {
      id: 'actions',
      
      label: 'عملیات',
      // این ستون برای نمایش دکمه‌های ویرایش و حذف است
      // تابع render به هر سطر (row) دسترسی دارد
      render: (row) => (
        <div className="flex space-x-2">
          {/* <button onClick={() => editAction(row.Id)} className="text-blue-500 hover:text-blue-700">ویرایش</button> خطا داره چون editAction رو اینجای کد شناسایی نمیکنه */}
          {/* <button onClick={() => deleteAction(row.Id)} className="text-red-500 hover:text-red-700">حذف</button> */}
        </div>
      ),
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
            
          />
        </div>
      )}
    />
  );
};

export default UsersManagementPage;
