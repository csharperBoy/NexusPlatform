// modules/identity/pages/RegisterPage.tsx

import React from 'react';
import { RegisterPageWithCustomForm, type RenderRegisterFormProps } from '../Interface/IRegisterPage';

const RegisterPage: React.FC = () => {
  return (
    <RegisterPageWithCustomForm
      redirectTo="/dashboard"
      renderForm={({
        username,
        email,
        password,
        confirmPassword,
        displayName,
        loading,
        error,
        setUsername,
        setEmail,
        setPassword,
        setConfirmPassword,
        setDisplayName,
        handleSubmit,
      }: RenderRegisterFormProps) => (
        <div style={{ maxWidth: '400px', margin: '50px auto', padding: '20px', border: '1px solid #ccc', borderRadius: '8px' }}>
          <h2 style={{ textAlign: 'center', marginBottom: '20px' }}>ثبت‌نام در سیستم</h2>
          <form onSubmit={handleSubmit}>
            {/* فیلد نام کاربری */}
            <div style={{ marginBottom: '15px' }}>
              <label htmlFor="username" style={{ display: 'block', marginBottom: '5px' }}>نام کاربری *</label>
              <input
                type="text"
                id="username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                style={{ width: '100%', padding: '8px', border: '1px solid #ccc', borderRadius: '4px' }}
                disabled={loading}
                required
              />
            </div>
            {/* فیلد ایمیل */}
            <div style={{ marginBottom: '15px' }}>
              <label htmlFor="email" style={{ display: 'block', marginBottom: '5px' }}>ایمیل *</label>
              <input
                type="email"
                id="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                style={{ width: '100%', padding: '8px', border: '1px solid #ccc', borderRadius: '4px' }}
                disabled={loading}
                required
              />
            </div>
            {/* فیلد نام نمایشی (اختیاری) */}
            <div style={{ marginBottom: '15px' }}>
              <label htmlFor="displayName" style={{ display: 'block', marginBottom: '5px' }}>نام نمایشی (اختیاری)</label>
              <input
                type="text"
                id="displayName"
                value={displayName}
                onChange={(e) => setDisplayName(e.target.value)}
                style={{ width: '100%', padding: '8px', border: '1px solid #ccc', borderRadius: '4px' }}
                disabled={loading}
              />
            </div>
            {/* فیلد رمز عبور */}
            <div style={{ marginBottom: '15px' }}>
              <label htmlFor="password" style={{ display: 'block', marginBottom: '5px' }}>رمز عبور *</label>
              <input
                type="password"
                id="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                style={{ width: '100%', padding: '8px', border: '1px solid #ccc', borderRadius: '4px' }}
                disabled={loading}
                required
              />
            </div>
            {/* فیلد تکرار رمز عبور */}
            <div style={{ marginBottom: '15px' }}>
              <label htmlFor="confirmPassword" style={{ display: 'block', marginBottom: '5px' }}>تکرار رمز عبور *</label>
              <input
                type="password"
                id="confirmPassword"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                style={{ width: '100%', padding: '8px', border: '1px solid #ccc', borderRadius: '4px' }}
                disabled={loading}
                required
              />
            </div>
            {error && (
              <div style={{ marginBottom: '15px', color: 'red', textAlign: 'center' }}>
                {error}
              </div>
            )}
            <button
              type="submit"
              disabled={loading}
              style={{ width: '100%', padding: '10px', backgroundColor: '#007bff', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
            >
              {loading ? 'در حال ثبت‌نام...' : 'ثبت‌نام'}
            </button>
          </form>
        </div>
      )}
    />
  );
};

export default RegisterPage;
