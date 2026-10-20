//src/modules/Trader/routes.tsx
import type { RouteObject } from "react-router-dom";
import { AccountsManagementPage } from "./pages/Accounts/AccountsManagementPage";
import { InstrumentsManagementPage } from "./pages/Instruments/InstrumentsManagementPage";
import { SchedulePlansManagementPage } from "./pages/SchedulePlans/SchedulePlansManagementPage";
import { ServerClockPage } from "./pages/ServerClock/ServerClockPage";
import { LogViewerPage } from "./pages/Logs/LogViewerPage";

/* ─── استفاده در TraderShotgun app (داخل MainLayout پلتفرم) ─── */
export const TraderShotgunRoutes: RouteObject[] = [
  { path: "trader/accounts",       element: <AccountsManagementPage /> },
  { path: "trader/instruments",    element: <InstrumentsManagementPage /> },
  { path: "trader/schedule-plans", element: <SchedulePlansManagementPage /> },
  { path: "trader/server-clock",   element: <ServerClockPage /> },
  { path: "trader/logs",           element: <LogViewerPage /> },
];

/* ─── برای پنل ادمین پلتفرم (flat) ─── */
export const traderPanelRoutes: RouteObject[] = [
  ...TraderShotgunRoutes,
];