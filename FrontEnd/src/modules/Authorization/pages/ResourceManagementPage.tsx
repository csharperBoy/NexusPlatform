import React from 'react';
import { ResourceManagementWithCustomForm, type RenderFormProps } from '../components/CustomPage/ResourceManagementPage';
import { Tree } from '@/core/components/Tree'; // ایمپورت کامپوننت Tree
import Card from '@/core/components/Card';
import Button from '@/core/components/Button';

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
         <Tree
          nodes={treeData}
          getNodeId={(node) => node.id}
          getNodeLabel={(node) => node.name}
          getNodeChildren={(node) => node.children}
          onNodeClick={(node) => console.log('Selected:', node)}
          selectable={true}
          cascadeSelection={true}  // آبشاری
          onSelectionChange={(selectedIds, selectedNodes) => {
            console.log('Selected IDs:', selectedIds);
          }}
          expandAll
        />
        </div>
      )}
    />
  );
};

export default ResourceManagementPage;