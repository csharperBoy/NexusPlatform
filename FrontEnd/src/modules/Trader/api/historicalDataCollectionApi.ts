import getAPI from "@/core/api/axiosClient";
import type {
  CollectHistoricalDataCommand,
  HistoricalDataCollectionJobSummaryView,
  HistoricalDataCollectionJobDetailView,
} from "../models";
import { mockDelay } from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";

export const historicalDataCollectionApi = {
  Collect: async (cmd: CollectHistoricalDataCommand): Promise<string> => {
    if (USE_MOCK) {
      await mockDelay(300);
      return crypto.randomUUID();
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<string>(
      "/api/Trader/HistoricalDataCollection/Collect",
      cmd,
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
      `/api/Trader/HistoricalDataCollection/Cancel/${id}`,
      {},
      { withCredentials: true },
    );
    return response.data;
  },

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