import getAPI from "@/core/api/axiosClient";
import type { StrategyInfoView } from "../models";
import {
  mockLoad,
  mockSave,
  mockDelay,
} from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";
const MOCK_KEY = "mock:trader:strategies";

const MOCK_STRATEGIES: StrategyInfoView[] = [
  {
    key: "rsi",
    displayName: "RSI",
    configTypeName: "RsiStrategyConfig",
    configFields: [
      { name: "Period", typeName: "Int32", isRequired: false, defaultValue: "14" },
      { name: "OversoldThreshold", typeName: "Decimal", isRequired: false, defaultValue: "30" },
      { name: "OverboughtThreshold", typeName: "Decimal", isRequired: false, defaultValue: "70" },
      { name: "MinSignalStrength", typeName: "Decimal", isRequired: false, defaultValue: "0.6" },
    ],
  },
];

export const strategyApi = {
  GetList: async (): Promise<StrategyInfoView[]> => {
    if (USE_MOCK) {
      await mockDelay(150);
      const stored = mockLoad<StrategyInfoView[] | null>(MOCK_KEY, null);
      if (!stored) {
        mockSave(MOCK_KEY, MOCK_STRATEGIES);
        return MOCK_STRATEGIES;
      }
      return stored;
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<StrategyInfoView[]>(
      "/api/Trader/Strategy/GetList",
      { withCredentials: true },
    );
    return response.data;
  },
};