import { Navigate, type RouteObject } from "react-router-dom";
import { TraderLayout } from "./components/TraderLayout";
import { AccountsManagementPage } from "./pages/Accounts/AccountsManagementPage";
import { SymbolsManagementPage } from "./pages/Symbols/SymbolsManagementPage";
import { SchedulePlansManagementPage } from "./pages/SchedulePlans/SchedulePlansManagementPage";
import { ServerClockPage } from "./pages/ServerClock/ServerClockPage";

/* ─── برای استفاده در پنل ادمین (flat) ─── */
export const traderPanelRoutes: RouteObject[] = [
  { path: "trader/schedule-plans", element: <SchedulePlansManagementPage /> },
  { path: "trader/accounts", element: <AccountsManagementPage /> },
  { path: "trader/symbols", element: <SymbolsManagementPage /> },
  { path: "trader/server-clock", element: <ServerClockPage /> },
];

/* ─── برای Shotgun app (با Layout) ─── */
export const TraderShotgunPublicRoutes: RouteObject[] = [
  {
    path: "/",
    element: <TraderLayout />,
    children: [
      { index: true, element: <Navigate to="/schedule-plans" replace /> },
      { path: "schedule-plans", element: <SchedulePlansManagementPage /> },
      { path: "accounts", element: <AccountsManagementPage /> },
      { path: "symbols", element: <SymbolsManagementPage /> },
      { path: "server-clock", element: <ServerClockPage /> },
    ],
  },
];