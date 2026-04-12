import React from 'react';
import { useNavigate } from 'react-router-dom';
import { ResourceManagementWithCustomForm, RenderFormProps } from '../components/CustomPage/ResourceManagementPage';
import { Tree } from '@/core/components/Tree';
import { resourceApi } from '../api/ResourcesApi';
import { ResourceDto } from '../models/ResourceDto';
const ResourceManagementPage: React.FC = () => {
  const navigate = useNavigate();


const handleDelete = async (
  node: ResourceDto,
  refresh: (rootId?: string) => Promise<void>
) => {
  if (!confirm(`آیا از حذف "${node.name}" مطمئن هستید؟`)) return;

  try {
    await resourceApi.deleteResource(node.id);
    await refresh();
  } catch {
    alert("حذف با مشکل مواجه شد");
  }
};


  return (
    <ResourceManagementWithCustomForm
      redirectTo="/dashboard"
      renderForm={({ treeData, loading, error, refresh }: RenderFormProps) => (
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
              <div className="flex items-center gap-3">
                <span>{node.name}</span>

                <button
                  className="px-2 py-1 text-xs bg-blue-500 text-white rounded"
                  onClick={() => navigate(`/resources/edit/${node.id}`)}
                >
                  ویرایش
                </button>

                <button
                  className="px-2 py-1 text-xs bg-red-500 text-white rounded"
                  onClick={() => handleDelete(node, refresh)}
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
