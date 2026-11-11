import getAPI from "@/core/api/axiosClient";
import type {
  OrderPresetView,
  CreateOrderPresetCommand,
  UpdateOrderPresetCommand,
} from "../models";
import { mockDelay } from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";

export const orderPresetApi = {
  GetList: async (): Promise<OrderPresetView[]> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return [];
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<OrderPresetView[]>(
      "/api/Trader/InstrumentOrderPreset/GetList",
      { withCredentials: true },
    );
    return response.data;
  },

  GetById: async (id: string): Promise<OrderPresetView> => {
    if (USE_MOCK) {
      await mockDelay(100);
      throw new Error("Mock not implemented");
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<OrderPresetView>(
      `/api/Trader/InstrumentOrderPreset/GetById/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },

  GetByInstrument: async (isin: string): Promise<OrderPresetView | null> => {
    if (USE_MOCK) {
      await mockDelay(100);
      return null;
    }
    const api = getAPI(API_MODULE);
    try {
      const response = await api.get<OrderPresetView>(
        `/api/Trader/InstrumentOrderPreset/GetByInstrument/${isin}`,
        { withCredentials: true },
      );
      return response.data;
    } catch {
      return null;
    }
  },

  create: async (data: CreateOrderPresetCommand): Promise<string> => {
    if (USE_MOCK) {
      await mockDelay(300);
      return crypto.randomUUID();
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<string>(
      "/api/Trader/InstrumentOrderPreset/Create",
      data,
      { withCredentials: true },
    );
    return response.data;
  },

  update: async (data: UpdateOrderPresetCommand): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(300);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.put<boolean>(
      `/api/Trader/InstrumentOrderPreset/${data.id}`,
      {
        name: data.name,
        defaultPrice: data.defaultPrice,
        defaultQuantity: data.defaultQuantity,
        defaultSide: data.defaultSide,
        validityType: data.validityType,
        commission: data.commission,
        defaultOrderMode: data.defaultOrderMode,
      },
      { withCredentials: true },
    );
    return response.data;
  },

  delete: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(200);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.delete<boolean>(
      `/api/Trader/InstrumentOrderPreset/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },
};