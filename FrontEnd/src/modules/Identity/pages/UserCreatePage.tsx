// src/modules/Identity/pages/UserCreatePage.tsx
import React from 'react';
import { UserCreateForm, type RenderFormProps } from '../Interface/IUserCreatePage';
import Button from '@/core/components/Button';
import Input from '@/core/components/Input';
import Card from '@/core/components/Card';


const UserCreatePage: React.FC = () => {
  return (
    <UserCreateForm
      redirectTo="/users"
      renderForm={({
        formData,
        loading,
        error,
        handleChange,
        handleSubmit,
      }: RenderFormProps) => (
        <Card className="max-w-2xl mx-auto p-6">
          <h2 className="text-xl font-bold mb-4">ایجاد کاربر جدید</h2>
          <form onSubmit={handleSubmit}>
            <div className="mb-4">
              <label className="block mb-1">نام کاربری (UserName)</label>
              <Input
                value={formData.UserName}
                onChange={(e) => handleChange('UserName', e.target.value)}
                required
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">نام</label>
              <Input
                value={formData.NickName || ''}
                onChange={(e) => handleChange('NickName', e.target.value)}
                
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">رمز عیور</label>
              <Input
                value={formData.Password || ''}
                onChange={(e) => handleChange('Password', e.target.value)}
                disabled={loading}
                required
                type='password'
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">ایمیل</label>
              <Input
                value={formData.Email || ''}
                onChange={(e) => handleChange('Email', e.target.value)}
                disabled={loading}
                required
                type='email'
              />
            </div>
            
            <div className="mb-4">
              <label className="block mb-1">تلفن همراه</label>
              <Input
                type="number"
                value={formData.phoneNumber || ''}
                onChange={(e) => handleChange('phoneNumber', parseInt(e.target.value) || null)}
                disabled={loading}
              />
            </div>
            <div className="mb-4">
              <label className="block mb-1">آیدی شخص</label>
              <Input
                value={formData.personId || ''}
                onChange={(e) => handleChange('personId', e.target.value)}
                disabled={loading}
              />
            </div>
           
            {error && (
              <div className="text-red-600 mb-4 p-3 bg-red-50 rounded">
                {error}
              </div>
            )}
            <Button type="submit" disabled={loading}>
              {loading ? "در حال ایجاد..." : "ایجاد کاربر"}
            </Button>
          </form>
        </Card>
      )}
    />
  );
};

export default UserCreatePage;