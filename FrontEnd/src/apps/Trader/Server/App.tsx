// src/apps/Trader/Server/App.tsx
import { useRoutes, Navigate, Outlet } from "react-router-dom";
import { 
  identityPublicRoutes, 
  identityPanelRoutes, 
  ProtectedRoute 
} from "@/modules/Identity";

import { 
  authorizationPanelRoutes
} from "@/modules/Authorization";
import { MainLayout } from "@/modules/DashboardCore";
import DashboardPage from "./pages/DashboardPage";
import LoginPage from "./pages/LoginPage"; // صفحه اختصاصی لاگین

export default function App() {
  const routes = useRoutes([
    // 1. مسیر سفارشی لاگین (اولویت بالاتر)
    {
      path: "/login",
      element: <LoginPage />,
    },
    // 2. مسیرهای عمومی Identity (مثلاً /register) - اگر تداخل نداشته باشند
    ...identityPublicRoutes,
    
    // 3. مسیرهای محافظت‌شده با Layout
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
        ...identityPanelRoutes, // مسیرهای خصوصی (مثلاً /users)
        ...authorizationPanelRoutes,
      ],
    },
    
    // 4. مسیر پیش‌فرض
    { path: "*", element: <Navigate to="/dashboard" replace /> },
  ]);

  return routes;
}