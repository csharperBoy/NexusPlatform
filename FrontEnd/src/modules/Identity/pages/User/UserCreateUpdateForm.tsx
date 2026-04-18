// src/components/UserCreateUpdateForm.tsx
import React from 'react';
import { CreateUserCommand, UserFormCommand } from '../../models/UserCommands'; 
import Card from '@/core/components/Card';
import Button from '@/core/components/Button';
import Input from '@/core/components/Input';
// فرض کنید این کامپوننت‌ها موجود هستند
// import { FormInput } from './FormInput';
// import { Checkbox } from './Checkbox';

interface UserCreateUpdateFormProps {
  formData: UserFormCommand;
  rolesList: { id: string; name: string }[];
  loading: boolean;
  error: string | null;
  isEdit: boolean;
  handleChange: <K extends keyof UserFormCommand>(field: K, value: UserFormCommand[K]) => void;
  handleRolesChange: (roleId: string, checked: boolean) => void;
  handleSubmit: (e: React.FormEvent) => void;
}

export const UserCreateUpdateForm: React.FC<UserCreateUpdateFormProps> = ({
  formData,
  rolesList,
  loading,
  error,
  isEdit,
  handleChange,
  handleRolesChange,
  handleSubmit,
}) => {
  return (
     <Card className="max-w-2xl mx-auto p-6">
        
    <form onSubmit={handleSubmit} className="p-4 space-y-4">
      <h2 className="text-2xl font-bold text-center mb-4">
        {isEdit ? "ویرایش کاربر" : "افزودن کاربر جدید"}
      </h2>

      {error && <div className="alert alert-error">{error}</div>}

      <div>
        <label htmlFor="userName">نام کاربری:</label>
        <Input
          id="userName"
          type="text"
          value={formData.UserName || ""}
          onChange={(e) => handleChange("UserName", e.target.value)}
          className="input input-bordered w-full"
          required={true} // همیشه الزامی است
        />
      </div>

      {!isEdit && ( // فقط در حالت ایجاد نمایش داده شود
        <div>
          <label htmlFor="password">رمز عبور:</label>
          <Input
            id="password"
            type="password"
            value={(formData as CreateUserCommand).Password || ""} // Type assertion
            onChange={(e) => handleChange("Password", e.target.value)}
            className="input input-bordered w-full"
            required={true}
          />
        </div>
      )}

      <div>
        <label htmlFor="email">ایمیل:</label>
        <Input
          id="email"
          type="email"
          value={formData.Email || ""}
          onChange={(e) => handleChange("Email", e.target.value)}
          className="input input-bordered w-full"
          required={true}
        />
      </div>

      <div>
        <label htmlFor="nickName">نام (اختیاری):</label>
        <Input
          id="nickName"
          type="text"
          value={formData.NickName || ""}
          onChange={(e) => handleChange("NickName", e.target.value)}
          className="input input-bordered w-full"
        />
      </div>

       <div>
        <label htmlFor="phoneNumber">شماره تماس (اختیاری):</label>
        <Input
          id="phoneNumber"
          type="tel"
          value={formData.phoneNumber || ""}
          onChange={(e) => handleChange("phoneNumber", e.target.value)}
          className="input input-bordered w-full"
        />
      </div>

      {/* بخش انتخاب نقش‌ها */}
      <div>
        <label className="block mb-2 font-medium">نقش‌ها:</label>
        {rolesList.length === 0 && !error && <p>در حال بارگذاری نقش‌ها...</p>}
        {rolesList.map(role => (
          <div key={role.name} className="flex items-center space-x-2 mb-1">
            <input
              type="checkbox"
              id={`role-${role.name}`}
              checked={formData.roles?.includes(role.name) || false}
              onChange={(e) => handleRolesChange(role.name, e.target.checked)}              
              className="mr-2"
            />
            <label htmlFor={`role-${role.name}`}>{role.name}</label>
          </div>
        ))}
      </div>

      <Button type="submit" disabled={loading} className="btn btn-primary w-full">
        {loading ? "در حال پردازش..." : (isEdit ? "ذخیره تغییرات" : "افزودن کاربر")}
      </Button>
    </form>
    </Card>
  );
};
