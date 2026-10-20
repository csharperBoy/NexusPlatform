import getAPI from "@/core/api/axiosClient";
import type { SignalView } from "../models";
import { mockLoad, mockDelay } from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";
const MOCK_KEY = "mock:trader:signals";

export const signalApi = {
  GetRecentByRun: async (
    runId: string,
    take: number = 100,
  ): Promise<SignalView[]> => {
    if (USE_MOCK) {
      await mockDelay(200);
      return mockLoad<SignalView[]>(MOCK_KEY, [])
        .filter((s) => s.runId === runId)
        .slice(0, take);
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<SignalView[]>(
      `/api/Trader/Signal/GetRecentByRun/${runId}?take=${take}`,
      { withCredentials: true },
    );
    return response.data;
  },
};