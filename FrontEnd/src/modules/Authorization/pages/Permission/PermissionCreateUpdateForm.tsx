// src/components/PermissionCreateUpdateForm.tsx
import React from 'react';
import { CreatePermissionCommand, PermissionFormCommand } from '../../models/PermissionCommands'; 
import Card from '@/core/components/Card';
import Button from '@/core/components/Button';
import Input from '@/core/components/Input';
import { data } from 'react-router-dom';
import { SelectionListDto } from '@/core/models/SelectionListDto';

interface PermissionCreateUpdateFormProps {
  formData: PermissionFormCommand;
  scopesList: { value: number; display: string }[];
  resourceList: SelectionListDto[];
  
  assignList  : SelectionListDto[];  
  loading: boolean;
  error: string | null;
  isEdit: boolean;
  handleChange: <K extends keyof PermissionFormCommand>(field: K, value: PermissionFormCommand[K]) => void;
  handleScopesChange: (scopeValue: number, checked: boolean) => void;
  handleSubmit: (e: React.FormEvent) => void;
  handleAssignTypeChange: (newAssignType: number)  => void;
}

const toInputDateTime = (date?: Date | null) => {
  if (!date) return "";
  return date.toISOString().slice(0, 16); // YYYY-MM-DDTHH:mm
};

export const PermissionCreateUpdateForm: React.FC<PermissionCreateUpdateFormProps> = ({
  formData,
  scopesList,
  resourceList,
  assignList,
  loading,
  error,
  isEdit,
  handleChange,
  handleScopesChange,
  handleSubmit,
  handleAssignTypeChange,

}) => {
  return (
     <Card className="max-w-2xl mx-auto p-6">
        
    <form onSubmit={handleSubmit} className="p-4 space-y-4">
      <h2 className="text-2xl font-bold text-center mb-4">
        {isEdit ? "ویرایش مجوز" : "افزودن مجوز جدید"}
      </h2>

      {error && <div className="alert alert-error">{error}</div>}

      {/* <div>
        <label htmlFor="ResourceId">ResourceId:</label>
        <Input
          id="ResourceId"
          type="text"
          value={formData.ResourceId || ""}
          onChange={(e) => handleChange("ResourceId", e.target.value)}
          className="input input-bordered w-full"
          required={true} // همیشه الزامی است
        />
      </div> */}
      <div className="mb-4">
              <label className="block mb-1">منبع</label>
              <select
                value={formData.ResourceId}
                onChange={(e) => handleChange('ResourceId',e.target.value)}
                className="w-full p-2 border rounded"
                disabled={loading}
                required={true}
              >
          
                 <option value="" disabled>انتخاب کنید...</option>
                  {resourceList.map((item) => (
                    <option key={item.value} value={item.value}>
                      {item.display}
                    </option>
                  ))}
                
              </select>
       </div>

       <div className="mb-4">
              <label className="block mb-1">عملیات</label>
              <select
                value={formData.Action}
                onChange={(e) => handleChange('Action',Number(e.target.value))}
                className="w-full p-2 border rounded"
                disabled={loading}
              >
          
                <option value='0' label="View">مشاهده (View)</option>
                <option value='1' label="Create">ایجاد (Create)</option>
                <option value='2' label="Edit">ویرایش (Edit)</option>
                
                <option value='3' label="Delete">حذف (Delete)</option>
                
                <option value='4' label="Export">خروجی (Export)</option>
                
                <option value='99' label="Full">کامل (Full)</option>
              </select>
       </div>
       <div className="mb-4">
              <label className="block mb-1">مجاز / غیر مجاز</label>
              <select
                value={formData.effect}
                onChange={(e) => handleChange('effect',Number(e.target.value))}
                className="w-full p-2 border rounded"
                disabled={loading}
              >
                <option value='0' label="allow">مجاز (Module)</option>
                <option value='1' label="Deny">غیر مجاز (Ui)</option>
              </select>
       </div>
      <div className="mb-4">
              <label className="block mb-1">نوع</label>
              <select
                value={formData.AssigneeType}
                onChange={(e) => handleAssignTypeChange(Number(e.target.value))}
                className="w-full p-2 border rounded"
                disabled={loading}
              >
                <option value='0' label="Person">شخص (Person)</option>
                <option value='1' label="Position">موقعیت (Position)</option>
                <option value='2' label="Role">نقش (Role)</option>
                <option value='3' label="User">کاربر (User)</option>
              </select>
       </div>
       <div className="mb-4">
              <label className="block mb-1">AssigneeId</label>
              <select
                value={formData.AssigneeId}
                onChange={(e) => handleChange('AssigneeId',e.target.value)}
                className="w-full p-2 border rounded"
                disabled={loading}
                required={true}
              >
          
                 <option value="" disabled>انتخاب کنید...</option>
                  {assignList.map((item) => (
                    <option key={item.value} value={item.value}>
                      {item.display}
                    </option>
                  ))}
                
              </select>
       </div>
      
      <div>
        <label htmlFor="AssigneeId">AssigneeId :</label>
        <Input
          id="AssigneeId"
          type="text"
          value={formData.AssigneeId || ""}
          onChange={(e) => handleChange("AssigneeId", e.target.value)}
          className="input input-bordered w-full"
          required={true} // همیشه الزامی است
        />
      </div>

 <div>
        <label htmlFor="Description"> توضیحات:</label>
        <Input
          id="Description"
          type="text"
          value={formData.Description || ""}
          onChange={(e) => handleChange("Description", e.target.value)}
          className="input input-bordered w-full"
        />
      </div>
      

       
 <div>
        <label htmlFor="IsActive">فعال / غیرفعال :</label>
        <Input
          id="IsActive"
          type="checkbox"
          checked={formData.IsActive ?? false}
          onChange={(e) => handleChange("IsActive", e.target.checked)}
          className="input input-bordered w-full"
        />
      </div>

      
      <div>
        <label htmlFor="IsActive">تاریخ اعمال:</label>
        <Input
          id="IsActive"
          type="datetime-local"
          value={toInputDateTime( formData.EffectiveFrom )}
          onChange={(e) => handleChange("EffectiveFrom", e.target.valueAsDate)}
          className="input input-bordered w-full"
        />
      </div>

      <div>
        <label htmlFor="IsActive"> تاریخ انقضا:</label>
        <Input
          id="IsActive"
          type="datetime-local"
          value={toInputDateTime(formData.ExpiresAt) }
          onChange={(e) => handleChange("EffectiveFrom", e.target.valueAsDate)}
          className="input input-bordered w-full"
        />
      </div>

      
      
      {/* بخش انتخاب نقش‌ها */}
      <div>
        <label className="block mb-2 font-medium">محدوده ها:</label>
        {scopesList.length === 0 && !error && <p>در حال بارگذاری محدوده ها...</p>}
        {scopesList.map(scope => (
          <div key={scope.value} className="flex items-center space-x-2 mb-1">
            <input
              type="checkbox"
              id={`${scope.value}`}
              checked={formData.scopes?.includes(scope.value) || false}
              onChange={(e) => handleScopesChange(scope.value, e.target.checked)}              
              className="mr-2"
            />
            <label htmlFor={`${scope.value}`}>{scope.display}</label>
          </div>
        ))}
      </div>

      <Button type="submit" disabled={loading} className="btn btn-primary w-full">
        {loading ? "در حال پردازش..." : (isEdit ? "ذخیره تغییرات" : "افزودن مجوز")}
      </Button>
    </form>
    </Card>
  );
};
