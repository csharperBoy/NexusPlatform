// src/modules/Authorization/pages/ResourceManagementPage.tsx
import React, { useMemo } from 'react';
import { ResourceManagementForm, RenderFormProps } from '../../Interface/Resource/IResourceManagementPage';
import { SmartDataGrid } from '@/core/components/SmartDataGrid/SmartDataGrid';
import { ColumnDef } from '@/core/components/SmartDataGrid/SmartDataGrid.types';
import type { ResourceDto } from '../../models/ResourceDto';

const flattenResourceTree = (nodes: ResourceDto[]): ResourceDto[] => {
  let result: ResourceDto[] = [];
  nodes.forEach(node => {
    const { children, ...rest } = node;
    result.push(rest as ResourceDto);
    if (children && children.length > 0) {
      result = result.concat(flattenResourceTree(children));
    }
  });
  return result;
};

const ResourceManagementPage: React.FC = () => {
  return (
    <ResourceManagementForm
      redirectTo="/dashboard"
      renderForm={({ treeData, error, refresh, deleteNode, editNode, addNode }: RenderFormProps) => {
        
        const flatData = useMemo(() => flattenResourceTree(treeData), [treeData]);

        // 🟢 فقط ستون‌های دیتا؛ بدون هیچ دکمه اضافه و کثیف‌کاری!
        const columns = useMemo<ColumnDef<ResourceDto>[]>(() => [
          { id: 'name', header: 'نام منبع', type: 'text', width: '35%' },
          { id: 'key', header: 'کلید منبع', type: 'text', width: '25%' },          
          { id: 'id', header: 'id', type: 'text', width: '25%' },
          { id: 'parentId', header: 'parentId', type: 'text', width: '25%' },
          { 
            id: 'path', 
            header: 'مسیر (URL)', 
            type: 'text', 
            width: '30%',
            render: (row: ResourceDto) => <span className="font-mono text-gray-500">{row.path || '-'}</span>
          },
          { id: 'isActive', header: 'وضعیت', type: 'checkbox', width: '10%' }
        ], []);

        return (
          <div className="p-6 bg-gray-50 min-h-screen">
            <div className="mb-4 flex justify-between items-center">
              <h1 className="text-xl font-bold text-gray-800">مدیریت ساختار منابع و دسترسی‌ها</h1>
              <button onClick={() => refresh()} className="text-sm bg-white border border-gray-300 px-3 py-1.5 rounded shadow-sm">
                🔄 به‌روزرسانی لیست
              </button>
            </div>

            {error && <div className="bg-red-50 text-red-600 p-3 rounded-lg mb-4 text-sm">{error}</div>}

            {/* 🚀 حالا شد! همه‌چیز رفت پشت مانیفستِ خود کامپوننت */}
            <SmartDataGrid<ResourceDto>
              data={flatData}
              columns={columns}
              keyExtractor={(row) => row.id}
              pageSize={10}
              emptyMessage="هیچ منبعی در سیستم تعریف نشده است."
              
              // 🌲 تنظیمات درخت
              treeConfig={{
                enabled: true,
                parentKey: 'parentId'
              }}

              // 🛠️ سپردن اکشن‌ها به خود پلاگین گرید بدون دستکاری کالم‌ها
              actionConfig={{
                allowEdit: true,
                allowDelete: true,
                allowAdd: true, // این دکمه افزودن رکورد اصلی ریشه است
                
                // مدیریت متمرکز تمام اکشن‌های ردیف‌ها (حذف و ویرایش و...)
                onSaveRow: async (row, actionType) => {
                  if (actionType === 'delete') {
                    await deleteNode(row.id);
                  } else if (actionType === 'edit') {
                    await editNode(row.id);
                  }
                },
                emptyRowFactory: () => ({ id: '', key: '', name: '', description: '', type: 1, category: 1, displayOrder: 0, isActive: true })
              }}
            />
          </div>
        );
      }}
    />
  );
};

export default ResourceManagementPage;