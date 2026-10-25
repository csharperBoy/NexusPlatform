import getAPI from "@/core/api/axiosClient";
import type { CurrencyRateView, CreateCurrencyRateCommand } from "../models";
import {
  mockLoad,
  mockSave,
  mockDelay,
  mockUid,
} from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";
const MOCK_KEY = "mock:trader:currency-rates";

export const currencyRateApi = {
  GetList: async (
    fromCurrency?: string,
    toCurrency?: string,
    limit = 100,
  ): Promise<CurrencyRateView[]> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return mockLoad<CurrencyRateView[]>(MOCK_KEY, [])
        .filter((r) => !fromCurrency || r.fromCurrency === fromCurrency)
        .filter((r) => !toCurrency || r.toCurrency === toCurrency)
        .slice(0, limit);
    }
    const api = getAPI(API_MODULE);
    const params = new URLSearchParams();
    if (fromCurrency) params.set("fromCurrency", fromCurrency);
    if (toCurrency) params.set("toCurrency", toCurrency);
    params.set("limit", String(limit));

    const response = await api.get<CurrencyRateView[]>(
      `/api/Trader/CurrencyRate/GetList?${params}`,
      { withCredentials: true },
    );
    return response.data;
  },

  GetLatest: async (
    fromCurrency: string,
    toCurrency: string,
    atTime?: string,
  ): Promise<number | null> => {
    if (USE_MOCK) {
      await mockDelay(100);
      const list = mockLoad<CurrencyRateView[]>(MOCK_KEY, [])
        .filter((r) => r.fromCurrency === fromCurrency && r.toCurrency === toCurrency)
        .sort((a, b) => b.effectiveAt.localeCompare(a.effectiveAt));
      return list[0]?.rate ?? null;
    }
    const api = getAPI(API_MODULE);
    const params = new URLSearchParams({ fromCurrency, toCurrency });
    if (atTime) params.set("atTime", atTime);

    const response = await api.get<number | null>(
      `/api/Trader/CurrencyRate/GetLatest?${params}`,
      { withCredentials: true },
    );
    return response.data;
  },

  create: async (data: CreateCurrencyRateCommand): Promise<string> => {
    if (USE_MOCK) {
      await mockDelay(300);
      const list = mockLoad<CurrencyRateView[]>(MOCK_KEY, []);
      const item: CurrencyRateView = {
        id: mockUid(),
        fromCurrency: data.fromCurrency.toUpperCase(),
        toCurrency: data.toCurrency.toUpperCase(),
        rate: data.rate,
        effectiveAt: data.effectiveAt,
        source: data.source ?? "Manual",
        createdAt: new Date().toISOString(),
      };
      list.push(item);
      mockSave(MOCK_KEY, list);
      return item.id;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<string>(
      "/api/Trader/CurrencyRate/Create",
      data,
      { withCredentials: true },
    );
    return response.data;
  },
};