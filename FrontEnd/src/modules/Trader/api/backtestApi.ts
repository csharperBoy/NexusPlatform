import getAPI from "@/core/api/axiosClient";
import type {
  RunBacktestModel,
  BacktestRunSummaryView,
  BacktestRunDetailView,
  BacktestTradeView,
  BacktestDailyEquityView,
} from "../models";
import { mockDelay } from "./_mockStorage";
import type { BacktestEventView, BacktestEventQuery } from "../models";

export interface PagedResult<T> {
  items: T[];
  totalRecords: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";

export const backtestApi = {
  Run: async (model: RunBacktestModel): Promise<string> => {
    if (USE_MOCK) {
      await mockDelay(300);
      return crypto.randomUUID();
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<string>(
      "/api/Trader/Backtest/Run",
      { model },
      { withCredentials: true },
    );
    return response.data;
  },

  Cancel: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<boolean>(
      `/api/Trader/Backtest/Cancel/${id}`,
      {},
      { withCredentials: true },
    );
    return response.data;
  },

  GetList: async (
    sourceRunId?: string,
    take = 100,
  ): Promise<BacktestRunSummaryView[]> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return [];
    }
    const api = getAPI(API_MODULE);
    const params = new URLSearchParams();
    if (sourceRunId) params.set("sourceRunId", sourceRunId);
    params.set("take", String(take));

    const response = await api.get<BacktestRunSummaryView[]>(
      `/api/Trader/Backtest/GetList?${params}`,
      { withCredentials: true },
    );
    return response.data;
  },

  GetById: async (id: string): Promise<BacktestRunDetailView> => {
    if (USE_MOCK) {
      await mockDelay(150);
      throw new Error("Mock not implemented for detail");
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<BacktestRunDetailView>(
      `/api/Trader/Backtest/GetById/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },

  GetTrades: async (id: string): Promise<BacktestTradeView[]> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return [];
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<BacktestTradeView[]>(
      `/api/Trader/Backtest/GetTrades/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },

  GetEquityCurve: async (id: string): Promise<BacktestDailyEquityView[]> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return [];
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<BacktestDailyEquityView[]>(
      `/api/Trader/Backtest/GetEquityCurve/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },

  Compare: async (ids: string[]): Promise<BacktestRunSummaryView[]> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return [];
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<BacktestRunSummaryView[]>(
      "/api/Trader/Backtest/Compare",
      ids,
      { withCredentials: true },
    );
    return response.data;
  },
  
QueryEvents: async (
  query: BacktestEventQuery,
): Promise<PagedResult<BacktestEventView>> => {
  if (USE_MOCK) {
    await mockDelay(150);
    return {
      items: [],
      totalRecords: 0,
      page: 1,
      pageSize: 500,
      totalPages: 0,
    };
  }
  const api = getAPI(API_MODULE);
  const response = await api.post<PagedResult<BacktestEventView>>(
    "/api/Trader/Backtest/QueryEvents",
    query,
    { withCredentials: true },
  );
  return response.data;
},
};