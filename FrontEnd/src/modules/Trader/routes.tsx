//src/modules/Trader/routes.tsx
import type { RouteObject } from "react-router-dom";
import { 
  TradingRunsManagementPage , 
  StrategiesListPage,
  LogViewerPage,
  ServerClockPage,
  SchedulePlansManagementPage,
  InstrumentsManagementPage,
  AccountsManagementPage } from "./pages";

/* ─── استفاده در TraderShotgun app (داخل MainLayout پلتفرم) ─── */
export const TraderShotgunRoutes: RouteObject[] = [
  { path: "trader/accounts",       element: <AccountsManagementPage /> },
  { path: "trader/instruments",    element: <InstrumentsManagementPage /> },
  { path: "trader/schedule-plans", element: <SchedulePlansManagementPage /> },
  { path: "trader/server-clock",   element: <ServerClockPage /> },
  { path: "trader/logs",           element: <LogViewerPage /> },  
{ path: "trader/strategies", element: <StrategiesListPage /> },

  { path: "trader/trading-runs",   element: <TradingRunsManagementPage /> },
];

/* ─── برای پنل ادمین پلتفرم (flat) ─── */
export const traderPanelRoutes: RouteObject[] = [
  ...TraderShotgunRoutes,
];