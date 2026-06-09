// modules/authorization/pages/PermissionRulesManagementPage.tsx
import React from 'react';
import { PermissionRuleManagementForm, RenderFormProps } from '../../Interface/PermissionRule/IPermissionRuleManagementPage';
import { Table, ColumnDef } from '@/core/components/Table';
import { PermissionRuleDto } from '../../models/PermissionRuleDto'; 
import { ComparisonOperator,ComparisonOperatorOptions,LogicalOperator , LogicalOperatorOptions, comparisonOperatorFromText,comparisonOperatorText,logicalOperatorFromText,logicalOperatorText } from '../../models/PermissionRuleEnum';
const PermissionRulesManagementPage: React.FC = () => {

  // تعریف ستون‌های جدول برای نمایش اطلاعات کاربران
  const PermissionRuleColumns: ColumnDef<PermissionRuleDto>[] = [
    {
      id: 'fieldName',
      header: 'fieldName',
      accessor: (row) =>row.fieldName,
    },
     {
      id: 'operator',
      header: 'operator',
      type: 'select',
      options: ComparisonOperatorOptions,
      // accessor: (row) =>comparisonOperatorText[ row.operator as ComparisonOperator],
      accessor: (row) => row.operator,
    },
    {
      id: 'value',
      header: 'value ',
      accessor: (row) => row.value,
    },
    {
      id: 'logicalOperator',
      header: 'logicalOperator',
      type: 'select',
      options: LogicalOperatorOptions,
      // accessor: (row) =>logicalOperatorText[ row.logicalOperator as LogicalOperator],
      accessor: (row) => row.logicalOperator,
    },
    {
      id: 'groupOrder',
      header: 'groupOrder',
      accessor: (row) =>  row.groupOrder,
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
          <Table<PermissionRuleDto>
            data={FormData}
            columns={PermissionRuleColumns}
            onEdit={(row) => editNode(row.id)}
            onDelete={(row) => deleteNode(row.id)}
            keyExtractor={(row) => row.id}
            pageSize={10} 
            emptyMessage="هیچ رکوردی یافت نشد"
          />;
        </div>
      )}
    />
  );
};

export default PermissionRulesManagementPage;
