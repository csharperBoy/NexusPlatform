// src/modules/Authorization/pages/RoleUpdatePage.tsx
import React from 'react';
import { RoleUpdateForm, type RenderFormProps } from '../../Interface/Role/IRoleUpdatePage';
import Button from '@/core/components/Button';
import Input from '@/core/components/Input';
import Card from '@/core/components/Card';

const RoleUpdatePage: React.FC = () => {
  return (
    <RoleUpdateForm
      redirectTo="/roles"
      renderForm={({
        formData,
        loading,
        error,
        handleChange,
        handleSubmit,
      }: RenderFormProps) => (
        <Card className="max-w-2xl mx-auto p-6">
          <h2 className="text-xl font-bold mb-4">ویرایش نقش </h2>
          <form onSubmit={handleSubmit}>
            <div className="mb-4">
              <label className="block mb-1">نام  (Name)</label>
              <Input
                value={formData?.Name || ''}
                onChange={(e) => handleChange('Name', e.target.value)}
                disabled={loading}
              />
            </div>
            
            <div className="mb-4">
              <label className="block mb-1">توضیح </label>
              <Input
                value={formData?.Description || ''}
                onChange={(e) => handleChange('Description', e.target.value)}
                disabled={loading}
              />
            </div>
            
            <div className="mb-4">
              <label className="block mb-1">ترتیب</label>
              <Input
                type="number"
                value={formData?.OrderNum || ''}
                onChange={(e) => handleChange('OrderNum', parseInt(e.target.value) || 0)}
                disabled={loading}
              />
            </div>
            
            
            {error && (
              <div className="text-red-600 mb-4 p-3 bg-red-50 rounded">
                {error}
              </div>
            )}
            <Button type="submit" disabled={loading}>
              {loading ? "در حال ویرایش..." : "ویرایش نقش"}
            </Button>
          </form>
        </Card>
      )}
    />
  );
};

export default RoleUpdatePage;