import getAPI from "@/core/api/axiosClient";
import type {
  SymbolInfoView,
  MarketSymbolInfoView,
  CreateSymbolCommand,
  UpdateSymbolCommand,
} from "../models";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import {
  mockLoad,
  mockSave,
  mockDelay,
  mockUid,
} from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";
const MOCK_KEY = "mock:trader:symbols";

export const symbolApi = {
  GetList: async (): Promise<SymbolInfoView[]> => {
    if (USE_MOCK) {
      await mockDelay(200);
      return mockLoad<SymbolInfoView[]>(MOCK_KEY, []);
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<SymbolInfoView[]>(
      "/api/Trader/Symbol/GetList",
      { withCredentials: true },
    );
    return response.data;
  },

  GetSelectionList: async (): Promise<SelectionListDto[]> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return mockLoad<SymbolInfoView[]>(MOCK_KEY, []).map((s) => ({
        value: s.symbolIsin,
        label: s.symbolName,
        display: `${s.symbolName} — ${s.symbolIsin}`,
      }));
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/Trader/Symbol/GetSelectionList",
      { withCredentials: true },
    );
    return response.data;
  },

  create: async (data: CreateSymbolCommand): Promise<string> => {
    if (USE_MOCK) {
      await mockDelay(300);
      const items = mockLoad<SymbolInfoView[]>(MOCK_KEY, []);
      const sym: SymbolInfoView = { id: mockUid(), ...data };
      items.push(sym);
      mockSave(MOCK_KEY, items);
      return sym.id;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<string>(
      "/api/Trader/Symbol/create",
      data,
      { withCredentials: true },
    );
    return response.data;
  },

  update: async (data: UpdateSymbolCommand): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(300);
      const items = mockLoad<SymbolInfoView[]>(MOCK_KEY, []);
      const idx = items.findIndex((s) => s.id === data.id);
      if (idx < 0) throw new Error("نماد پیدا نشد");
      items[idx] = { ...items[idx], ...data };
      mockSave(MOCK_KEY, items);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.put<boolean>(
      `/api/Trader/Symbol/${data.id}`,
      data,
      { withCredentials: true },
    );
    return response.data;
  },

  delete: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(200);
      const items = mockLoad<SymbolInfoView[]>(MOCK_KEY, []);
      mockSave(
        MOCK_KEY,
        items.filter((s) => s.id !== id),
      );
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.delete<boolean>(
      `/api/Trader/Symbol/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },

  /* ─── قیمت لحظه‌ای از کارگزاری (backend proxy می‌کنه) ─── */
  getMarketInfo: async (symbolIsin: string): Promise<MarketSymbolInfoView> => {
    if (USE_MOCK) {
      await mockDelay(500);
      const base = Math.floor(1000 + Math.random() * 5000);
      return {
        symbolIsin,
        highAllowedPrice: Math.floor(base * 1.05),
        lowAllowedPrice: Math.floor(base * 0.95),
        lastTradedPrice: base,
        closingPrice: base - 10,
        tradeDate: new Date().toISOString(),
        fetchedAt: Date.now(),
      };
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<MarketSymbolInfoView>(
      "/api/Trader/Symbol/GetMarketInfo",
      { symbolIsin },
      { withCredentials: true },
    );
    return response.data;
  },
};