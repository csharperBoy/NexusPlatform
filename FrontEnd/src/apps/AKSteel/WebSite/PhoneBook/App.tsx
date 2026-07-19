// src/apps/Trader/Server/App.tsx
import { useRoutes, Navigate, Outlet } from "react-router-dom";
import { ProtectedRoute } from "@/modules/Identity";
import { identityPublicRoutes, identityPanelRoutes } from "@/modules/Identity";
import { authorizationPanelRoutes } from "@/modules/Authorization";
import { useActiveModules } from "@/core/context/ModuleContext";
import HomePage from "./Pages/Home";

export default function App() {
  const { activeModules, loading } = useActiveModules();

  if (loading) {
    // می‌توانید یک اسلایدر یا spinner سفارشی قرار دهید
    return <div>در حال بارگذاری تنظیمات…</div>;
  }

  const routes = useRoutes([

        { path: "/home", element: <HomePage /> },


    /* مسیر پیش‌فرض */
    { path: "*", element: <Navigate to="/home" replace /> },
  ]);

  return routes;
}
