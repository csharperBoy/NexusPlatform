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