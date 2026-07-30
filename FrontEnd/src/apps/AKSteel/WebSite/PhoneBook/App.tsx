// src/apps/AKSteel/Website/PhoneBook/App.tsx
import { useRoutes, Navigate, Outlet } from "react-router-dom";
import { useActiveModules } from "@/core/context/ModuleContext";
import PhoneBookPage from "@/modules/PhoneBook/pages/PhoneBook/PhoneBookPage";

export default function App() {
  
    console.info('start:');
    console.warn('start=');
  const { activeModules, loading } = useActiveModules();

  if (loading) {
    // می‌توانید یک اسلایدر یا spinner سفارشی قرار دهید
    return <div>در حال بارگذاری تنظیمات…</div>;
  }

  const routes = useRoutes([

        { path: "/", element: <PhoneBookPage /> },


    /* مسیر پیش‌فرض */
    { path: "*", element: <Navigate to="/" replace /> },
  ]);

  return routes;
}
