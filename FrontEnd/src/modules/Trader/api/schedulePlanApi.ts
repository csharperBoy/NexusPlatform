import getAPI from "@/core/api/axiosClient";
import type {
  SchedulePlanInfoView,
  CreateSchedulePlanCommand,
  UpdateSchedulePlanCommand,
  ScheduledOrderInfoView,
} from "../models";
import {
  mockLoad,
  mockSave,
  mockDelay,
  mockUid,
} from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";
const MOCK_KEY = "mock:trader:plans";

export const schedulePlanApi = {
  GetList: async (): Promise<SchedulePlanInfoView[]> => {
    if (USE_MOCK) {
      await mockDelay(200);
      return mockLoad<SchedulePlanInfoView[]>(MOCK_KEY, []);
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<SchedulePlanInfoView[]>(
      "/api/Trader/SchedulePlan/GetList",
      { withCredentials: true },
    );
    return response.data;
  },

  GetById: async (id: string): Promise<SchedulePlanInfoView> => {
    if (USE_MOCK) {
      await mockDelay(150);
      const items = mockLoad<SchedulePlanInfoView[]>(MOCK_KEY, []);
      const plan = items.find((p) => p.id === id);
      if (!plan) throw new Error("پلن پیدا نشد");
      return plan;
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<SchedulePlanInfoView>(
      `/api/Trader/SchedulePlan/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },

  create: async (data: CreateSchedulePlanCommand): Promise<string> => {
    if (USE_MOCK) {
      await mockDelay(400);
      const items = mockLoad<SchedulePlanInfoView[]>(MOCK_KEY, []);
      const plan: SchedulePlanInfoView = {
        id: mockUid(),
        name: data.name,
        date: data.date,
        enabled: data.enabled,
        autoLoginAt: data.autoLoginAt,
        autoRefreshAt: data.autoRefreshAt,
        orders: data.orders.map<ScheduledOrderInfoView>((o) => ({
          id: mockUid(),
          ...o,
          fired: false,
        })),
        status: "idle",
        message: "",
      };
      items.push(plan);
      mockSave(MOCK_KEY, items);
      return plan.id;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<string>(
      "/api/Trader/SchedulePlan/create",
      data,
      { withCredentials: true },
    );
    return response.data;
  },

  update: async (data: UpdateSchedulePlanCommand): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(300);
      const items = mockLoad<SchedulePlanInfoView[]>(MOCK_KEY, []);
      const idx = items.findIndex((p) => p.id === data.id);
      if (idx < 0) throw new Error("پلن پیدا نشد");
      const existing = items[idx];
      items[idx] = {
        ...existing,
        name: data.name,
        date: data.date,
        enabled: data.enabled,
        autoLoginAt: data.autoLoginAt,
        autoRefreshAt: data.autoRefreshAt,
        orders: data.orders.map<ScheduledOrderInfoView>((o, i) => ({
          id: existing.orders[i]?.id ?? mockUid(),
          ...o,
          fired: existing.orders[i]?.fired ?? false,
        })),
      };
      mockSave(MOCK_KEY, items);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.put<boolean>(
      `/api/Trader/SchedulePlan/${data.id}`,
      data,
      { withCredentials: true },
    );
    return response.data;
  },

  delete: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(200);
      const items = mockLoad<SchedulePlanInfoView[]>(MOCK_KEY, []);
      mockSave(
        MOCK_KEY,
        items.filter((p) => p.id !== id),
      );
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.delete<boolean>(
      `/api/Trader/SchedulePlan/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },

  enable: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(200);
      const items = mockLoad<SchedulePlanInfoView[]>(MOCK_KEY, []);
      const idx = items.findIndex((p) => p.id === id);
      if (idx < 0) throw new Error("پلن پیدا نشد");
      items[idx].enabled = true;
      mockSave(MOCK_KEY, items);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<boolean>(
      "/api/Trader/SchedulePlan/enable",
      { id },
      { withCredentials: true },
    );
    return response.data;
  },

  disable: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(200);
      const items = mockLoad<SchedulePlanInfoView[]>(MOCK_KEY, []);
      const idx = items.findIndex((p) => p.id === id);
      if (idx < 0) throw new Error("پلن پیدا نشد");
      items[idx].enabled = false;
      mockSave(MOCK_KEY, items);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<boolean>(
      "/api/Trader/SchedulePlan/disable",
      { id },
      { withCredentials: true },
    );
    return response.data;
  },
};