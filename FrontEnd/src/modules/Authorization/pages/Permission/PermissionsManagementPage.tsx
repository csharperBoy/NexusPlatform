// modules/identity/pages/PermissionsManagementPage.tsx
import React from 'react';
import { PermissionManagementForm } from '../../Interface/Permission/IPermissionManagementPage';
import { ColumnDef } from '@/core/components/SmartDataGrid/SmartDataGrid.types';
import { PermissionDto } from '../../models/PermissionDto';
import { ActionOptions, AssignTypeOptions, EffectOptions } from '../../models/PermissionEnum';
import SmartDataGrid from '@/core/components/SmartDataGrid/SmartDataGrid';

const PermissionsManagementPage: React.FC = () => {
  // تعریف ستون‌های جدول
  const permissionColumns: ColumnDef<PermissionDto>[] = [
    {
      id: 'action',
      header: 'عملیات',
      type: 'select',
      options: ActionOptions,      
      accessor: (row) => row.action,
      editable: true,
    },
    {
      id: 'resourceKey',
      header: 'منبع',
      type: 'text',
      accessor: (row) => row.resourceKey,
      editable: true,
    },
    {
      id: 'description',
      header: 'توضیحات',
      type: 'text',
      accessor: (row) => row.description,
      editable: true,
    },
    {
      id: 'assigneeType',
      header: 'نوع گیرنده مجوز',
      type: 'select',
      options: AssignTypeOptions,
      accessor: (row) => row.assigneeType,
      editable: true,
    },
    {
      id: 'effect',
      header: 'مجاز یا غیرمجاز',
      type: 'select',
      options: EffectOptions,
      accessor: (row) => row.effect,
      editable: true,
    },
  ];

  return (
    <PermissionManagementForm
      redirectTo="/dashboard"
      renderForm={({ FormData }) => (
        <div className="p-4">
          <SmartDataGrid<PermissionDto>
            data={FormData}
            columns={permissionColumns}
            keyExtractor={(row) => row.id || `${row.resourceKey}_${row.assigneeType}_${row.action}`}
            allowAdd
            allowEdit
            allowDelete
            allowExcelImport
            
            // فکتوری کاملاً بهینه و بدون خطای تایپ
            emptyRowFactory={() => ({
              id: '',
              resourceId: '',
              resourceKey: '',
              description: '',
              assigneeId: '',
              isActive: true,
              action: undefined as any,
              assigneeType: undefined as any,
              effect: undefined as any,
              effectiveFrom: null as any,
              expiresAt: null as any,
              scopes: [], 
              rules: []   
            } as PermissionDto)}

            // اعتبارسنجی فیلدهای اجباری بر اساس DTO واقعی شما
            validateRow={(row) => {
              const errors: string[] = [];
              if (!row.resourceKey) errors.push("وارد کردن کلید منبع اجباری است.");
              if (!row.action) errors.push("انتخاب نوع عملیات اجباری است.");
              if (!row.assigneeType) errors.push("انتخاب نوع گیرنده مجوز اجباری است.");
              if (!row.effect) errors.push("تعیین وضعیت مجاز/غیرمجاز اجباری است.");
              return errors.length > 0 ? errors : null;
            }}

            onSaveBatch={(changes) => {
              console.log("تمام تغییرات اعمال شده جهت ارسال به API:", changes);
            }}
            pageSize={10}
          />
        </div>
      )}
    />
  );
};

export default PermissionsManagementPage;