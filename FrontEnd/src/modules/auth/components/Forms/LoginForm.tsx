// src/modules/auth/components/forms/LoginForm.tsx
import React, { useState } from "react";
import Button from "@/core/components/Button";
import Input from "@/core/components/Input";
import Card from "@/core/components/Card";
import { useLogin } from "../../hooks/useLogin";

export type LoginFormProps = {
  onSuccess?: () => void;
  className?: string;
};

const LoginForm: React.FC<LoginFormProps> = ({ onSuccess, className }) => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const { login, loading, error } = useLogin();

const handleSubmit = async (e: React.FormEvent) => {
  e.preventDefault();
  await login({ email, password });
  onSuccess?.();
};


  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100 dark:bg-gray-900">
      <Card className="w-full max-w-sm">
        <h2 className="text-center text-2xl font-bold mb-6">ورود به سیستم</h2>

        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="ایمیل"
          />

          <Input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="رمز عبور"
          />

          <Button type="submit" disabled={loading} className="w-full">
            {loading ? "در حال ورود..." : "ورود"}
          </Button>

          {error && <p className="text-center text-sm text-red-500">{error}</p>}

          <div className="text-center text-sm text-gray-500">
            حساب کاربری ندارید؟{" "}
            <a href="/register" className="text-blue-600 hover:underline">
              ثبت‌نام
            </a>
          </div>
        </form>
      </Card>
    </div>
  );
};

export default LoginForm;
