// // src/apps/Trader/Shotgun/App.tsx
// import { useRoutes, Navigate, Outlet } from "react-router-dom";
// import { useActiveModules } from "@/core/context/ModuleContext";
// import { TraderShotgunPublicRoutes  } from "@/modules/Trader";

// export default function App() {
  
//   const { activeModules, loading } = useActiveModules();

//   if (loading) {
//     return <div>در حال بارگذاری تنظیمات…</div>;
//   }

//   const routes = useRoutes([
//   ...TraderShotgunPublicRoutes, 

//     /* مسیر پیش‌فرض */
//     { path: "*", element: <Navigate to="/" replace /> },
//   ]);

//   return routes;
// }
import { useAccountsStore ,useSymbolsStore} from "@/modules/Trader";

export default function App() {
  const accounts = useAccountsStore((s) => s.accounts);
  const symbols = useSymbolsStore((s) => s.symbols);
  return (
    <div className="p-4">
      <div>accounts: {accounts.length}</div>
      <div>symbols: {symbols.length}</div>
    </div>
  );
}