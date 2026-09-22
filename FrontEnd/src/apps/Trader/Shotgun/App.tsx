import { useRoutes, Navigate } from "react-router-dom";
import { useActiveModules } from "@/core/context/ModuleContext";
import { TraderShotgunPublicRoutes } from "@/modules/Trader";

export default function App() {
  const { loading } = useActiveModules();

  if (loading) {
    return <div>در حال بارگذاری تنظیمات…</div>;
  }

  const routes = useRoutes([
    ...TraderShotgunPublicRoutes,
    /* fallback */
    { path: "*", element: <Navigate to="/schedule-plans" replace /> },
  ]);

  return routes;
}