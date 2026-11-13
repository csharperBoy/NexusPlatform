import React from "react";
import { useDashboard } from "../../hooks/useDashboard";
import {
  EventsCard,
  KillSwitchCard,
  OrdersTodayCard,
  PositionsCard,
  RunsCard,
  SessionsCard,
} from "./components/DashboardCards";

export const DashboardPage: React.FC = () => {
  const { data, loading, error, refresh, lastRefreshAt } = useDashboard();

  return (
    <div className="space-y-4 p-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-900 dark:text-white">
            داشبورد معاملات
          </h1>
          <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
            نمای کلی وضعیت سیستم — به‌روزرسانی خودکار هر ۱۵ ثانیه
          </p>
        </div>
        <div className="flex items-center gap-2">
          {lastRefreshAt && (
            <span className="text-[10px] text-gray-500">
              آخرین به‌روزرسانی: {lastRefreshAt.toLocaleTimeString("fa-IR")}
            </span>
          )}
          <button
            type="button"
            onClick={refresh}
            disabled={loading}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 disabled:opacity-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
          >
            🔄 بروزرسانی
          </button>
        </div>
      </div>

      {error && (
        <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
          ⚠️ {error}
        </div>
      )}

      {loading && !data && (
        <div className="p-12 text-center text-gray-500">در حال بارگذاری داشبورد...</div>
      )}

      {data && (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          <KillSwitchCard data={data.killSwitch} />
          <RunsCard data={data.runs} />
          <OrdersTodayCard data={data.ordersToday} />
          <PositionsCard data={data.positions} />
          <SessionsCard data={data.accountsSessions} />
          <EventsCard data={data.recentCriticalEvents} />
        </div>
      )}
    </div>
  );
};