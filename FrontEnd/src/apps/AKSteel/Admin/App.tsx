// src/apps/Trader/Server/App.tsx
import { useRoutes, Navigate, Outlet } from "react-router-dom";
import { ProtectedRoute } from "@/modules/Identity";
import { identityPublicRoutes, identityPanelRoutes } from "@/modules/Identity";
import { authorizationPanelRoutes } from "@/modules/Authorization";
import { MainLayout } from "@/modules/DashboardCore";
import DashboardPage from "./Pages/DashboardPage";
import LoginPage from "./Pages/LoginPage";
import { useActiveModules } from "@/core/context/ModuleContext";
import HomePage from "./Pages/Home";
import { hrPanelRoutes } from "@/modules/HR";

export default function App() {
  
    console.info('start:');
    console.warn('start=');
  const { activeModules, loading } = useActiveModules();

  if (loading) {
    // می‌توانید یک اسلایدر یا spinner سفارشی قرار دهید
    return <div>در حال بارگذاری تنظیمات…</div>;
  }

  const routes = useRoutes([
    /* مسیر لاگین اختصاصی */
    { path: "/login", element: <LoginPage /> },

    /* مسیرهای عمومی ماژول Identity (مثل /register) فقط اگر Identity فعال باشد */
    ...(activeModules.has("Identity")
      ? identityPublicRoutes.filter((r) => r.path !== "/login") // حذف login duplicate
      : []),

    /* مسیرهای محافظت‌شده با Layout */
    {
      element: (
        <ProtectedRoute>
          <MainLayout>
            <Outlet />
          </MainLayout>
        </ProtectedRoute>
      ),
      children: [
        { path: "/dashboard", element: <DashboardPage /> },

        /* مسیرهای خصوصی Identity */
        ...(activeModules.has("Identity") ? identityPanelRoutes : []),

        /* مسیرهای خصوصی Authorization */
        ...(activeModules.has("Authorization") ? authorizationPanelRoutes : []),

        
        /* مسیرهای خصوصی hr */
        ...(activeModules.has("HR") ? hrPanelRoutes : []),

        
     
      ],
    },

    /* مسیر پیش‌فرض */
    { path: "*", element: <Navigate to="/dashboard" replace /> },
  ]);

  return routes;
}
