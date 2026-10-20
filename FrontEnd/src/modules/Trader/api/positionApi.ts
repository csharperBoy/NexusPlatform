import getAPI from "@/core/api/axiosClient";
import type { PositionView } from "../models";
import { mockLoad, mockDelay } from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";
const MOCK_KEY = "mock:trader:positions";

export const positionApi = {
  GetByRun: async (runId: string): Promise<PositionView[]> => {
    if (USE_MOCK) {
      await mockDelay(200);
      return mockLoad<PositionView[]>(MOCK_KEY, []).filter(
        (p) => p.runId === runId,
      );
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<PositionView[]>(
      `/api/Trader/Position/GetByRun/${runId}`,
      { withCredentials: true },
    );
    return response.data;
  },

  GetByAccount: async (accountId: string): Promise<PositionView[]> => {
    if (USE_MOCK) {
      await mockDelay(200);
      return mockLoad<PositionView[]>(MOCK_KEY, []).filter(
        (p) => p.traderAccountId === accountId,
      );
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<PositionView[]>(
      `/api/Trader/Position/GetByAccount/${accountId}`,
      { withCredentials: true },
    );
    return response.data;
  },
};