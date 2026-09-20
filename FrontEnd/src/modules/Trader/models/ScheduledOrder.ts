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
  orders: ScheduledOrder[];
}

export type SchedulerStatus =
  | "idle"
  | "waiting-login"
  | "logging-in"
  | "waiting-refresh"
  | "refreshing"
  | "waiting-fire"
  | "firing"
  | "done"
  | "cancelled"
  | "error";

export interface SchedulerRuntime {
  status: SchedulerStatus;
  message: string;
  /** "YYYY-MM-DD" روز جاری که runner داره پردازش می‌کنه */
  currentDate: string | null;
  lastLoginDate: string | null;
  lastRefreshDate: string | null;
  /** کلیدهای شلیک‌شده: "planId:orderId" */
  firedKeys: string[];
  lastLoginAt: number | null;
  lastRefreshAt: number | null;
}

export interface ExportedSchedule {
  version: 1;
  exportedAt: string;
  enabled: boolean;
  autoLoginAt: string;
  autoRefreshAt: string;
  plans: Array<{
    name: string;
    date: string;
    enabled: boolean;
    orders: Array<Omit<ScheduledOrder, "id">>;
  }>;
}