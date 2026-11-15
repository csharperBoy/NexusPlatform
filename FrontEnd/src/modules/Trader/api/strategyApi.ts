//src/modules/Trader/api/strategyApi.ts
import getAPI from "@/core/api/axiosClient";
import type { StrategyInfoView } from "../models";
import { mockLoad, mockSave, mockDelay } from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";

/* ═══════════════════════════════════════════════════════════════
   MOCK_VERSION: هر بار که MOCK_STRATEGIES رو تغییر می‌دی، این عدد
   رو زیاد کن. این کار localStorage قدیمی رو invalidate می‌کنه و
   مقادیر stale (از session قبلی) جایگزین می‌شن.
   ═══════════════════════════════════════════════════════════════ */
const MOCK_VERSION = 2;
const MOCK_KEY = `mock:trader:strategies:v${MOCK_VERSION}`;

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

  /* ─── mtf-composite ─── */
  {
    key: "mtf-composite",
    displayName: "Multi-TimeFrame Composite",
    configTypeName: "MtfCompositeStrategyConfig",
    configFields: [
      // Trend Layer
      { name: "EmaFastPeriod", typeName: "Int32", isRequired: false, defaultValue: "20" },
      { name: "EmaSlowPeriod", typeName: "Int32", isRequired: false, defaultValue: "50" },
      { name: "TrendTimeFrame", typeName: "TimeFrame", isRequired: false, defaultValue: "D1" },

      // Momentum Layer
      { name: "RsiPeriod", typeName: "Int32", isRequired: false, defaultValue: "14" },
      { name: "RsiBullishThreshold", typeName: "Decimal", isRequired: false, defaultValue: "55" },
      { name: "RsiBearishThreshold", typeName: "Decimal", isRequired: false, defaultValue: "45" },
      { name: "MacdFast", typeName: "Int32", isRequired: false, defaultValue: "12" },
      { name: "MacdSlow", typeName: "Int32", isRequired: false, defaultValue: "26" },
      { name: "MacdSignal", typeName: "Int32", isRequired: false, defaultValue: "9" },

      // Volatility
      { name: "AtrPeriod", typeName: "Int32", isRequired: false, defaultValue: "14" },

      // Baselines
      { name: "IntraDayBaselineCandles", typeName: "Int32", isRequired: false, defaultValue: "100" },
      { name: "OpeningBaselineDays", typeName: "Int32", isRequired: false, defaultValue: "30" },
      { name: "MinBaselineSamples", typeName: "Int32", isRequired: false, defaultValue: "10" },
      { name: "VolumeMultiplier", typeName: "Decimal", isRequired: false, defaultValue: "1.5" },

      // Order Book
      { name: "BuyRatioThreshold", typeName: "Decimal", isRequired: false, defaultValue: "1.1" },
      { name: "SellRatioThreshold", typeName: "Decimal", isRequired: false, defaultValue: "1.1" },
      { name: "OrderBookDepthRatio", typeName: "Decimal", isRequired: false, defaultValue: "1.5" },

      // Weights
      { name: "TrendWeight", typeName: "Decimal", isRequired: false, defaultValue: "0.40" },
      { name: "MomentumWeight", typeName: "Decimal", isRequired: false, defaultValue: "0.35" },
      { name: "MicroWeight", typeName: "Decimal", isRequired: false, defaultValue: "0.25" },

      // Signal
      { name: "MinSignalStrength", typeName: "Decimal", isRequired: false, defaultValue: "0.65" },
      { name: "MinStrengthDiff", typeName: "Decimal", isRequired: false, defaultValue: "0.05" },

      // SL/TP
      { name: "AtrStopMultiplier", typeName: "Decimal", isRequired: false, defaultValue: "2.0" },
      { name: "AtrTakeProfitMultiplier", typeName: "Decimal", isRequired: false, defaultValue: "4.0" },

      // Intra-Candle
      { name: "EnableIntraCandleDecision", typeName: "Boolean", isRequired: false, defaultValue: "true" },
      { name: "SubCandleTimeFrame", typeName: "TimeFrame", isRequired: false, defaultValue: "M5" },
      { name: "SubCandleHistoryDepth", typeName: "Int32", isRequired: false, defaultValue: "2000" },
      { name: "IntraCandleVolumeThreshold", typeName: "Decimal", isRequired: false, defaultValue: "2.0" },
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