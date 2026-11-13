//src/apps/Trader/App.tsx
import { useRoutes, Navigate, Outlet } from "react-router-dom";
import { ProtectedRoute } from "@/modules/Identity";
import { identityPublicRoutes, identityPanelRoutes } from "@/modules/Identity";
import { authorizationPanelRoutes } from "@/modules/Authorization";
import { MainLayout } from "@/modules/DashboardCore";
import { useActiveModules } from "@/core/context/ModuleContext";
import { MenuProvider } from "@/core/context/MenuContext";
import { TraderShotgunRoutes } from "@/modules/Trader";
import LoginPage from "./Pages/LoginPage";
import DashboardPage from "./Pages/DashboardPage";

export default function App() {
  const { activeModules, loading } = useActiveModules();

  // useRoutes MUST always be called — Rules of Hooks.
  // Loading state is expressed *inside* the routes array, not as an early return.
  const routes = useRoutes([
    { path: "/login", element: <LoginPage /> },

    ...(!loading && activeModules.has("Identity")
      ? identityPublicRoutes.filter((r) => r.path !== "/login")
      : []),

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
      children: loading
        ? []
        : [
            { path: "/dashboard", element: <DashboardPage /> },
            ...(activeModules.has("Identity") ? identityPanelRoutes : []),
            ...(activeModules.has("Authorization") ? authorizationPanelRoutes : []),
            ...(activeModules.has("Trader") ? TraderShotgunRoutes : []),
          ],
    },

    {
      path: "*",
      element: loading ? (
        <div className="flex h-screen items-center justify-center text-gray-500">
          در حال بارگذاری تنظیمات…
        </div>
      ) : (
        <Navigate to="/dashboard" replace />
      ),
    },
  ]);

  return routes;
}