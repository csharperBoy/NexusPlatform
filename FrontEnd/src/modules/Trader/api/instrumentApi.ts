import getAPI from "@/core/api/axiosClient";
import type {
  InstrumentInfoView,
  CreateInstrumentCommand,
  UpdateInstrumentCommand,
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
const MOCK_KEY = "mock:trader:instruments";

interface MockInstrument {
  id: string;
  isin: string;
  symbolName: string;
  displayName: string;
  type: number;
  market: number;
  boardCode?: string | null;
  tradingScheduleId?: string | null;   // ← اضافه شود
  isActive: boolean;
  createdAt: string;
}

function toView(m: MockInstrument): InstrumentInfoView {
  return {
    id: m.id,
    isin: m.isin,
    symbolName: m.symbolName,
    displayName: m.displayName,
    type: m.type,
    typeName: typeNameOf(m.type),
    market: m.market,
    marketName: marketNameOf(m.market),
    boardCode: m.boardCode ?? null,
    tradingScheduleId: m.tradingScheduleId ?? null,       // ← جدید
    tradingScheduleCode: null,                             // mock
    tradingScheduleName: null,                             // mock
    isActive: m.isActive,
    createdAt: m.createdAt,
  };
}

function typeNameOf(type: number): string {
  switch (type) {
    case 0: return "سهام";
    case 1: return "اختیار معامله";
    case 2: return "آتی";
    case 3: return "اوراق";
    case 4: return "صندوق";
    case 5: return "ارز";
    case 6: return "رمزارز";
    default: return "نامشخص";
  }
}

function marketNameOf(market: number): string {
  switch (market) {
    case 0: return "بورس تهران";
    case 1: return "بورس آپشن";
    case 2: return "بورس آتی";
    case 10: return "فارکس";
    case 20: return "کریپتو";
    default: return "نامشخص";
  }
}

export const instrumentApi = {
  /* ─── لیست ─── */
  GetList: async (): Promise<InstrumentInfoView[]> => {
    if (USE_MOCK) {
      await mockDelay(200);
      return mockLoad<MockInstrument[]>(MOCK_KEY, []).map(toView);
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<InstrumentInfoView[]>(
      "/api/Trader/Instrument/GetList",
      { withCredentials: true },
    );
    return response.data;
  },

  /* ─── لیست انتخابی ─── */
  GetSelectionList: async (): Promise<SelectionListDto[]> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return mockLoad<MockInstrument[]>(MOCK_KEY, [])
        .filter((m) => m.isActive)
        .map((m) => ({
          value: m.isin,
          label: m.symbolName,
          display: m.displayName,
        }));
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/Trader/Instrument/GetSelectionList",
      { withCredentials: true },
    );
    return response.data;
  },

  /* ─── ایجاد ─── */
  create: async (data: CreateInstrumentCommand): Promise<string> => {
    if (USE_MOCK) {
      await mockDelay(300);
      const items = mockLoad<MockInstrument[]>(MOCK_KEY, []);
      const inst: MockInstrument = {
        id: mockUid(),
        isin: data.isin,
        symbolName: data.symbolName,
        displayName: data.displayName,
        type: data.type,
        market: data.market,
        boardCode: data.boardCode ?? null,
        isActive: true,
        createdAt: new Date().toISOString(),
      };
      items.push(inst);
      mockSave(MOCK_KEY, items);
      return inst.id;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<string>(
      "/api/Trader/Instrument/Create",
      data,
      { withCredentials: true },
    );
    return response.data;
  },

  /* ─── ویرایش ─── */
  update: async (data: UpdateInstrumentCommand): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(300);
      const items = mockLoad<MockInstrument[]>(MOCK_KEY, []);
      const idx = items.findIndex((i) => i.id === data.id);
      if (idx < 0) throw new Error("نماد پیدا نشد");
      items[idx] = {
        ...items[idx],
        symbolName: data.symbolName,
        displayName: data.displayName,
        boardCode: data.boardCode ?? null,
      };
      mockSave(MOCK_KEY, items);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.put<boolean>(
      `/api/Trader/Instrument/${data.id}`,
      data,
      { withCredentials: true },
    );
    return response.data;
  },

  /* ─── غیرفعال‌سازی (soft delete) ─── */
  delete: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(200);
      const items = mockLoad<MockInstrument[]>(MOCK_KEY, []);
      const idx = items.findIndex((i) => i.id === id);
      if (idx >= 0) {
        items[idx].isActive = false;
        mockSave(MOCK_KEY, items);
      }
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.delete<boolean>(
      `/api/Trader/Instrument/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },
};