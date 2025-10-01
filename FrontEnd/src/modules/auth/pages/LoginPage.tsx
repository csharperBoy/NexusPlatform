// modules/auth/pages/LoginPage.tsx
import React, { useEffect } from "react";
import LoginForm from "../components/Forms/LoginForm";
import { useNavigate } from "react-router-dom";
import { useUIConfig } from "@/core/context/UIProvider";
import { useAuth } from "../hooks/useAuth"; // فرض می‌کنیم hook وضعیت کاربر رو برمی‌گردونه

const LoginPage: React.FC = () => {
  const nav = useNavigate();
  const { theme, style } = useUIConfig();
  const { user } = useAuth(); // user = null اگر لاگین نشده باشه

  console.log("LoginPage theme/style:", theme, style);
  console.log("LoginPage user:", user);

  useEffect(() => {
    if (user) {
      nav("/dashboard");
    }
  }, [user, nav]);

  if (user) return null;

  return <LoginForm onSuccess={() => nav("/dashboard")} />;
};

export default LoginPage;
