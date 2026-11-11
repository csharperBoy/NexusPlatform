/* ═══════════════ Enums (mirror Domain) ═══════════════ */

export enum OrderSide {
  Buy = 0,
  Sell = 1,
}

export enum OrderStateKind {
  Unknown = 0,
  Pending = 1,
  Open = 2,
  PartiallyExecuted = 3,
  FullyExecuted = 4,
  Canceled = 5,
  Rejected = 6,
  Expired = 7,
}

export enum OrderIntentState {
  Pending = 0,
  Dispatched = 1,
  Acknowledged = 2,
  Rejected = 3,
  Failed = 4,
  RiskBlocked = 5,
}

/* ═══════════════ Views ═══════════════ */

export interface BrokerOrderView {
  id: string;
  orderIntentId: string | null;
  runId: string;
  traderAccountId: string;
  accountName: string | null;
  isin: string;
  symbolName: string | null;
  side: number;
  sideName: string;
  price: number;
  quantity: number;
  filledQuantity: number;
  filledRatio: number;
  state: number;
  stateName: string;
  isTerminal: boolean;
  isPaper: boolean;
  brokerOrderId: string | null;
  submittedAt: string;
  lastSyncedAt: string | null;
  createdAt: string;
}

export interface OrderTradeView {
  id: string;
  tradeNumber: number;
  price: number;
  quantity: number;
  tradeDate: string;
  isCanceled: boolean;
  cancelDateTime: string | null;
  createdAt: string;
}

export interface BrokerOrderDetailView {
  summary: BrokerOrderView;
  trades: OrderTradeView[];
}

export interface OrderIntentView {
  id: string;
  runId: string;
  traderAccountId: string;
  accountName: string | null;
  sourceSignalId: string | null;
  sourceEvaluationId: string | null;
  isin: string;
  symbolName: string | null;
  side: number;
  sideName: string;
  price: number;
  quantity: number;
  orderMode: number;
  orderModeName: string;
  state: number;
  stateName: string;
  reason: string | null;
  failureReason: string | null;
  stopLossPrice: number | null;
  takeProfitPrice: number | null;
  isPaper: boolean;
  dispatchedAt: string | null;
  completedAt: string | null;
  createdAt: string;
}

/* ═══════════════ Queries ═══════════════ */

export interface BrokerOrderQuery {
  runId?: string | null;
  accountId?: string | null;
  isin?: string | null;
  state?: number | null;
  isPaper?: boolean | null;
  submittedFrom?: string | null;
  submittedTo?: string | null;
  search?: string | null;
  skip?: number;
  take?: number;
}

export interface OrderIntentQuery {
  runId?: string | null;
  accountId?: string | null;
  isin?: string | null;
  state?: number | null;
  isPaper?: boolean | null;
  from?: string | null;
  to?: string | null;
  search?: string | null;
  skip?: number;
  take?: number;
}

/* ═══════════════ Enriched Paged Result ═══════════════ */

export interface PagedResult<T> {
  items: T[];
  totalRecords: number;
  page: number;
  pageSize: number;
}

/* ═══════════════ Colors (state number → tailwind classes) ═══════════════ */

export const ORDER_STATE_COLORS: Record<number, string> = {
  [OrderStateKind.Unknown]: "bg-slate-700 text-slate-300",
  [OrderStateKind.Pending]: "bg-blue-900/40 text-blue-300",
  [OrderStateKind.Open]: "bg-blue-900/40 text-blue-300",
  [OrderStateKind.PartiallyExecuted]: "bg-amber-900/40 text-amber-300",
  [OrderStateKind.FullyExecuted]: "bg-emerald-900/40 text-emerald-300",
  [OrderStateKind.Canceled]: "bg-slate-700 text-slate-400",
  [OrderStateKind.Rejected]: "bg-red-900/40 text-red-300",
  [OrderStateKind.Expired]: "bg-slate-700 text-slate-400",
};

export const INTENT_STATE_COLORS: Record<number, string> = {
  [OrderIntentState.Pending]: "bg-slate-700 text-slate-300",
  [OrderIntentState.Dispatched]: "bg-blue-900/40 text-blue-300",
  [OrderIntentState.Acknowledged]: "bg-emerald-900/40 text-emerald-300",
  [OrderIntentState.Rejected]: "bg-red-900/40 text-red-300",
  [OrderIntentState.Failed]: "bg-red-900/40 text-red-300",
  [OrderIntentState.RiskBlocked]: "bg-amber-900/40 text-amber-300",
};

/* ═══════════════ Filter Options ═══════════════ */

export const ORDER_STATE_OPTIONS = [
  { value: OrderStateKind.Pending, label: "در انتظار" },
  { value: OrderStateKind.Open, label: "باز" },
  { value: OrderStateKind.PartiallyExecuted, label: "نیمه اجرا" },
  { value: OrderStateKind.FullyExecuted, label: "کامل اجرا" },
  { value: OrderStateKind.Canceled, label: "لغو شده" },
  { value: OrderStateKind.Rejected, label: "رد شده" },
  { value: OrderStateKind.Expired, label: "منقضی" },
];

export const INTENT_STATE_OPTIONS = [
  { value: OrderIntentState.Pending, label: "در انتظار" },
  { value: OrderIntentState.Dispatched, label: "ارسال شده" },
  { value: OrderIntentState.Acknowledged, label: "تأیید شده" },
  { value: OrderIntentState.Rejected, label: "رد شده" },
  { value: OrderIntentState.Failed, label: "خطا" },
  { value: OrderIntentState.RiskBlocked, label: "مسدود توسط Risk" },
];

export const PAPER_OPTIONS = [
  { value: "", label: "همه" },
  { value: "true", label: "📄 Paper" },
  { value: "false", label: "🔴 Live" },
];