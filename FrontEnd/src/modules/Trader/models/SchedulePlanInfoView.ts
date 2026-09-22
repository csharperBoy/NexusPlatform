import type { OrderMode } from "./SchedulePlanCommand";

export interface ScheduledOrderInfoView {
  id: string;
  accountId: string;
  accountName?: string | null;
  symbolIsin: string;
  symbolName?: string | null;
  side: number;
  mode: OrderMode;
  quantity: string;
  totalValue: string;
  time: string;
  /** آیا این سفارش توسط بک‌اند ارسال شده */
  fired: boolean;
}

export type SchedulePlanStatus =
  | "idle"
  | "waiting"
  | "logging-in"
  | "refreshing"
  | "waiting-fire"
  | "firing"
  | "done"
  | "cancelled"
  | "error";

export interface SchedulePlanInfoView {
  id: string;
  name: string;
  date: string;
  enabled: boolean;
  autoLoginAt: string;
  autoRefreshAt: string;
  orders: ScheduledOrderInfoView[];
  status: SchedulePlanStatus;
  message?: string | null;
}