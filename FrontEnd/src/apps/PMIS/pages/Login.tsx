// src/pages/Login.tsx
import React, { useState, useEffect } from 'react';
import ChangeThemes from '../components/ChangesThemes';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import toast from 'react-hot-toast';

const Login: React.FC = () => {
  const navigate = useNavigate();
  const { login, isAuthenticated, loading: authLoading } = useAuth();
  const [userName, setUserName] = useState('');
  const [password, setPassword] = useState('');
  const [remember, setRemember] = useState(true);
  const [loading, setLoading] = useState(false);

  // اگر کاربر قبلاً لاگین کرده باشد، مستقیم به صفحهٔ هوم برود
  useEffect(() => {
    if (!authLoading && isAuthenticated) {
      navigate('/', { replace: true });
    }
  }, [authLoading, isAuthenticated, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!userName.trim() || !password) {
      toast.error('نام‌کاربری و رمز عبور نمی‌تواند خالی باشد.');
      return;
    }

    setLoading(true);
    try {
      await login({ userName, password });
      navigate('/', { replace: true });
    } catch (err: any) {
      const msg = err.response?.data?.message || 'خطا در ورود';
      toast.error(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="w-full p-0 m-0">
      <div className="w-full min-h-screen flex justify-center items-center bg-base-200 relative">
        <div className="absolute top-5 right-5 z-[99]">
          <ChangeThemes />
        </div>
        <form
          onSubmit={handleSubmit}
          className="w-full h-screen xl:h-auto xl:w-[30%] 2xl:w-[25%] 3xl:w-[20%] bg-base-100 rounded-lg shadow-md flex flex-col items-center p-5 pb-7 gap-8 pt-20 xl:pt-7"
        >
          <div className="flex items-center gap-1 xl:gap-2">
            <img
              src="/logo.png"
              alt="PMIS Logo"
              className="w-8 h-8 sm:w-10 sm:h-10 xl:w-10 xl:h-10 text-primary"
            />
            <span className="text-[18px] leading-[1.2] sm:text-lg xl:text-3xl 2xl:text-3xl font-semibold text-base-content dark:text-neutral-200">
              PMIS
            </span>
          </div>

          <div className="w-full flex flex-col items-stretch gap-3">
            <label className="input input-bordered min-w-full flex items-center gap-2">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 16 16"
                fill="currentColor"
                className="w-4 h-4 opacity-70"
              >
                <path d="M2.5 3A1.5 1.5 0 0 0 1 4.5v.793c.026.009.051.02.076.032L7.674 8.51c.206.1.446.1.652 0l6.598-3.185A.755.755 0 0 1 15 5.293V4.5A1.5 1.5 0 0 0 13.5 3h-11Z" />
                <path d="M15 6.954 8.978 9.86a2.25 2.25 0 0 1-1.956 0L1 6.954V11.5A1.5 1.5 0 0 0 2.5 13h11a1.5 1.5 0 0 0 1.5-1.5V6.954Z" />
              </svg>
              <input
                type="text"
                value={userName}
                onChange={e => setUserName(e.target.value)}
                className="grow input outline-none focus:outline-none border-none border-[0px] h-auto pl-1 pr-0"
                placeholder="نام کاربری"
                disabled={loading}
              />
            </label>
            <label className="input input-bordered flex items-center gap-2">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 16 16"
                fill="currentColor"
                className="w-4 h-4 opacity-70"
              >
                <path
                  fillRule="evenodd"
                  d="M14 6a4 4 0 0 1-4.899 3.899l-1.955 1.955a.5.5 0 0 1-.353.146H5v1.5a.5.5 0 0 1-.5.5h-2a.5.5 0 0 1-.5-.5v-2.293a.5.5 0 0 1 .146-.353l3.955-3.955A4 4 0 1 1 14 6Zm-4-2a.75.75 0 0 0 0 1.5.5.5 0 0 1 .5.5.75.75 0 0 0 1.5 0 2 2 0 0 0-2-2Z"
                  clipRule="evenodd"
                />
              </svg>
              <input
                type="password"
                value={password}
                onChange={e => setPassword(e.target.value)}
                className="grow input outline-none focus:outline-none border-none border-[0px] h-auto pl-1 pr-0"
                placeholder="رمز عبور"
                disabled={loading}
              />
            </label>
            <div className="flex items-center justify-between">
              <div className="form-control">
                <label className="label cursor-pointer gap-2">
                  <input
                    type="checkbox"
                    checked={remember}
                    onChange={e => setRemember(e.target.checked)}
                    className="checkbox w-4 h-4 rounded-md checkbox-primary"
                    disabled={loading}
                  />
                  <span className="label-text text-xs">به خاطر بسپار</span>
                </label>
              </div>
              <a
                href="#"
                className="link link-primary font-semibold text-xs no-underline"
              >
                فراموشی رمز عبور
              </a>
            </div>
            <button
              type="submit"
              disabled={loading}
              className={`btn btn-block btn-primary ${loading ? 'loading' : ''}`}
            >
              ورود
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default Login;
