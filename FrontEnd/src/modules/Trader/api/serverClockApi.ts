import getAPI from "@/core/api/axiosClient";
import type { ServerClockInfoView } from "../models";
import { mockLoad, mockSave, mockDelay } from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";
const MOCK_KEY = "mock:trader:clock";

const EMPTY: ServerClockInfoView = {
  diff: 0,
  offset: 0,
  oneWayLatency: 0,
  lastUpdatedAt: 0,
  samplesCount: 0,
  lastError: null,
};

export const serverClockApi = {
  GetStatus: async (): Promise<ServerClockInfoView> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return mockLoad<ServerClockInfoView>(MOCK_KEY, EMPTY);
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<ServerClockInfoView>(
      "/api/Trader/ServerClock/GetStatus",
      { withCredentials: true },
    );
    return response.data;
  },

  Sync: async (samples = 5): Promise<ServerClockInfoView> => {
    if (USE_MOCK) {
      await mockDelay(600);
      const now = Date.now();
      const rtt = 80 + Math.random() * 60;
      const offset = -50 + Math.random() * 100;
      const info: ServerClockInfoView = {
        diff: Math.round(rtt / 2 + offset),
        offset: Math.round(offset),
        oneWayLatency: Math.round(rtt / 2),
        lastUpdatedAt: now,
        samplesCount: samples,
        lastError: null,
      };
      mockSave(MOCK_KEY, info);
      return info;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<ServerClockInfoView>(
      "/api/Trader/ServerClock/Sync",
      { samples },
      { withCredentials: true },
    );
    return response.data;
  },
};