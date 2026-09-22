export type OrderMode = "quantity" | "totalValue";

export interface ScheduledOrderCommand {
  accountId: string;
  symbolIsin: string;
  /** 0 = خرید، 1 = فروش */
  side: number;
  mode: OrderMode;
  quantity: string;
  totalValue: string;
  /** "HH:MM:SS.mmm" */
  time: string;
}

export interface CreateSchedulePlanCommand {
  name: string;
  /** "YYYY-MM-DD" */
  date: string;
  enabled: boolean;
  /** "HH:MM:SS" */
  autoLoginAt: string;
  autoRefreshAt: string;
  orders: ScheduledOrderCommand[];
}

export interface UpdateSchedulePlanCommand extends CreateSchedulePlanCommand {
  id: string;
}

export interface EnableSchedulePlanCommand {
  id: string;
}

export interface DisableSchedulePlanCommand {
  id: string;
}