// modules/auth/pages/LoginPage.tsx
import React from "react";
import LoginForm from "../components/Forms/LoginForm";
import { useNavigate } from "react-router-dom";

const LoginPage: React.FC = () => {
  const nav = useNavigate();
  return (
    <div className="max-w-md mx-auto mt-10 p-6 border rounded shadow">
      <h2 className="text-2xl font-bold mb-4">ورود</h2>
      <LoginForm onSuccess={() => nav("/dashboard")} />
    </div>
  );
};

export default LoginPage;
