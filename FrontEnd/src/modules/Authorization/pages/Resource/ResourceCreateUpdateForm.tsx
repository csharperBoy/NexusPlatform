// src/modules/Authorization/pages/Resource/ResourceCreateUpdateForm.tsx
import React from 'react';
import { CreateResourceCommand, ResourceFormCommand } from '../../models/ResourceCommands'; 
import Card from '@/core/components/Card';
import Button from '@/core/components/Button';
import Input from '@/core/components/Input';

interface ResourceCreateUpdateFormProps {
  formData: ResourceFormCommand;
  loading: boolean;
  error: string | null;
  isEdit: boolean;
  handleChange: <K extends keyof ResourceFormCommand>(field: K, value: ResourceFormCommand[K]) => void;
  handleSubmit: (e: React.FormEvent) => void;
}

export const ResourceCreateUpdateForm: React.FC<ResourceCreateUpdateFormProps> = ({
  formData,
  loading,
  error,
  isEdit,
  handleChange,
  handleSubmit,
}) => {
  return (
     
      
      <Card className="max-w-2xl mx-auto p-6">
        
    <form onSubmit={handleSubmit} className="p-4 space-y-4">
      <h2 className="text-2xl font-bold text-center mb-4">
            {isEdit ? "ویرایش منبع" : "افزودن منبع جدید"}
            </h2>

      {error && <div className="alert alert-error">{error}</div>}

 
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
                onChange={(e) => handleChange('type',Number(e.target.value))}
                className="w-full p-2 border rounded"
                disabled={loading}
              >
                <option value='0' label="Module">ماژول (Module)</option>
                <option value='1' label="Ui">رابط کاربری (Ui)</option>
                <option value='2' label="Data">داده (Data)</option>
              </select>
            </div>
            <div className="mb-4">
              <label className="block mb-1">دسته‌بندی</label>
              <select
                value={formData.category}
                onChange={(e) => handleChange('category', Number(e.target.value))}
                className="w-full p-2 border rounded"
                disabled={loading}
              >
               <option value = '0' label= "General" > عمومی(General) </ option >
                <option value = '1' label="System">سیستمی(System)</option>
                <option value = '2' label= "Module" > ماژول(Module) </ option >
                <option value = '3' label="Menu">منو(Menu)</option>
                <option value = '4' label= "Page" > صفحه(Page) </ option >
                <option value = '5' label="Component">کامپوننت(Component)</option>
                <option value = '6' label= "DatabaseTable" > جدول دیتابیس(DatabaseTable)</option>
                <option value = '7' label= "RowInTable" > سطر جدول(RowInTable)</option>
              </select>
            </div>
            <div className="mb-4">
              <label className="block mb-1">ترتیب نمایش</label>
              <Input
                type="number"
                value={formData.displayOrder}
                onChange={(e) => handleChange('displayOrder', parseInt(e.target.value) || 0)}
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">آیکون (اختیاری)</label>
              <Input
                value={formData.icon}
                onChange={(e) => handleChange('icon', e.target.value)}
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">مسیر (Route - اختیاری)</label>
              <Input
                value={formData.route}
                onChange={(e) => handleChange('route', e.target.value)}
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">والد (ParentId - اختیاری)</label>
              <Input
                value={formData.parentId || ''}
                onChange={(e) => handleChange('parentId', e.target.value || null)}
                disabled={loading}
                placeholder="GUID والد"
              />
            </div>
           
           
            <Button type="submit" disabled={loading}>
              {loading ? "در حال ایجاد..." : "ایجاد نقش"}
            </Button>
          </form>
        </Card>
  );
};
