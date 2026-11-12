//src/modules/Trader/models/index.ts
export type {
  CreateAccountCommand,
  UpdateAccountCommand,
  LoginAccountCommand,
  ActivateAccountCommand,
} from "./AccountCommand";
export type { AccountInfoView, TokenStatus } from "./AccountInfoView";

// ─── Instrument (NEW) ───
export type {
  InstrumentInfoView,
} from "./InstrumentInfoView";
export type {
  CreateInstrumentCommand,
  UpdateInstrumentCommand,
  OptionContractModel,
} from "./InstrumentCommand";
export { InstrumentType, MarketType, OPTION_TYPE_OPTIONS } from "./InstrumentType";
export type { OptionTypeOption } from "./InstrumentType";

// ─── Legacy Symbol (نگه‌داری برای سازگاری، حذف در Phase 2) ───
export type {
  CreateSymbolCommand,
  UpdateSymbolCommand,
  GetSymbolMarketInfoQuery,
} from "./SymbolCommand";
export type { SymbolInfoView, MarketSymbolInfoView } from "./SymbolInfoView";

export type {
  ScheduleOrderMode,
  ScheduledOrderCommand,
  CreateSchedulePlanCommand,
  UpdateSchedulePlanCommand,
  EnableSchedulePlanCommand,
  DisableSchedulePlanCommand,
} from "./SchedulePlanCommand";
export type {
  ScheduledOrderInfoView,
  SchedulePlanInfoView,
  SchedulePlanStatus,
} from "./SchedulePlanInfoView";

export type {
  ServerClockInfoView,
  SyncServerClockCommand,
} from "./ServerClockInfoView";

export type {
  ExecutionLogInfoView,
  ExecutionLogQueryResult,
} from "./ExecutionLogInfoView";

// ─── Strategy ───
export type {
  StrategyInfoView,
  StrategyConfigFieldView,
} from "./StrategyInfoView";

// ─── TradingRun (NEW) ───
export type {
  TradingRunInfoView,
  TradingRunDetailView,
  TradingRunSettingsDto,
  TradingRunSymbolDto,
  TradingRunAccountDto,          // ← NEW
  TradingRunAccountModel,        // ← NEW
  TradingRunSettingsModel,
  TradingRunSymbolModel,
  CreateTradingRunCommand,
  UpdateTradingRunCommand,
  StopTradingRunCommand,
} from "./TradingRun";

export {
  TradingRunState,
  CapitalAllocationMode,
  StopRunBehavior,
  MarketCloseBehavior,
  OrderMode,
  TimeFrame,
  TRADING_RUN_STATE_LABELS,
  TRADING_RUN_STATE_COLORS,
  CAPITAL_MODE_LABELS,
  TIMEFRAME_LABELS,
  STOP_BEHAVIOR_LABELS,
  MARKET_CLOSE_LABELS,
  ORDER_MODE_LABELS,
} from "./TradingRunEnums";

// ─── Position (NEW) ───
export type { PositionView } from "./PositionView";
export {
  PositionState,
  POSITION_STATE_LABELS,
  POSITION_STATE_COLORS,
} from "./PositionView";

// ─── Signal (NEW) ───
export type { SignalView } from "./SignalView";
export {
  SignalAction,
  SIGNAL_ACTION_LABELS,
  SIGNAL_ACTION_COLORS,
} from "./SignalView";

export type { TradingScheduleInfoView } from "./TradingScheduleInfoView";

export type { CurrencyRateView } from "./CurrencyRateView";
export type { CreateCurrencyRateCommand } from "./CreateCurrencyRateCommand";

export type {
  TradingEventView,
  TradingEventQuery,
} from "./TradingEventView";

export {
  STAGE_LABELS,
  SEVERITY_LABELS,
  SEVERITY_COLORS,
} from "./TradingEventView";

export type {
  BacktestSymbolModel,
  RunBacktestConfigModel,
  RunBacktestModel,
  BacktestRunSummaryView,
  BacktestRunDetailView,
  BacktestTradeView,
  BacktestDailyEquityView,
} from "./Backtest";

