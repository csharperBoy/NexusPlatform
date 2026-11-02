/* ═══════════════════ Enums ═══════════════════ */

export enum BacktestStatus {
  Pending = 0,
  Running = 1,
  Completed = 2,
  Failed = 3,
  Cancelled = 4,
}

export enum BacktestExitReason {
  Signal = 0,
  StopLoss = 1,
  TakeProfit = 2,
  EndOfBacktest = 3,
  ManualClose = 4,
}

export const BACKTEST_STATUS_LABELS: Record<BacktestStatus, string> = {
  [BacktestStatus.Pending]: "در انتظار",
  [BacktestStatus.Running]: "در حال اجرا",
  [BacktestStatus.Completed]: "تکمیل شده",
  [BacktestStatus.Failed]: "خطا",
  [BacktestStatus.Cancelled]: "لغو شده",
};

export const BACKTEST_STATUS_COLORS: Record<BacktestStatus, string> = {
  [BacktestStatus.Pending]: "bg-slate-700 text-slate-300",
  [BacktestStatus.Running]: "bg-blue-900/40 text-blue-300",
  [BacktestStatus.Completed]: "bg-emerald-900/40 text-emerald-300",
  [BacktestStatus.Failed]: "bg-red-900/40 text-red-300",
  [BacktestStatus.Cancelled]: "bg-amber-900/40 text-amber-300",
};

export const EXIT_REASON_LABELS: Record<BacktestExitReason, string> = {
  [BacktestExitReason.Signal]: "سیگنال",
  [BacktestExitReason.StopLoss]: "حد ضرر",
  [BacktestExitReason.TakeProfit]: "حد سود",
  [BacktestExitReason.EndOfBacktest]: "پایان بکتست",
  [BacktestExitReason.ManualClose]: "دستی",
};

/* ═══════════════════ DTOs ═══════════════════ */

export interface BacktestSymbolModel {
  isin: string;
  allocationPercentage: number;
}

export interface RunBacktestConfigModel {
  strategyKey: string;
  strategyConfigJson: string;
  primaryTimeFrame: number;
  targetCurrency: string | null;
  strengthMultiplier: number;
  minDeltaPercent: number;
  stopLossPercentage: number | null;
  stopLossPrice: number | null;
  takeProfitPercentage: number | null;
  takeProfitPrice: number | null;
  defaultOrderMode: number;
  onMarketClose: number;
  onStop: number;
  symbols: BacktestSymbolModel[];
}

export interface RunBacktestModel {
  name: string;
  sourceRunId: string | null;
  configOverride: RunBacktestConfigModel | null;
  dateFrom: string;   // ISO
  dateTo: string;     // ISO
  initialCapital: number | null;
  slippagePercent: number | null;
  subCandleTimeFrame: number | null;
  timeZoneId: string | null;
}

export interface BacktestRunSummaryView {
  id: string;
  name: string;
  status: number;
  statusName: string;
  strategyKey: string;
  primaryTimeFrame: number;
  targetCurrency: string | null;
  initialCapital: number;
  finalEquity: number;
  totalReturnPct: number;
  maxDrawdownPct: number;
  sharpeRatio: number;
  winRate: number;
  totalTrades: number;
  winningTrades: number;
  losingTrades: number;
  dateFrom: string;
  dateTo: string;
  startedAt: string | null;
  completedAt: string | null;
  durationMs: number | null;
  createdAt: string;
}

export interface BacktestRunDetailView extends BacktestRunSummaryView {
  sourceRunId: string | null;
  strategyConfigJson: string;
  capitalAllocationMode: number;
  fixedCapitalAmount: number | null;
  strengthMultiplier: number;
  minDeltaPercent: number;
  stopLossPercentage: number | null;
  stopLossPrice: number | null;
  takeProfitPercentage: number | null;
  takeProfitPrice: number | null;
  defaultOrderMode: number;
  onMarketClose: number;
  onStop: number;
  configSnapshotJson: string;
  slippagePercent: number;
  subCandleTimeFrame: number | null;
  timeZoneId: string;
  profitFactor: number;
  avgWin: number;
  avgLoss: number;
  totalCommission: number;
  totalSlippage: number;
  symbols: BacktestSymbolModel[];
  errorMessage: string | null;
}

export interface BacktestTradeView {
  id: string;
  isin: string;
  entryTime: string;
  lastEntryTime: string;
  totalEntryQuantity: number;
  weightedAvgEntryPrice: number;
  totalBuyCommission: number;
  exitTime: string;
  exitPrice: number;
  exitQuantity: number;
  totalSellCommission: number;
  exitReason: number;
  exitReasonName: string;
  grossPnL: number;
  netPnL: number;
  netPnLPct: number;
  holdingDurationSeconds: number;
  entryFillsJson: string;
}

export interface BacktestDailyEquityView {
  date: string;
  equity: number;
  cash: number;
  positionValue: number;
  dailyReturnPct: number;
}