// modules/identity/pages/PermissionsManagementPage.tsx
import React, { useCallback } from 'react';
import { PermissionManagementForm } from '../../Interface/Permission/IPermissionManagementPage';
import { BatchChanges, ColumnDef } from '@/core/components/SmartDataGrid1/SmartDataGrid.types';
import { PermissionDto } from '../../models/PermissionDto';
import { ActionOptions, AssignTypeOptions, EffectOptions } from '../../models/PermissionEnum';
import SmartDataGrid from '@/core/components/SmartDataGrid/SmartDataGrid';

const PermissionsManagementPage: React.FC = () => {
  // تعریف ستون‌های جدول با تایپ اصلاح‌شده و استاندارد آرایه‌ای
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

  // متد ذخیره دسته‌ای برای جلوگیری از رندر مجدد با useCallback بهینه شد
  const handleSaveBatch = useCallback((changes: BatchChanges<PermissionDto>) => {
  console.log("تمام تغییرات اعمال شده جهت ارسال به API:", changes);
  
  // بسته به طراحی گریدتان، احتمالاً داخل changes به این صورت به داده‌ها دسترسی دارید:
  // const { added, updated, deleted } = changes;
}, []);

  // متد اعتبارسنجی سطرها
  const handleValidateRow = useCallback((row: PermissionDto) => {
    const errors: string[] = [];
    if (!row.resourceKey) errors.push("وارد کردن کلید منبع اجباری است.");
    if (!row.action) errors.push("انتخاب نوع عملیات اجباری است.");
    if (!row.assigneeType) errors.push("انتخاب نوع گیرنده مجوز اجباری است.");
    if (!row.effect) errors.push("تعیین وضعیت مجاز/غیرمجاز اجباری است.");
    return errors.length > 0 ? errors : null;
  }, []);

  // فکتوری ساخت سطر خالی جدید
  const createEmptyRow = useCallback((): PermissionDto => ({
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
  }), []);

  return (
    <PermissionManagementForm
      redirectTo="/dashboard"
      renderForm={({ FormData }) => (
        <div className="p-4">
          

          <SmartDataGrid
           data={FormData}
            columns={permissionColumns}
            keyExtractor={(row) => row.id || `${row.resourceKey}_${row.assigneeType}_${row.action}`}
            pageSize={20}
            
            // تنظیمات درختی شیک و مجتمع
            // treeConfig={{
            //   enabled: true,
            //   parentKey: 'parentId'
            // }}

            // تنظیمات ادیت و اکشن‌ها یکجا
            actionConfig={{
              allowAdd: true,
              allowEdit: true,
              allowDelete: true,
              onSaveBatch: handleSaveBatch,
              onSaveRow: () => {},
              emptyRowFactory: createEmptyRow,
              validateRow: handleValidateRow
            }}

            // تنظیمات اکسل یکجا
            excelConfig={{
              allowExport: true,
              allowImport: true
            }}
          />
        </div>
      )}
    />
  );
};

export default PermissionsManagementPage;