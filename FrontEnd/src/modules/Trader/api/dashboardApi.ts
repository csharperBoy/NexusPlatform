import getAPI from "@/core/api/axiosClient";
import type { DashboardSummaryView } from "../models";
import { mockDelay } from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";

export const dashboardApi = {
  GetSummary: async (): Promise<DashboardSummaryView> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return {
        killSwitch: { globalActive: false, globalReason: null, globalActivatedAt: null, activeAccounts: [] },
        runs: { running: 0, ready: 0, stopped: 0, faulted: 0, created: 0, total: 0 },
        ordersToday: {
          todayStartUtc: new Date().toISOString(),
          total: 0, paper: 0, live: 0, pending: 0, open: 0,
          partiallyExecuted: 0, fullyExecuted: 0, canceled: 0, rejected: 0, expired: 0,
        },
        positions: { totalCount: 0, paperCount: 0, liveCount: 0, totalUnrealizedPnL: 0, items: [] },
        recentCriticalEvents: [],
        accountsSessions: { total: 0, valid: 0, expired: 0, empty: 0, invalid: 0 },
        generatedAt: new Date().toISOString(),
      };
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<DashboardSummaryView>(
      "/api/Trader/Dashboard/Summary",
      { withCredentials: true },
    );
    return response.data;
  },
};