import getAPI from "@/core/api/axiosClient";
import type {
  CreateHistoricalBackfillCommand,
  CreateHistoricalRecurringCommand,
  HistoricalDataCollectionJobSummaryView,
  HistoricalDataCollectionJobDetailView,
} from "../models";
import { mockDelay } from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";

export const historicalDataCollectionApi = {
  /* ═══════════════ Commands ═══════════════ */

  CreateBackfill: async (
    cmd: CreateHistoricalBackfillCommand,
  ): Promise<string> => {
    if (USE_MOCK) {
      await mockDelay(300);
      return crypto.randomUUID();
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<string>(
      "/api/Trader/HistoricalDataCollection/Backfill",
      cmd,
      { withCredentials: true },
    );
    return response.data;
  },

  CreateRecurring: async (
    cmd: CreateHistoricalRecurringCommand,
  ): Promise<string> => {
    if (USE_MOCK) {
      await mockDelay(300);
      return crypto.randomUUID();
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<string>(
      "/api/Trader/HistoricalDataCollection/Recurring",
      cmd,
      { withCredentials: true },
    );
    return response.data;
  },

  Start: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(200);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<boolean>(
      `/api/Trader/HistoricalDataCollection/Start/${id}`,
      {},
      { withCredentials: true },
    );
    return response.data;
  },

  Stop: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(200);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<boolean>(
      `/api/Trader/HistoricalDataCollection/Stop/${id}`,
      {},
      { withCredentials: true },
    );
    return response.data;
  },

  /* ═══════════════ Queries ═══════════════ */

  GetList: async (
    take = 50,
  ): Promise<HistoricalDataCollectionJobSummaryView[]> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return [];
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<HistoricalDataCollectionJobSummaryView[]>(
      `/api/Trader/HistoricalDataCollection/GetList?take=${take}`,
      { withCredentials: true },
    );
    return response.data;
  },

  GetById: async (
    id: string,
  ): Promise<HistoricalDataCollectionJobDetailView> => {
    if (USE_MOCK) {
      await mockDelay(150);
      throw new Error("Mock not implemented for detail");
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<HistoricalDataCollectionJobDetailView>(
      `/api/Trader/HistoricalDataCollection/GetById/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },
};