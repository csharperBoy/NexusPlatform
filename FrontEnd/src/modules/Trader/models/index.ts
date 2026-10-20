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
  OrderMode,
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