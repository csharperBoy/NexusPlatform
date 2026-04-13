// src/modules/Identity/pages/LoginPage.tsx
import React from 'react';
import { LoginPageWithCustomForm, type RenderFormProps } from '../Interface/ILoginPage';

/**
 * صفحه لاگین پیش‌فرض ماژول Identity
 * بسیار سبک و بدون وابستگی به فریمورک ظاهری
 * مناسب برای پروژه‌هایی که ظاهر اهمیت ندارد
 */
const LoginPage: React.FC = () => {
  return (
    <LoginPageWithCustomForm
      redirectTo="/dashboard"
      renderForm={({
        identifier,
        password,
        loading,
        error,
        setIdentifier,
        setPassword,
        handleSubmit,
      }: RenderFormProps) => (
        <div style={{ maxWidth: '400px', margin: '50px auto', padding: '20px', border: '1px solid #ccc', borderRadius: '8px' }}>
          <h2 style={{ textAlign: 'center', marginBottom: '20px' }}>ورود به سیستم</h2>
          <form onSubmit={handleSubmit}>
            <div style={{ marginBottom: '15px' }}>
              <label htmlFor="identifier" style={{ display: 'block', marginBottom: '5px' }}>نام کاربری یا ایمیل</label>
              <input
                type="text"
                id="identifier"
                value={identifier}
                onChange={(e) => setIdentifier(e.target.value)}
                style={{ width: '100%', padding: '8px', border: '1px solid #ccc', borderRadius: '4px' }}
                disabled={loading}
                required
              />
            </div>
            <div style={{ marginBottom: '15px' }}>
              <label htmlFor="password" style={{ display: 'block', marginBottom: '5px' }}>رمز عبور</label>
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
              {loading ? 'در حال ورود...' : 'ورود'}
            </button>
          </form>
        </div>
      )}
    />
  );
};

export default LoginPage;