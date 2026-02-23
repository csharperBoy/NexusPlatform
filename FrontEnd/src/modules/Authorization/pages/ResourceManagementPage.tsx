// src/modules/Authorization/pages/ResourceManagementPage.tsx
import React from 'react';
import { ResourceManagementWithCustomForm, type RenderFormProps } from '../components/CustomPage/ResourceManagementPage';
import Card from '@/core/components/Card';
import Button from '@/core/components/Button';

// یک کامپوننت ساده برای نمایش درخت
const TreeView: React.FC<{ data: any[] }> = ({ data }) => {
  return (
    <ul className="list-disc list-inside">
      {data.map(item => (
        <li key={item.id} className="mb-2">
          <span className="font-medium">{item.name}</span> ({item.key})
          {item.children && item.children.length > 0 && (
            <div className="mr-4 mt-1">
              <TreeView data={item.children} />
            </div>
          )}
        </li>
      ))}
    </ul>
  );
};

const ResourceManagementPage: React.FC = () => {
  return (
    <ResourceManagementWithCustomForm
      redirectTo="/dashboard"
      renderForm={({
        treeData,
        loading,
        error,
        refresh,
      }: RenderFormProps) => (
        <div className="p-4">
          <div className="flex justify-between items-center mb-4">
            <h1 className="text-2xl font-bold">مدیریت منابع</h1>
            <Button onClick={() => refresh()}>بروزرسانی</Button>
          </div>
          <Card className="p-4">
            {loading && <p>در حال بارگذاری...</p>}
            {error && <p className="text-red-600">{error}</p>}
            {!loading && !error && <TreeView data={treeData} />}
          </Card>
          
        </div>

        
      )}
    />
  );
};

export default ResourceManagementPage;