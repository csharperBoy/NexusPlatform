export enum TradingRunState {
  Created = 0,
  Ready = 1,
  Running = 2,
  Stopped = 3,
  Faulted = 4,
}

export enum CapitalAllocationMode {
  Fixed = 0,
  Dynamic = 1,
}

export enum StopRunBehavior {
  KeepPositions = 0,
  ClosePositions = 1,
}

export enum MarketCloseBehavior {
  KeepPositions = 0,
  ClosePositions = 1,
}

export enum OrderMode {
  Quantity = 0,
  TotalValue = 1,
}

export enum TimeFrame {
  M1 = 1,
  M5 = 5,
  M15 = 15,
  M30 = 30,
  H1 = 60,
  H4 = 240,
  D1 = 1440,
  W1 = 10080,
}

/* ─── Label maps ─── */
export const TRADING_RUN_STATE_LABELS: Record<TradingRunState, string> = {
  [TradingRunState.Created]: "ایجاد شده",
  [TradingRunState.Ready]: "آماده",
  [TradingRunState.Running]: "در حال اجرا",
  [TradingRunState.Stopped]: "متوقف",
  [TradingRunState.Faulted]: "خطا",
};

export const TRADING_RUN_STATE_COLORS: Record<TradingRunState, string> = {
  [TradingRunState.Created]: "bg-slate-700 text-slate-300",
  [TradingRunState.Ready]: "bg-blue-900/40 text-blue-300",
  [TradingRunState.Running]: "bg-emerald-900/40 text-emerald-300",
  [TradingRunState.Stopped]: "bg-amber-900/40 text-amber-300",
  [TradingRunState.Faulted]: "bg-red-900/40 text-red-300",
};

export const CAPITAL_MODE_LABELS: Record<CapitalAllocationMode, string> = {
  [CapitalAllocationMode.Fixed]: "ثابت",
  [CapitalAllocationMode.Dynamic]: "داینامیک (موجودی حساب)",
};

export const TIMEFRAME_LABELS: Record<TimeFrame, string> = {
  [TimeFrame.M1]: "۱ دقیقه",
  [TimeFrame.M5]: "۵ دقیقه",
  [TimeFrame.M15]: "۱۵ دقیقه",
  [TimeFrame.M30]: "۳۰ دقیقه",
  [TimeFrame.H1]: "۱ ساعت",
  [TimeFrame.H4]: "۴ ساعت",
  [TimeFrame.D1]: "روزانه",
  [TimeFrame.W1]: "هفتگی",
};

export const STOP_BEHAVIOR_LABELS: Record<StopRunBehavior, string> = {
  [StopRunBehavior.KeepPositions]: "حفظ موقعیت‌های باز",
  [StopRunBehavior.ClosePositions]: "بستن موقعیت‌های باز",
};

export const MARKET_CLOSE_LABELS: Record<MarketCloseBehavior, string> = {
  [MarketCloseBehavior.KeepPositions]: "حفظ موقعیت‌ها در پایان بازار",
  [MarketCloseBehavior.ClosePositions]: "بستن موقعیت‌ها در پایان بازار",
};

export const ORDER_MODE_LABELS: Record<OrderMode, string> = {
  [OrderMode.Quantity]: "بر اساس تعداد",
  [OrderMode.TotalValue]: "بر اساس ارزش کل",
};