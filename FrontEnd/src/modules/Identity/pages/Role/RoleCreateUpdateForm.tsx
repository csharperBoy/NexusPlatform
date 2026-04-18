// src/components/RoleCreateUpdateForm.tsx
import React from 'react';
import { CreateRoleCommand, RoleFormCommand } from '../../models/RoleCommands'; 
import Card from '@/core/components/Card';
import Button from '@/core/components/Button';
import Input from '@/core/components/Input';

interface RoleCreateUpdateFormProps {
  formData: RoleFormCommand;
  loading: boolean;
  error: string | null;
  isEdit: boolean;
  handleChange: <K extends keyof RoleFormCommand>(field: K, value: RoleFormCommand[K]) => void;
  handleSubmit: (e: React.FormEvent) => void;
}

export const RoleCreateUpdateForm: React.FC<RoleCreateUpdateFormProps> = ({
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
            {isEdit ? "ویرایش نقش" : "افزودن نقش جدید"}
            </h2>

      {error && <div className="alert alert-error">{error}</div>}

 
            <div >
              <label className="block mb-1">نام  (Name)</label>
              <Input
                value={formData.Name || ''}
                onChange={(e) => handleChange('Name', e.target.value)}
                required
                disabled={loading}
              />
            </div>
            <div >
              <label className="block mb-1">توضیحات</label>
              <Input
                value={formData.Description || ''}
                onChange={(e) => handleChange('Description', e.target.value)}
                
                disabled={loading}
              />
            </div>
            <div>
              <label className="block mb-1">ترتیب</label>
              <Input
                value={formData.OrderNum || 0}
                onChange={(e) => handleChange('OrderNum', e.target.valueAsNumber)}
                disabled={loading}                
                type='number'
              />
            </div>
           
           
            <Button type="submit" disabled={loading}>
              {loading ? "در حال ایجاد..." : "ایجاد نقش"}
            </Button>
          </form>
        </Card>
  );
};
