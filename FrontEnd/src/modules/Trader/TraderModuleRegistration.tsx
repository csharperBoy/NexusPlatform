import { lazy, useEffect } from "react";
import { usePlugin } from "@/modules/DashboardCore";

const TraderKillSwitchWidget = lazy(() => import("./widgets/TraderKillSwitchWidget"));
const TraderRunsWidget = lazy(() => import("./widgets/TraderRunsWidget"));
const TraderOrdersTodayWidget = lazy(() => import("./widgets/TraderOrdersTodayWidget"));
const TraderPositionsWidget = lazy(() => import("./widgets/TraderPositionsWidget"));
const TraderSessionsWidget = lazy(() => import("./widgets/TraderSessionsWidget"));
const TraderEventsWidget = lazy(() => import("./widgets/TraderEventsWidget"));
const SymbolStatsWidget = lazy(() => import("./widgets/SymbolStatsWidget"));

/**
 * Priority contract (for cross-module ordering):
 *   10   KillSwitch    (safety-critical، first)
 *   20   Runs          (execution context)
 *   30   SymbolStats   (existing، unchanged)
 *   40   OrdersToday   (execution activity)
 *   50   Positions     (open exposure)
 *   60   Sessions      (infrastructure)
 *   70   Events        (diagnostic)
 *
 * Gap of 10 between modules allows other modules to interleave.
 */
export const TraderModuleRegistration = () => {
  const { registerWidget } = usePlugin();

  useEffect(() => {
    registerWidget({
      id: "trader-kill-switch",
      area: "widgets",
      priority: 10,
      component: TraderKillSwitchWidget,
      title: "Kill Switch",
      permissions: ["trader.dashboard"],
    });
    registerWidget({
      id: "trader-runs",
      area: "widgets",
      priority: 20,
      component: TraderRunsWidget,
      title: "Trading Runs",
      permissions: ["trader.dashboard"],
    });
    registerWidget({
      id: "trader-symbol-stats",
      area: "widgets",
      priority: 30,
      component: SymbolStatsWidget,
      title: "آمار تعداد نمادها",
      permissions: ["trader.dashboard"],
    });
    registerWidget({
      id: "trader-orders-today",
      area: "widgets",
      priority: 40,
      component: TraderOrdersTodayWidget,
      title: "سفارش‌های امروز",
      permissions: ["trader.dashboard"],
    });
    registerWidget({
      id: "trader-positions",
      area: "widgets",
      priority: 50,
      component: TraderPositionsWidget,
      title: "موقعیت‌های باز",
      permissions: ["trader.dashboard"],
    });
    registerWidget({
      id: "trader-sessions",
      area: "widgets",
      priority: 60,
      component: TraderSessionsWidget,
      title: "حساب‌های کارگزاری",
      permissions: ["trader.dashboard"],
    });
    registerWidget({
      id: "trader-events",
      area: "widgets",
      priority: 70,
      component: TraderEventsWidget,
      title: "رویدادهای مهم",
      permissions: ["trader.dashboard"],
    });
  }, [registerWidget]);

  return null;
};