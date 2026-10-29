import getAPI from "@/core/api/axiosClient";
import type {
  TradingRunInfoView,
  TradingRunDetailView,
  CreateTradingRunCommand,
  UpdateTradingRunCommand,
} from "../models";
import {
  mockLoad,
  mockSave,
  mockDelay,
  mockUid,
} from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";
const MOCK_KEY = "mock:trader:trading-runs";

export const tradingRunApi = {
  /* ─── لیست ─── */
  GetList: async (): Promise<TradingRunInfoView[]> => {
    if (USE_MOCK) {
      await mockDelay(200);
      return mockLoad<TradingRunInfoView[]>(MOCK_KEY, []);
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<TradingRunInfoView[]>(
      "/api/Trader/TradingRun/GetList",
      { withCredentials: true },
    );
    return response.data;
  },

  /* ─── جزئیات ─── */
  GetById: async (id: string): Promise<TradingRunDetailView> => {
    if (USE_MOCK) {
      await mockDelay(150);
      const list = mockLoad<TradingRunInfoView[]>(MOCK_KEY, []);
      const item = list.find((r) => r.id === id);
      if (!item) throw new Error("پلن پیدا نشد");
      // جزئیات ساده در mock
      return {
        ...item,
        strategyConfigJson: "{}",
        settings: {
          capitalAllocationMode: item.capitalAllocationMode,
          fixedCapitalAmount: item.fixedCapital,
          stopLossPercentage: null,
          stopLossPrice: null,
          takeProfitPercentage: null,
          takeProfitPrice: null,
          defaultOrderMode: 0,
          onMarketClose: 0,
          onStop: 0,
        },
        symbols: [],
        accountIds: [],
        modifiedAt: null,
      };
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<TradingRunDetailView>(
      `/api/Trader/TradingRun/GetById/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },

  /* ─── ایجاد ─── */
  create: async (data: CreateTradingRunCommand): Promise<string> => {
  if (USE_MOCK) {
    await mockDelay(300);
    const items = mockLoad<TradingRunInfoView[]>(MOCK_KEY, []);
    const id = mockUid();
    items.push({
      id,
      name: data.name,
      state: 1,
      stateName: "Ready",
      strategyKey: data.strategyKey,
      primaryTimeFrame: data.primaryTimeFrame,
      primaryTimeFrameName: "1d",
      capitalAllocationMode: data.settings.capitalAllocationMode,
      fixedCapital: data.settings.fixedCapitalAmount ?? null,
      targetCurrency: data.targetCurrency ?? null,
      strengthMultiplier: data.strengthMultiplier,
      symbolCount: data.symbols.length,
      accountCount: data.accountIds.length,
      startedAt: null,
      stoppedAt: null,
      createdAt: new Date().toISOString(),
    });
    mockSave(MOCK_KEY, items);
    return id;
  }
  const api = getAPI(API_MODULE);
  const response = await api.post<string>(
    "/api/Trader/TradingRun/Create",
    data,
    { withCredentials: true },
  );
  return response.data;
},

  /* ─── ویرایش ─── */
  update: async (data: UpdateTradingRunCommand): Promise<boolean> => {
  if (USE_MOCK) {
    await mockDelay(300);
    const items = mockLoad<TradingRunInfoView[]>(MOCK_KEY, []);
    const idx = items.findIndex((r) => r.id === data.id);
    if (idx < 0) throw new Error("پلن پیدا نشد");
    items[idx] = {
      ...items[idx],
      name: data.name,
      strategyKey: data.strategyKey,
      primaryTimeFrame: data.primaryTimeFrame,
      capitalAllocationMode: data.settings.capitalAllocationMode,
      fixedCapital: data.settings.fixedCapitalAmount ?? null,
      targetCurrency: data.targetCurrency ?? null,
      strengthMultiplier: data.strengthMultiplier,
      symbolCount: data.symbols.length,
      accountCount: data.accountIds.length,
    };
    mockSave(MOCK_KEY, items);
    return true;
  }
  const api = getAPI(API_MODULE);
  const response = await api.put<boolean>(
    `/api/Trader/TradingRun/${data.id}`,
    data,
    { withCredentials: true },
  );
  return response.data;
},

  /* ─── حذف ─── */
  delete: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(200);
      const items = mockLoad<TradingRunInfoView[]>(MOCK_KEY, []);
      mockSave(MOCK_KEY, items.filter((r) => r.id !== id));
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.delete<boolean>(
      `/api/Trader/TradingRun/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },

  /* ─── Start ─── */
  start: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(500);
      const items = mockLoad<TradingRunInfoView[]>(MOCK_KEY, []);
      const idx = items.findIndex((r) => r.id === id);
      if (idx < 0) throw new Error("پلن پیدا نشد");
      items[idx].state = 2;
      items[idx].stateName = "Running";
      items[idx].startedAt = new Date().toISOString();
      mockSave(MOCK_KEY, items);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<boolean>(
      `/api/Trader/TradingRun/Start/${id}`,
      {},
      { withCredentials: true },
    );
    return response.data;
  },

  /* ─── Stop ─── */
  stop: async (id: string, behavior: number): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(500);
      const items = mockLoad<TradingRunInfoView[]>(MOCK_KEY, []);
      const idx = items.findIndex((r) => r.id === id);
      if (idx < 0) throw new Error("پلن پیدا نشد");
      items[idx].state = 3;
      items[idx].stateName = "Stopped";
      items[idx].stoppedAt = new Date().toISOString();
      mockSave(MOCK_KEY, items);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<boolean>(
      `/api/Trader/TradingRun/Stop/${id}?behavior=${behavior}`,
      {},
      { withCredentials: true },
    );
    return response.data;
  },
};