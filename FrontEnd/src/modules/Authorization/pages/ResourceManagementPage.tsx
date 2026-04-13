import React from 'react';
import { ResourceManagementForm, RenderFormProps } from '../Interface/IResourceManagementPage';
import { Tree } from '@/core/components/Tree';
const ResourceManagementPage: React.FC = () => {

  return (
    <ResourceManagementForm
      redirectTo="/dashboard"
      renderForm={({ treeData, loading, error, refresh, deleteNode , editNode,addNode }: RenderFormProps) => (
        <div className="p-4"> 
          <Tree
            nodes={treeData}
            getNodeId={(node) => node.id}
            getNodeLabel={(node) => node.name}
            getNodeChildren={(node) => node.children}
            selectable
            cascadeSelection
            expandAll
            renderNode={(node) => (
              <div 
              // className="flex items-center gap-3"
              >
                <span>{node.name}</span>

                <button
                  // className="px-2 py-1 text-xs bg-blue-500 text-blue rounded"
                   onClick={async () => {
                    
                    try {
                      await addNode(node.id);
                    } catch (e) {
                      alert(e);
                    }
                  }}
                >
                  افزودن
                </button>
                <button
                  // className="px-2 py-1 text-xs bg-blue-500 text-blue rounded"
                   onClick={async () => {
                    
                    try {
                      await editNode(node.id);
                    } catch (e) {
                      alert(e);
                    }
                  }}
                >
                  ویرایش
                </button>
                <button
                  // className="text-red-600 hover:underline"
                  onClick={async () => {
                    if (!confirm(`حذف "${node.name}"?`)) return;

                    try {
                      await deleteNode(node.id);
                    } catch (e) {
                      alert(e);
                    }
                  }}
                >
                  حذف
                </button>
              </div>
            )}
          />
        </div>
      )}
    />
  );
};

export default ResourceManagementPage;
