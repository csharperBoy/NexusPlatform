import React from "react";
import { LoginForm } from "@/modules/auth";

const LoginPage: React.FC = () => {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="max-w-md w-full bg-white rounded shadow p-6">
        <h2 className="text-xl font-bold mb-4 text-center">ورود به حساب کاربری</h2>
        <LoginForm onSuccess={() => (window.location.href = "/dashboard")} />
      </div>
    </div>
  );
};

export default LoginPage;
