export interface TradingEventView {
  id: string;
  runId: string | null;
  isin: string | null;
  accountId: string | null;
  signalId: string | null;
  orderIntentId: string | null;
  brokerOrderId: string | null;
  tradeId: string | null;
  positionId: string | null;
  stage: number;
  stageName: string;
  type: number;
  typeName: string;
  severity: number;
  severityName: string;
  occurredAt: string;
  message: string;
  payloadJson: string | null;
}

export interface TradingEventQuery {
  runId?: string | null;
  isin?: string | null;
  accountId?: string | null;
  stage?: number | null;
  severity?: number | null;
  minSeverity?: number | null;
  from?: string | null;
  to?: string | null;
  search?: string | null;
  skip?: number;
  take?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  skip: number;
  take: number;
}

/* ─── Labels ─── */
export const STAGE_LABELS: Record<number, string> = {
  0: "چرخه",
  1: "داده بازار",
  2: "تحلیل",
  3: "اجرا",
  4: "سفارش‌ها",
  5: "بازیابی",
  6: "سیستم",
};

export const SEVERITY_LABELS: Record<number, string> = {
  0: "اشکال‌زدایی",
  1: "اطلاع",
  2: "هشدار",
  3: "خطا",
  4: "بحرانی",
};

export const SEVERITY_COLORS: Record<number, string> = {
  0: "bg-slate-700 text-slate-300",
  1: "bg-blue-900/40 text-blue-300",
  2: "bg-amber-900/40 text-amber-300",
  3: "bg-red-900/40 text-red-300",
  4: "bg-red-950/60 text-red-200 border border-red-500",
};