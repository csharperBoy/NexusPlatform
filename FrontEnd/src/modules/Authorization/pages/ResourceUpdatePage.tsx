// src/modules/Authorization/pages/ResourceUpdatePage.tsx
import React from 'react';
import { ResourceUpdateForm, type RenderFormProps } from '../Interface/IResourceUpdatePage';
import Button from '@/core/components/Button';
import Input from '@/core/components/Input';
import Card from '@/core/components/Card';

const ResourceUpdatePage: React.FC = () => {
  return (
    <ResourceUpdateForm
      redirectTo="/resources"
      renderForm={({
        formData,
        loading,
        error,
        handleChange,
        handleSubmit,
      }: RenderFormProps) => (
        <Card className="max-w-2xl mx-auto p-6">
          <h2 className="text-xl font-bold mb-4">ویرایش منبع </h2>
          <form onSubmit={handleSubmit}>
            <div className="mb-4">
              <label className="block mb-1">کلید (Key)</label>
              <Input
                value={formData?.key}
                onChange={(e) => handleChange('key', e.target.value)}
                required
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">نام</label>
              <Input
                value={formData?.name}
                onChange={(e) => handleChange('name', e.target.value)}
                required
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">توضیحات</label>
              <Input
                value={formData?.description}
                onChange={(e) => handleChange('description', e.target.value)}
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">نوع</label>
              <select
                value={formData?.type}
                onChange={(e) => handleChange('type', e.target.value)}
                className="w-full p-2 border rounded"
                disabled={loading}
              >
                <option value="Module">ماژول (Module)</option>
                <option value="Ui">رابط کاربری (Ui)</option>
                <option value="Data">داده (Data)</option>
              </select>
            </div>
            <div className="mb-4">
              <label className="block mb-1">دسته‌بندی</label>
              <select
                value={formData?.category}
                onChange={(e) => handleChange('category', e.target.value)}
                className="w-full p-2 border rounded"
                disabled={loading}
              >
                <option value="General">عمومی (General)</option>
                <option value="System">سیستمی (System)</option>
                <option value="Module">ماژول (Module)</option>
                <option value="Menu">منو (Menu)</option>
                <option value="Page">صفحه (Page)</option>
                <option value="Component">کامپوننت (Component)</option>
                <option value="DatabaseTable">جدول دیتابیس (DatabaseTable)</option>
                <option value="RowInTable">سطر جدول (RowInTable)</option>
              </select>
            </div>
            <div className="mb-4">
              <label className="block mb-1">ترتیب نمایش</label>
              <Input
                type="number"
                value={formData?.displayOrder}
                onChange={(e) => handleChange('displayOrder', parseInt(e.target.value) || 0)}
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">آیکون (اختیاری)</label>
              <Input
                value={formData?.icon}
                onChange={(e) => handleChange('icon', e.target.value)}
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">مسیر (Route - اختیاری)</label>
              <Input
                value={formData?.route}
                onChange={(e) => handleChange('route', e.target.value)}
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">والد (ParentId - اختیاری)</label>
              <Input
                value={formData?.parentId || ''}
                onChange={(e) => handleChange('parentId', e.target.value || null)}
                disabled={loading}
                placeholder="GUID والد"
              />
            </div>
            {error && (
              <div className="text-red-600 mb-4 p-3 bg-red-50 rounded">
                {error}
              </div>
            )}
            <Button type="submit" disabled={loading}>
              {loading ? "در حال ویرایش..." : "ویرایش منبع"}
            </Button>
          </form>
        </Card>
      )}
    />
  );
};

export default ResourceUpdatePage;