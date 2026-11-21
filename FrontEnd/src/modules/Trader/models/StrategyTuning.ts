// src/modules/Trader/models/StrategyTuning.ts
export enum TuningStatus {
  Pending = 0, Running = 1, Completed = 2, Failed = 3, Cancelled = 4,
}
export const TUNING_STATUS_LABELS: Record<TuningStatus, string> = {
  [TuningStatus.Pending]: "در انتظار",
  [TuningStatus.Running]: "در حال اجرا",
  [TuningStatus.Completed]: "تکمیل",
  [TuningStatus.Failed]: "خطا",
  [TuningStatus.Cancelled]: "لغو",
};

export interface ParameterDefinitionView {
  name: string;
  displayName: string;
  typeName: string;
  importance: number;      // 0-4
  importanceLabel: string;
  description: string | null;
  minValue: number | string | null;
  maxValue: number | string | null;
  allowedValues: (string | number | boolean)[] | null;
}

export const IMPORTANCE_COLORS: Record<number, string> = {
  4: "bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-300",
  3: "bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-300",
  2: "bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-300",
  1: "bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-400",
  0: "bg-slate-100 text-slate-500 dark:bg-slate-700 dark:text-slate-400",
};
export const TUNING_SCALE_LABELS: Record<number, string> = {
  1: "سالانه", 2: "۶ ماهه", 4: "۳ ماهه", 8: "۱ ماهه",
};

export interface CreateStrategyTuningModel {
  name: string;
  strategyKey: string;
  isin: string;
  targetCurrency: string | null;
  primaryTimeFrame: number;
  subCandleTimeFrame: number | null;
  dateFrom: string;
  dateTo: string;
  initialCapital: number;
  slippagePercent: number;
  selectedScales: number;
  primaryScale: number;
  minTradesPerWindow: number;
  populationSize: number;
  generations: number;
  mutationRate: number;
  crossoverRate: number;
  
  selectedParameters: string[] | null;
}

export interface StrategyTuningRunSummaryView {
  id: string; name: string; strategyKey: string; isin: string;
  primaryTimeFrame: number; primaryTimeFrameName: string;
  dateFrom: string; dateTo: string;
  status: number; statusName: string;
  totalEvaluations: number; completedEvaluations: number;
  startedAt: string | null; completedAt: string | null; createdAt: string;
}

export interface StrategyTuningResultView {
  id: string; generation: number; status: number; configJson: string;
  avgReturnCompoundYearly: number | null;
  avgReturnCompoundSemi: number | null;
  avgReturnCompoundQuarterly: number | null;
  avgReturnCompoundMonthly: number | null;
  validWindowsYearly: number | null;
  validWindowsSemi: number | null;
  validWindowsQuarterly: number | null;
  validWindowsMonthly: number | null;
  totalTradesYearly: number | null;
  totalTradesSemi: number | null;
  totalTradesQuarterly: number | null;
  totalTradesMonthly: number | null;
  isTop10InYearly: boolean; isTop10InSemi: boolean;
  isTop10InQuarterly: boolean; isTop10InMonthly: boolean;
  runtimeMs: number;
}

export interface StrategyTuningRunDetailView extends StrategyTuningRunSummaryView {
  targetCurrency: string | null;
  subCandleTimeFrame: number | null;
  initialCapital: number; slippagePercent: number;
  selectedScales: number; primaryScale: number; minTradesPerWindow: number;
  populationSize: number; generations: number;
  mutationRate: number; crossoverRate: number;
  bestResultIdYearly: string | null;
  bestResultIdSemi: string | null;
  bestResultIdQuarterly: string | null;
  bestResultIdMonthly: string | null;
  errorMessage: string | null;
  topResults: StrategyTuningResultView[];
  selectedParameters: string[] | null;
}