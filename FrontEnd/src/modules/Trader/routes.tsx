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
  SignalsPage, 
  BacktestDetailPage,
  BacktestsListPage,
  TradingEventsPage,
  HistoricalDataCollectionPage,
  BacktestComparePage,
  RiskDashboardPage,
  PriceCollectionPage,
  OrdersManagementPage,
  OrderPresetsManagementPage} from "./pages";
import { StrategyTuningDetailPage } from "./pages/StrategyTuning/StrategyTuningDetailPage";
import { StrategyTuningListPage } from "./pages/StrategyTuning/StrategyTuningListPage";

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
  // در TraderShotgunRoutes (قبل از route detail، چون path match باید قبل از :id باشد):
{ path: "trader/backtests/compare", element: <BacktestComparePage /> },
  { path: "trader/backtests/:id", element: <BacktestDetailPage /> },
{ path: "trader/historical-data", element: <HistoricalDataCollectionPage /> },
{ path: "trader/risk", element: <RiskDashboardPage /> },  
  { path: "trader/price-collection", element: <PriceCollectionPage /> },
  { path: "trader/orders",            element: <OrdersManagementPage /> },
  { path: "trader/order-presets",     element: <OrderPresetsManagementPage /> },
  

{ path: "trader/tuning", element: <StrategyTuningListPage /> },
{ path: "trader/tuning/:id", element: <StrategyTuningDetailPage /> },
];

export const traderPanelRoutes: RouteObject[] = [
  ...TraderShotgunRoutes,
];