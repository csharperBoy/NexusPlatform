// modules/identity/pages/UsersManagementPage.tsx
import React from 'react';
import { UserManagementForm, RenderFormProps } from '../Interface/IUserManagementPage';
import { Table } from '@/core/components/Table';
const UsersManagementPage: React.FC = () => {

  return (
    <UserManagementForm
      redirectTo="/dashboard"
      renderForm={({ Data,filters, loading, error, refresh, deleteAction , editAction,addAction }: RenderFormProps) => (
        <div className="p-4"> 
          <Table
            rows={Data}
            getRowId={(row) => row.Id}
            getRowLabel={(row) => row.FullName || ''}
            selectable
            renderRow={(row) => (
              <div 
              // className="flex items-center gap-3"
              >
                <span>{row.FullName}</span>

                
                <button
                  // className="px-2 py-1 text-xs bg-blue-500 text-blue rounded"
                   onClick={async () => {
                    
                    try {
                      await editAction(row.Id);
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
                    if (!confirm(`حذف "${row.FullName}"?`)) return;

                    try {
                      await deleteAction(row.Id);
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

export default UsersManagementPage;
