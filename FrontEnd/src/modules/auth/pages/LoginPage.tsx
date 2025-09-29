// modules/auth/pages/LoginPage.tsx
 import React from "react";
 import LoginForm from "../components/Forms/LoginForm";
 import { useNavigate } from "react-router-dom";
 import { useUIConfig } from "@/core/context/UIProvider";

 const LoginPage: React.FC = () => {
   const nav = useNavigate();
   const { theme, style } = useUIConfig();
   console.log("LoginPage theme/style:", theme, style);

   return <LoginForm onSuccess={() => nav("/dashboard")} />;
 };

 export default LoginPage;
