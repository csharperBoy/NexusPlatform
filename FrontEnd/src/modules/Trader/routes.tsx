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
  CurrencyRatesManagementPage,
  SignalsPage, BacktestDetailPage,BacktestsListPage,
  TradingEventsPage} from "./pages";

export const TraderShotgunRoutes: RouteObject[] = [
  { path: "trader/accounts",       element: <AccountsManagementPage /> },
  { path: "trader/instruments",    element: <InstrumentsManagementPage /> },
  { path: "trader/strategies",     element: <StrategiesListPage /> },
  { path: "trader/trading-runs",   element: <TradingRunsManagementPage /> },
  { path: "trader/currency-rates", element: <CurrencyRatesManagementPage /> }, 
  { path: "trader/positions",      element: <PositionsPage /> },
  { path: "trader/signals",        element: <SignalsPage /> },
  { path: "trader/schedule-plans", element: <SchedulePlansManagementPage /> },
  { path: "trader/server-clock",   element: <ServerClockPage /> },
  { path: "trader/logs",           element: <LogViewerPage /> },
  { path: "trader/events", element: <TradingEventsPage /> },
  { path: "trader/backtests", element: <BacktestsListPage /> },
  { path: "trader/backtests/:id", element: <BacktestDetailPage /> },
];

export const traderPanelRoutes: RouteObject[] = [
  ...TraderShotgunRoutes,
];