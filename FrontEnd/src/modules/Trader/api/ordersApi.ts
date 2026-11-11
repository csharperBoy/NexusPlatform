import getAPI from "@/core/api/axiosClient";
import type {
  BrokerOrderDetailView,
  BrokerOrderQuery,
  BrokerOrderView,
  OrderIntentQuery,
  OrderIntentView,
} from "../models";
import { mockDelay } from "./_mockStorage";
import { PagedResult } from "../models/Orders";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";

export const brokerOrderApi = {
  Query: async (
    query: BrokerOrderQuery,
  ): Promise<PagedResult<BrokerOrderView>> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return { items: [], totalRecords: 0, page: 1, pageSize: query.take ?? 100 };
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<PagedResult<BrokerOrderView>>(
      "/api/Trader/BrokerOrder/Query",
      query,
      { withCredentials: true },
    );
    return response.data;
  },

  GetById: async (id: string): Promise<BrokerOrderDetailView> => {
    if (USE_MOCK) {
      await mockDelay(100);
      throw new Error("Mock not implemented");
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<BrokerOrderDetailView>(
      `/api/Trader/BrokerOrder/GetById/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },
};

export const orderIntentApi = {
  Query: async (
    query: OrderIntentQuery,
  ): Promise<PagedResult<OrderIntentView>> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return { items: [], totalRecords: 0, page: 1, pageSize: query.take ?? 100 };
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<PagedResult<OrderIntentView>>(
      "/api/Trader/OrderIntent/Query",
      query,
      { withCredentials: true },
    );
    return response.data;
  },

  GetById: async (id: string): Promise<OrderIntentView> => {
    if (USE_MOCK) {
      await mockDelay(100);
      throw new Error("Mock not implemented");
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<OrderIntentView>(
      `/api/Trader/OrderIntent/GetById/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },
};