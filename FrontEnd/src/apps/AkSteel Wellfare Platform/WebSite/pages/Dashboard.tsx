import React from "react";
import { useAuth } from "@/modules/auth/hooks/useAuth";
import { useNavigate } from "react-router-dom";

const Dashboard: React.FC = () => {
  const { logout } = useAuth();
  const nav = useNavigate();

  const handleLogout = () => {
    logout();            // پاک کردن توکن و user
    nav("/login");       // هدایت به صفحه ورود
  };

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold">داشبورد وب‌سایت رفاهی</h1>
      <p className="mt-4">این بخش مخصوص کاربرهای لاگین شده است.</p>

      <button
        onClick={handleLogout}
        className="mt-6 px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
      >
        خروج
      </button>
    </div>
  );
};

export default Dashboard;
