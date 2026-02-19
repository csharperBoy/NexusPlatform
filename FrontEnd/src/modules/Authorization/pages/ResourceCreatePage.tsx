// src/modules/Authorization/pages/ResourceCreatePage.tsx
import React from 'react';
import { ResourceCreateWithCustomForm, type RenderFormProps } from '../components/CustomPage/ResourceCreatePage';
import Button from '@/core/components/Button';
import Input from '@/core/components/Input';
import Card from '@/core/components/Card';

const ResourceCreatePage: React.FC = () => {
  return (
    <ResourceCreateWithCustomForm
      redirectTo="/resources" // بعد از ایجاد به صفحه مدیریت برود
      renderForm={({
        formData,
        loading,
        error,
        handleChange,
        handleSubmit,
      }: RenderFormProps) => (
        <Card className="max-w-2xl mx-auto p-6">
          <h2 className="text-xl font-bold mb-4">ایجاد منبع جدید</h2>
          <form onSubmit={handleSubmit}>
            <div className="mb-4">
              <label className="block mb-1">کلید (Key)</label>
              <Input
                value={formData.key}
                onChange={(e) => handleChange('key', e.target.value)}
                required
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">نام</label>
              <Input
                value={formData.name}
                onChange={(e) => handleChange('name', e.target.value)}
                required
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">توضیحات</label>
              <Input
                value={formData.description}
                onChange={(e) => handleChange('description', e.target.value)}
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">نوع</label>
              <select
                value={formData.type}
                onChange={(e) => handleChange('type', e.target.value)}
                className="w-full p-2 border rounded"
                disabled={loading}
              >
                <option value="Module">ماژول</option>
                <option value="Page">صفحه</option>
                <option value="Action">عملیات</option>
              </select>
            </div>
            <div className="mb-4">
              <label className="block mb-1">دسته‌بندی</label>
              <select
                value={formData.category}
                onChange={(e) => handleChange('category', e.target.value)}
                className="w-full p-2 border rounded"
                disabled={loading}
              >
                <option value="Admin">مدیریتی</option>
                <option value="Public">عمومی</option>
              </select>
            </div>
            <div className="mb-4">
              <label className="block mb-1">ترتیب نمایش</label>
              <Input
                type="number"
                value={formData.displayOrder}
                onChange={(e) => handleChange('displayOrder', parseInt(e.target.value))}
                disabled={loading}
              />
            </div>
            {error && <div className="text-red-600 mb-4">{error}</div>}
            <Button type="submit" disabled={loading}>
              {loading ? "در حال ایجاد..." : "ایجاد منبع"}
            </Button>
          </form>
        </Card>
      )}
    />
  );
};

export default ResourceCreatePage;