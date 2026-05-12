// modules/authorization/pages/PermissionRulesManagementPage.tsx
import React from 'react';
import { PermissionRuleManagementForm, RenderFormProps } from '../../Interface/PermissionRule/IPermissionRuleManagementPage';
import { Table, ColumnDef } from '@/core/components/Table';
import { PermissionRuleDto } from '../../models/PermissionRuleDto'; 
import { ComparisonOperator,LogicalOperator , comparisonOperatorFromText,comparisonOperatorText,logicalOperatorFromText,logicalOperatorText } from '../../models/PermissionRuleEnum';
const PermissionRulesManagementPage: React.FC = () => {

  // تعریف ستون‌های جدول برای نمایش اطلاعات کاربران
  const PermissionRuleColumns: ColumnDef<PermissionRuleDto>[] = [
    {
      id: 'fieldName',
      label: 'fieldName',
      accessor: (row) =>row.fieldName,
    },
     {
      id: 'operator',
      label: 'operator',
      accessor: (row) =>comparisonOperatorText[ row.operator as ComparisonOperator],
    },
    {
      id: 'value',
      label: 'value ',
      accessor: (row) => row.value,
    },
    {
      id: 'logicalOperator',
      label: 'logicalOperator',
      accessor: (row) =>logicalOperatorText[ row.logicalOperator as LogicalOperator],
    },
    {
      id: 'groupOrder',
      label: 'groupOrder',
      accessor: (row) =>  row.groupOrder,
    },
    {
      id: 'actions',      
      label: 'عملیات',
      
    },
  ];

  return (
    <PermissionRuleManagementForm
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
            columns={PermissionRuleColumns}
            onEdit={editNode}
            onDelete={deleteNode}
          />
        </div>
      )}
    />
  );
};

export default PermissionRulesManagementPage;