export {
  BacktestStatus,
  BacktestExitReason,
  BACKTEST_STATUS_LABELS,
  BACKTEST_STATUS_COLORS,
  EXIT_REASON_LABELS,
} from "./Backtest";

export type {
  HistoricalDataCollectionJobSummaryView,
  HistoricalDataCollectionJobDetailView,
  CreateHistoricalBackfillCommand,
  CreateHistoricalRecurringCommand,
} from "./HistoricalDataCollection";

export {
  HistoricalCollectionMode,
  HistoricalJobState,
  HISTORICAL_MODE_LABELS,
  HISTORICAL_MODE_COLORS,
  HISTORICAL_JOB_STATE_LABELS,
  HISTORICAL_JOB_STATE_COLORS,
  COLLECTION_TIMEFRAME_OPTIONS,
  TIMEFRAME_LABEL_MAP,
  toEndOfDayUtc,
  toStartOfDayUtc,
  toDateInputValue,
} from "./HistoricalDataCollection";

export type { BacktestEventView, BacktestEventQuery } from "./BacktestEvent";
export {
  BacktestEventType,
  BacktestEventSeverity,
  BACKTEST_EVENT_TYPE_LABELS,
  BACKTEST_EVENT_TYPE_COLORS,
  BACKTEST_EVENT_SEVERITY_LABELS,
} from "./BacktestEvent";

export {
  ExecutionMode,
  EXECUTION_MODE_LABELS,
  EXECUTION_MODE_COLORS,
} from "./TradingRunEnums";

// ─── Risk (Phase D — PR 14-20) ───
export type {
  KillSwitchStatusView,
  LossStreakState,
  ActivateKillSwitchCommand,
  DeactivateKillSwitchCommand,
  ResetCircuitBreakerCommand,
} from "./Risk";

export {
  KillSwitchScope,
  isCircuitActive,
  formatCooldown,
} from "./Risk";

// ─── Price Collection (B1) ───
export type {
  PriceSourceView,
  CreatePriceSourceCommand,
  UpdatePriceSourceCommand,
  PriceCollectionJobView,
  CreatePriceCollectionJobCommand,
  RecurrencePatternJson,
} from "./PriceCollection";
export {
  PriceSourceType,
  PRICE_SOURCE_TYPE_LABELS,
  PriceCollectionMode,
  PRICE_COLLECTION_MODE_LABELS,
  PRICE_COLLECTION_MODE_COLORS,
  PriceCollectionJobState,
  PRICE_COLLECTION_JOB_STATE_LABELS,
  PRICE_COLLECTION_JOB_STATE_COLORS,
  RecurrenceType,
  RECURRENCE_TYPE_LABELS,
  buildRecurrencePattern,
  formatRecurrencePattern,
} from "./PriceCollection";

// ─── Orders (C-lite.1) ───
export type {
  BrokerOrderView,
  BrokerOrderDetailView,
  OrderTradeView,
  OrderIntentView,
  BrokerOrderQuery,
  OrderIntentQuery,
} from "./Orders";

export {
  OrderSide,
  OrderStateKind,
  OrderIntentState,
  ORDER_STATE_COLORS,
  INTENT_STATE_COLORS,
  ORDER_STATE_OPTIONS,
  INTENT_STATE_OPTIONS,
  PAPER_OPTIONS,
} from "./Orders";

// ─── Order Presets (C-lite.2 + strategy refactor) ───
export type {
  OrderPresetView,
  ResolvedOrderPresetView,
  CreateOrderPresetCommand,
  UpdateOrderPresetCommand,
} from "./OrderPresets";

export {
  OrderPriceStrategy,
  OrderQuantityStrategy,
  ORDER_PRICE_STRATEGY_OPTIONS,
  ORDER_QUANTITY_STRATEGY_OPTIONS,
  DEFAULT_SIDE_OPTIONS,
  DEFAULT_ORDER_MODE_OPTIONS,
  VALIDITY_TYPE_OPTIONS,
  labelOf,
} from "./OrderPresets";