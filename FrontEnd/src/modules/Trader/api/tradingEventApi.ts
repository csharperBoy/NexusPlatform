import getAPI from "@/core/api/axiosClient";
import type { TradingEventView, TradingEventQuery } from "../models";
import { mockDelay } from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";

/**
 * آینه‌ی PagedResult<T> در Backend.
 * نام‌ها با backend camelCase مطابق است.
 */
export interface PagedResult<T> {
  items: T[];
  totalRecords: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export const tradingEventApi = {
  Query: async (
    filter: TradingEventQuery,
  ): Promise<PagedResult<TradingEventView>> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return {
        items: [],
        totalRecords: 0,
        page: 1,
        pageSize: 100,
        totalPages: 0,
      };
    }

    const api = getAPI(API_MODULE);
    const response = await api.post<PagedResult<TradingEventView>>(
      "/api/Trader/TradingEvent/Query",
      filter,
      { withCredentials: true },
    );
    return response.data;
  },
};