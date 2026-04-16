// src/modules/Identity/pages/RoleCreatePage.tsx
import React from 'react';
import { RoleCreateForm, type RenderFormProps } from '../../Interface/Role/IRoleCreatePage';
import Button from '@/core/components/Button';
import Input from '@/core/components/Input';
import Card from '@/core/components/Card';


const RoleCreatePage: React.FC = () => {
  return (
    <RoleCreateForm
      redirectTo="/roles"
      renderForm={({
        formData,
        loading,
        error,
        handleChange,
        handleSubmit,
      }: RenderFormProps) => (
        <Card className="max-w-2xl mx-auto p-6">
          <h2 className="text-xl font-bold mb-4">ایجاد نقش جدید</h2>
          <form onSubmit={handleSubmit}>
            <div className="mb-4">
              <label className="block mb-1">نام  (Name)</label>
              <Input
                value={formData.Name || ''}
                onChange={(e) => handleChange('Name', e.target.value)}
                required
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">توضیحات</label>
              <Input
                value={formData.Description || ''}
                onChange={(e) => handleChange('Description', e.target.value)}
                
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">ترتیب</label>
              <Input
                value={formData.OrderNum || 0}
                onChange={(e) => handleChange('OrderNum', e.target.value)}
                disabled={loading}
                
                type='number'
              />
            </div>
           
           
            {error && (
              <div className="text-red-600 mb-4 p-3 bg-red-50 rounded">
                {error}
              </div>
            )}
            <Button type="submit" disabled={loading}>
              {loading ? "در حال ایجاد..." : "ایجاد نقش"}
            </Button>
          </form>
        </Card>
      )}
    />
  );
};

export default RoleCreatePage;