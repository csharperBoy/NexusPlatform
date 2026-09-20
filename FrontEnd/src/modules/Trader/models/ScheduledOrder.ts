export type OrderMode = "quantity" | "totalValue";

export interface ScheduledOrder {
  id: string;
  accountId: string;
  symbolIsin: string;
  side: 0 | 1;
  mode: OrderMode;
  quantity: string;
  totalValue: string;
  /** "HH:MM:SS.mmm" */
  time: string;
}

export interface SchedulePlan {
  id: string;
  name: string;
  /** "YYYY-MM-DD" */
  date: string;
  enabled: boolean;
  /** "HH:MM:SS" — ساعت لاگین این برنامه */
  autoLoginAt: string;
  /** "HH:MM:SS" — ساعت رفرش قیمت‌های این برنامه */
  autoRefreshAt: string;
  /** وضعیت کارت: بسته یا باز */
  collapsed: boolean;
  orders: ScheduledOrder[];
}

export interface PlanLogEntry {
  t: string;
  msg: string;
  type: "ok" | "err" | "info" | "send";
}

/** State هر برنامه در طول روز (روی روز عوض شه ریست میشه) */
export interface PlanRuntimeState {
  lastLoginDate: string | null;
  lastRefreshDate: string | null;
  firedOrderIds: string[];
  /** orderId که برای اون clock sync انجام شده (جلوگیری از تکرار) */
  clockSyncedForOrderId: string | null;
  /** لاگ‌های مخصوص این پلن */
  logs: PlanLogEntry[];
}

export type SchedulerStatus =
  | "idle"
  | "waiting"
  | "logging-in"
  | "refreshing"
  | "waiting-fire"
  | "firing"
  | "done"
  | "cancelled"
  | "error";

export interface SchedulerRuntime {
  /** "YYYY-MM-DD" */
  currentDate: string | null;
  /** کلیدش planId */
  planStates: Record<string, PlanRuntimeState>;
  /** کلی برای نمایش */
  status: SchedulerStatus;
  message: string;
  /** برای هر پلن: پیام وضعیت */
  planMessages: Record<string, string>;
}

export interface ExportedSchedule {
  version: 2;
  exportedAt: string;
  enabled: boolean;
  plans: Array<{
    name: string;
    date: string;
    enabled: boolean;
    autoLoginAt: string;
    autoRefreshAt: string;
    orders: Array<Omit<ScheduledOrder, "id">>;
  }>;
}