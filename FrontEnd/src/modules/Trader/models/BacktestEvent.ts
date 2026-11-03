export enum BacktestEventType {
  RunStarted = 1,
  RunCompleted = 2,
  RunFailed = 3,
  ConfigLoaded = 10,
  InstrumentsLoaded = 11,
  CandlesLoaded = 12,
  EventsBuilt = 13,
  SimulationStarted = 20,
  SignalGenerated = 21,
  FillExecuted = 22,
  PositionOpened = 23,
  PositionIncreased = 24,
  PositionClosed = 25,
  SLTPTriggered = 26,
  MetricsComputed = 30,
}

export enum BacktestEventSeverity {
  Debug = 0,
  Info = 1,
  Warning = 2,
  Error = 3,
  Critical = 4,
}

export const BACKTEST_EVENT_TYPE_LABELS: Record<number, string> = {
  [BacktestEventType.RunStarted]: "شروع",
  [BacktestEventType.RunCompleted]: "تکمیل",
  [BacktestEventType.RunFailed]: "خطا",
  [BacktestEventType.ConfigLoaded]: "Config",
  [BacktestEventType.InstrumentsLoaded]: "Instruments",
  [BacktestEventType.CandlesLoaded]: "کندل‌ها",
  [BacktestEventType.EventsBuilt]: "Eventها",
  [BacktestEventType.SimulationStarted]: "شبیه‌سازی",
  [BacktestEventType.SignalGenerated]: "سیگنال",
  [BacktestEventType.FillExecuted]: "Fill",
  [BacktestEventType.PositionOpened]: "باز شدن",
  [BacktestEventType.PositionIncreased]: "افزایش",
  [BacktestEventType.PositionClosed]: "بسته شدن",
  [BacktestEventType.SLTPTriggered]: "SL/TP",
  [BacktestEventType.MetricsComputed]: "متریک",
};

export const BACKTEST_EVENT_TYPE_COLORS: Record<number, string> = {
  [BacktestEventType.RunStarted]: "bg-blue-900/40 text-blue-300",
  [BacktestEventType.RunCompleted]: "bg-emerald-900/40 text-emerald-300",
  [BacktestEventType.RunFailed]: "bg-red-900/40 text-red-300",
  [BacktestEventType.ConfigLoaded]: "bg-slate-700 text-slate-300",
  [BacktestEventType.InstrumentsLoaded]: "bg-slate-700 text-slate-300",
  [BacktestEventType.CandlesLoaded]: "bg-slate-700 text-slate-300",
  [BacktestEventType.EventsBuilt]: "bg-slate-700 text-slate-300",
  [BacktestEventType.SimulationStarted]: "bg-slate-700 text-slate-300",
  [BacktestEventType.SignalGenerated]: "bg-amber-900/40 text-amber-300",
  [BacktestEventType.FillExecuted]: "bg-blue-900/40 text-blue-300",
  [BacktestEventType.PositionOpened]: "bg-emerald-900/40 text-emerald-300",
  [BacktestEventType.PositionIncreased]: "bg-emerald-900/40 text-emerald-300",
  [BacktestEventType.PositionClosed]: "bg-slate-700 text-slate-300",
  [BacktestEventType.SLTPTriggered]: "bg-red-900/40 text-red-300",
  [BacktestEventType.MetricsComputed]: "bg-blue-900/40 text-blue-300",
};

export const BACKTEST_EVENT_SEVERITY_LABELS: Record<number, string> = {
  0: "Debug",
  1: "Info",
  2: "Warning",
  3: "Error",
  4: "Critical",
};

export interface BacktestEventView {
  id: string;
  backtestRunId: string;
  type: number;
  typeName: string;
  severity: number;
  severityName: string;
  occurredAt: string;
  message: string;
  payloadJson: string | null;
  isin: string | null;
  candleTime: string | null;
  tradeId: string | null;
}

export interface BacktestEventQuery {
  backtestRunId: string;
  type?: number | null;
  minSeverity?: number | null;
  isin?: string | null;
  search?: string | null;
  skip?: number;
  take?: number;
}