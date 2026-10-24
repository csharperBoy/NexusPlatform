import getAPI from "@/core/api/axiosClient";
import type { TradingScheduleInfoView } from "../models";
import type { SelectionListDto } from "@/core/models/SelectionListDto";
import { mockDelay } from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";

const MOCK_SCHEDULES: TradingScheduleInfoView[] = [
  {
    id: "11111111-1111-1111-1111-111111111111",
    code: "TSE-REGULAR",
    name: "بورس تهران — سهام عادی",
    description: "سهام بورس تهران، فرابورس و صندوق‌های قابل معامله",
    preMarketStart: "08:45",
    sessionStart: "09:00",
    sessionEnd: "12:30",
    isActive: true,
  },
  {
    id: "22222222-2222-2222-2222-222222222222",
    code: "TSE-OPTIONS",
    name: "بورس تهران — اختیار معامله",
    description: "قراردادهای اختیار معامله",
    preMarketStart: "08:45",
    sessionStart: "09:00",
    sessionEnd: "15:00",
    isActive: true,
  },
  {
    id: "33333333-3333-3333-3333-333333333333",
    code: "TSE-LEVERAGED",
    name: "صندوق‌های اهرمی",
    preMarketStart: "08:45",
    sessionStart: "09:00",
    sessionEnd: "12:30",
    isActive: true,
  },
  {
    id: "44444444-4444-4444-4444-444444444444",
    code: "TSE-AYAR",
    name: "گواهی عیار / صیاد",
    preMarketStart: "12:25",
    sessionStart: "12:30",
    sessionEnd: "18:00",
    isActive: true,
  },
];

export const tradingScheduleApi = {
  GetList: async (): Promise<TradingScheduleInfoView[]> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return MOCK_SCHEDULES;
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<TradingScheduleInfoView[]>(
      "/api/Trader/TradingSchedule/GetList",
      { withCredentials: true },
    );
    return response.data;
  },

  GetSelectionList: async (): Promise<SelectionListDto[]> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return MOCK_SCHEDULES.filter((s) => s.isActive).map((s) => ({
        value: s.id,
        label: s.name,
        display: `${s.name} (${s.sessionStart}–${s.sessionEnd})`,
      }));
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/Trader/TradingSchedule/GetSelectionList",
      { withCredentials: true },
    );
    return response.data;
  },
};