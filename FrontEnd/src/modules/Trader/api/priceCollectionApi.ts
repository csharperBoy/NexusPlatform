import getAPI from "@/core/api/axiosClient";
import type {
  PriceSourceView,
  CreatePriceSourceCommand,
  UpdatePriceSourceCommand,
  PriceCollectionJobView,
  CreatePriceCollectionJobCommand,
} from "../models";
import { mockLoad, mockSave, mockDelay, mockUid } from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";
const MOCK_SOURCES = "mock:trader:price-sources";
const MOCK_JOBS = "mock:trader:price-jobs";

/* ═══════════════ PriceSource ═══════════════ */

export const priceSourceApi = {
  GetList: async (): Promise<PriceSourceView[]> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return mockLoad<PriceSourceView[]>(MOCK_SOURCES, []);
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<PriceSourceView[]>(
      "/api/Trader/PriceSource/GetList",
      { withCredentials: true },
    );
    return response.data;
  },

  GetById: async (id: string): Promise<PriceSourceView> => {
    if (USE_MOCK) {
      await mockDelay(100);
      const list = mockLoad<PriceSourceView[]>(MOCK_SOURCES, []);
      const item = list.find((s) => s.id === id);
      if (!item) throw new Error("منبع پیدا نشد");
      return item;
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<PriceSourceView>(
      `/api/Trader/PriceSource/GetById/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },

  create: async (data: CreatePriceSourceCommand): Promise<string> => {
    if (USE_MOCK) {
      await mockDelay(300);
      const list = mockLoad<PriceSourceView[]>(MOCK_SOURCES, []);
      const item: PriceSourceView = {
        id: mockUid(),
        code: data.code.toLowerCase(),
        name: data.name,
        type: data.type,
        typeName: "TGJU",
        configJson: data.configJson,
        fromAsset: data.fromAsset.toUpperCase(),
        toAsset: data.toAsset.toUpperCase(),
        isActive: true,
        createdAt: new Date().toISOString(),
      };
      list.push(item);
      mockSave(MOCK_SOURCES, list);
      return item.id;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<string>(
      "/api/Trader/PriceSource/Create",
      data,
      { withCredentials: true },
    );
    return response.data;
  },

  update: async (data: UpdatePriceSourceCommand): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(300);
      const list = mockLoad<PriceSourceView[]>(MOCK_SOURCES, []);
      const idx = list.findIndex((s) => s.id === data.id);
      if (idx < 0) throw new Error("منبع پیدا نشد");
      list[idx] = {
        ...list[idx],
        name: data.name,
        configJson: data.configJson,
        fromAsset: data.fromAsset.toUpperCase(),
        toAsset: data.toAsset.toUpperCase(),
      };
      mockSave(MOCK_SOURCES, list);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.put<boolean>(
      `/api/Trader/PriceSource/${data.id}`,
      data,
      { withCredentials: true },
    );
    return response.data;
  },

  delete: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(200);
      const list = mockLoad<PriceSourceView[]>(MOCK_SOURCES, []);
      const idx = list.findIndex((s) => s.id === id);
      if (idx >= 0) {
        list[idx] = { ...list[idx], isActive: false };
        mockSave(MOCK_SOURCES, list);
      }
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.delete<boolean>(
      `/api/Trader/PriceSource/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },
};

/* ═══════════════ PriceCollectionJob ═══════════════ */

export const priceCollectionJobApi = {
  GetList: async (priceSourceId?: string): Promise<PriceCollectionJobView[]> => {
    if (USE_MOCK) {
      await mockDelay(150);
      const list = mockLoad<PriceCollectionJobView[]>(MOCK_JOBS, []);
      return priceSourceId
        ? list.filter((j) => j.priceSourceId === priceSourceId)
        : list;
    }
    const api = getAPI(API_MODULE);
    const params = new URLSearchParams();
    if (priceSourceId) params.set("priceSourceId", priceSourceId);
    const qs = params.toString();
    const response = await api.get<PriceCollectionJobView[]>(
      `/api/Trader/PriceCollectionJob/GetList${qs ? "?" + qs : ""}`,
      { withCredentials: true },
    );
    return response.data;
  },

  GetById: async (id: string): Promise<PriceCollectionJobView> => {
    if (USE_MOCK) {
      await mockDelay(100);
      const list = mockLoad<PriceCollectionJobView[]>(MOCK_JOBS, []);
      const item = list.find((j) => j.id === id);
      if (!item) throw new Error("Job پیدا نشد");
      return item;
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<PriceCollectionJobView>(
      `/api/Trader/PriceCollectionJob/GetById/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },

  create: async (data: CreatePriceCollectionJobCommand): Promise<string> => {
    if (USE_MOCK) {
      await mockDelay(300);
      const jobs = mockLoad<PriceCollectionJobView[]>(MOCK_JOBS, []);
      const sources = mockLoad<PriceSourceView[]>(MOCK_SOURCES, []);
      const source = sources.find((s) => s.id === data.priceSourceId);

      const item: PriceCollectionJobView = {
        id: mockUid(),
        priceSourceId: data.priceSourceId,
        priceSourceCode: source?.code ?? "—",
        priceSourceName: source?.name ?? "—",
        mode: data.mode,
        modeName: data.mode === 1 ? "Backfill" : "Recurring",
        state: 0,
        stateName: "ایجاد شده",
        recurrencePatternJson: data.recurrencePatternJson ?? null,
        schedulerRecurringId: null,
        schedulerJobId: null,
        startedAt: null,
        stoppedAt: null,
        lastRunAt: null,
        lastRunInserted: 0,
        lastRunSkipped: 0,
        totalInserted: 0,
        backfillProgressPercent: null,
        lastError: null,
        createdAt: new Date().toISOString(),
      };
      jobs.push(item);
      mockSave(MOCK_JOBS, jobs);
      return item.id;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<string>(
      "/api/Trader/PriceCollectionJob/Create",
      data,
      { withCredentials: true },
    );
    return response.data;
  },

  start: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(400);
      const list = mockLoad<PriceCollectionJobView[]>(MOCK_JOBS, []);
      const idx = list.findIndex((j) => j.id === id);
      if (idx < 0) throw new Error("Job پیدا نشد");
      list[idx] = {
        ...list[idx],
        state: 1,
        stateName: "در حال اجرا",
        startedAt: new Date().toISOString(),
        schedulerJobId: mockUid(),
      };
      mockSave(MOCK_JOBS, list);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<boolean>(
      `/api/Trader/PriceCollectionJob/Start/${id}`,
      {},
      { withCredentials: true },
    );
    return response.data;
  },

  stop: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(300);
      const list = mockLoad<PriceCollectionJobView[]>(MOCK_JOBS, []);
      const idx = list.findIndex((j) => j.id === id);
      if (idx < 0) throw new Error("Job پیدا نشد");
      list[idx] = {
        ...list[idx],
        state: 2,
        stateName: "متوقف",
        stoppedAt: new Date().toISOString(),
        schedulerRecurringId: null,
        schedulerJobId: null,
      };
      mockSave(MOCK_JOBS, list);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<boolean>(
      `/api/Trader/PriceCollectionJob/Stop/${id}`,
      {},
      { withCredentials: true },
    );
    return response.data;
  },
};