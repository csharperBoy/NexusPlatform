//src/modules/Trader/routes.tsx
import type { RouteObject } from "react-router-dom";
import { 
  TradingRunsManagementPage , 
  StrategiesListPage,
  LogViewerPage,
  ServerClockPage,
  SchedulePlansManagementPage,
  InstrumentsManagementPage,
  AccountsManagementPage,
  PositionsPage,
  SignalsPage } from "./pages";

export const TraderShotgunRoutes: RouteObject[] = [
  { path: "trader/accounts",       element: <AccountsManagementPage /> },
  { path: "trader/instruments",    element: <InstrumentsManagementPage /> },
  { path: "trader/strategies",     element: <StrategiesListPage /> },
  { path: "trader/trading-runs",   element: <TradingRunsManagementPage /> },
  { path: "trader/positions",      element: <PositionsPage /> },
  { path: "trader/signals",        element: <SignalsPage /> },
  { path: "trader/schedule-plans", element: <SchedulePlansManagementPage /> },
  { path: "trader/server-clock",   element: <ServerClockPage /> },
  { path: "trader/logs",           element: <LogViewerPage /> },
];

export const traderPanelRoutes: RouteObject[] = [
  ...TraderShotgunRoutes,
];