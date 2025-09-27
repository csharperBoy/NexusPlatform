//src/modules/auth/components/forms/loginForm.tsx
import React, { useState } from "react";
import Button from "../../../../core/components/Button";
import { useLogin } from "../../hooks/useLogin";

export type LoginFormProps = {
  onSuccess?: () => void;
  className?: string;
};

const LoginForm: React.FC<LoginFormProps> = ({ onSuccess, className }) => {
  const { doLogin, loading, error } = useLogin();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await doLogin({ email, password });
    onSuccess?.();
  };

  return (
   <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-gray-100 to-gray-300">
  <div className="w-full max-w-sm bg-white rounded-2xl shadow-xl p-8 border border-gray-200">
    <h2 className="text-center text-3xl font-bold text-gray-800 mb-6">ورود به پنل مدیریت</h2>

    <form onSubmit={handleSubmit} className="space-y-5">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">ایمیل</label>
        <input
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder="مثلاً admin@proj.com"
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">رمز عبور</label>
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="رمز عبور خود را وارد کنید"
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <Button
        type="submit"
        disabled={loading}
        className="w-full py-2 text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition duration-150"
      >
        {loading ? "در حال ورود..." : "ورود"}
      </Button>

      {error && <p className="text-center text-sm text-red-500">{error}</p>}

      <div className="text-center text-sm text-gray-500 mt-4">
        حساب کاربری ندارید؟ <a href="/register" className="text-blue-600 hover:underline">ثبت‌نام</a>
      </div>
    </form>
  </div>
</div>

  );
};

export default LoginForm;
