//src/apps/Trader/Shotgun/App.tsx
import { useRoutes, Navigate, Outlet } from "react-router-dom";
import { ProtectedRoute } from "@/modules/Identity";
import { identityPublicRoutes, identityPanelRoutes } from "@/modules/Identity";
import { authorizationPanelRoutes } from "@/modules/Authorization";
import { MainLayout } from "@/modules/DashboardCore";
import { useActiveModules } from "@/core/context/ModuleContext";
import { MenuProvider } from "@/core/context/MenuContext";
import {  TraderShotgunRoutes } from "@/modules/Trader";
import LoginPage from "./Pages/LoginPage";
import DashboardPage from "./Pages/DashboardPage";

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
        <MenuProvider>
        <ProtectedRoute>
          <MainLayout>
            <Outlet />
          </MainLayout>
        </ProtectedRoute>
        </MenuProvider>
      ),
      children: [

        { path: "/dashboard", element: <DashboardPage /> },
        /* مسیرهای خصوصی Identity */
        ...(activeModules.has("Identity") ? identityPanelRoutes : []),

        /* مسیرهای خصوصی Authorization */
        ...(activeModules.has("Authorization") ? authorizationPanelRoutes : []),

        
        /* مسیرهای خصوصی Scheduler */
        // ...(activeModules.has("Scheduler") ? schedulerPanelRoutes : []),

        /* مسیرهای خصوصی Trader */
        ...(activeModules.has("Trader") ? TraderShotgunRoutes : []),
        
     
      ],
    },

    /* مسیر پیش‌فرض */
    { path: "*", element: <Navigate to="/dashboard" replace /> },
  ]);

  return routes;
}
/* 
import { useRoutes, Navigate } from "react-router-dom";
import { useActiveModules } from "@/core/context/ModuleContext";
import { TraderShotgunRoutes } from "@/modules/Trader";
import { identityPublicRoutes, LoginPage } from "@/modules/Identity";
export default function App() {

  const { activeModules, loading } = useActiveModules();
  if (loading) {
    return <div>در حال بارگذاری تنظیمات…</div>;
  }

  const routes = useRoutes([
    // { path: "/login", element: <LoginPage /> },

    ...(activeModules.has("Identity")
      ? identityPublicRoutes//.filter((r) => r.path !== "/login") // حذف login duplicate
      : []),

    ...TraderShotgunRoutes,
    { path: "*", element: <Navigate to="/schedule-plans" replace /> },
  ]);

  return routes;
} */