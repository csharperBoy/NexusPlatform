// src/apps/AKSteel/Website/PhoneBook/App.tsx
import { useRoutes, Navigate, Outlet } from "react-router-dom";
import { useActiveModules } from "@/core/context/ModuleContext";
import PhoneBookPage from "@/modules/Contact/pages/PhoneBook/PhoneBookPage";
import { ContactPublicRoutes  } from "@/modules/Contact";

export default function App() {
  
    console.info('start:');
    console.warn('start=');
  const { activeModules, loading } = useActiveModules();

  if (loading) {
    // می‌توانید یک اسلایدر یا spinner سفارشی قرار دهید
    return <div>در حال بارگذاری تنظیمات…</div>;
  }

  const routes = useRoutes([

        // { path: "/", element: <PhoneBookPage /> },
   
  ...ContactPublicRoutes, 

    /* مسیر پیش‌فرض */
    { path: "*", element: <Navigate to="/" replace /> },
  ]);

  return routes;
}
