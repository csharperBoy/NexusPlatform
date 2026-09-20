 // src/apps/Trader/Shotgun/App.tsx
 import { useRoutes, Navigate, Outlet } from "react-router-dom";
 import { useActiveModules } from "@/core/context/ModuleContext";
 import { TraderShotgunPublicRoutes  } from "@/modules/Trader";

 export default function App() {

   const { activeModules, loading } = useActiveModules();

   if (loading) {
    return <div>در حال بارگذاری تنظیمات…</div>;
   }

   const routes = useRoutes([
   ...TraderShotgunPublicRoutes, 

     /* مسیر پیش‌فرض */
     { path: "*", element: <Navigate to="/" replace /> },
   ]);

   return routes;
 }
