// src/modules/Authorization/pages/UserUpdatePage.tsx
import React from 'react';
import { UserUpdateForm, type RenderFormProps } from '../Interface/IUserUpdatePage';
import Button from '@/core/components/Button';
import Input from '@/core/components/Input';
import Card from '@/core/components/Card';

const UserUpdatePage: React.FC = () => {
  return (
    <UserUpdateForm
      redirectTo="/users"
      renderForm={({
        formData,
        loading,
        error,
        handleChange,
        handleSubmit,
      }: RenderFormProps) => (
        <Card className="max-w-2xl mx-auto p-6">
          <h2 className="text-xl font-bold mb-4">ویرایش کاربر </h2>
          <form onSubmit={handleSubmit}>
            <div className="mb-4">
              <label className="block mb-1">نام کاربری (UserName)</label>
              <Input
                value={formData?.UserName}
                onChange={(e) => handleChange('UserName', e.target.value)}
                required
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">نام</label>
              <Input
                value={formData?.FirstName || ''}
                onChange={(e) => handleChange('FirstName', e.target.value)}                
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">نام خانوادگی</label>
              <Input
                value={formData?.LastName || ''}
                onChange={(e) => handleChange('LastName', e.target.value)}
                disabled={loading}
              />
            </div>
            
            <div className="mb-4">
              <label className="block mb-1">ایمیل</label>
              <Input
                type="email"
                value={formData?.Email || ''}
                onChange={(e) => handleChange('Email', parseInt(e.target.value) || 0)}
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">تلفن همراه</label>
              <Input
                type="phoneNumber"
                value={formData?.phoneNumber || ''}
                onChange={(e) => handleChange('phoneNumber', e.target.value)}
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">رمز عبور</label>
              <Input
                type='password'
                value={formData?.Password || ''}
                onChange={(e) => handleChange('Password', e.target.value)}
                disabled={loading}
              />
            </div>
            
            {error && (
              <div className="text-red-600 mb-4 p-3 bg-red-50 rounded">
                {error}
              </div>
            )}
            <Button type="submit" disabled={loading}>
              {loading ? "در حال ویرایش..." : "ویرایش کاربر"}
            </Button>
          </form>
        </Card>
      )}
    />
  );
};

export default UserUpdatePage;