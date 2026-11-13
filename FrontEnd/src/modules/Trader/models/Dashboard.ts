/* ═══════════════ Summary DTOs ═══════════════ */

export interface DashboardSummaryView {
  killSwitch: KillSwitchSummary;
  runs: RunsSummary;
  ordersToday: OrdersTodaySummary;
  positions: PositionsSummary;
  recentCriticalEvents: CriticalEventView[];
  accountsSessions: AccountsSessionSummary;
  generatedAt: string;
}

export interface KillSwitchSummary {
  globalActive: boolean;
  globalReason: string | null;
  globalActivatedAt: string | null;
  activeAccounts: KillSwitchAccountView[];
}

export interface KillSwitchAccountView {
  accountId: string;
  accountName: string;
  reason: string | null;
  activatedAt: string;
}

export interface RunsSummary {
  running: number;
  ready: number;
  stopped: number;
  faulted: number;
  created: number;
  total: number;
}

export interface OrdersTodaySummary {
  todayStartUtc: string;
  total: number;
  paper: number;
  live: number;
  pending: number;
  open: number;
  partiallyExecuted: number;
  fullyExecuted: number;
  canceled: number;
  rejected: number;
  expired: number;
}

export interface PositionsSummary {
  totalCount: number;
  paperCount: number;
  liveCount: number;
  totalUnrealizedPnL: number;
  items: PositionBriefView[];
}

export interface PositionBriefView {
  id: string;
  runId: string;
  accountId: string;
  accountName: string;
  isin: string;
  symbolName: string;
  quantity: number;
  averageEntryPrice: number;
  currentPrice: number | null;
  unrealizedPnL: number | null;
  isPaper: boolean;
}

export interface CriticalEventView {
  id: string;
  occurredAt: string;
  severity: number;
  severityName: string;
  type: number;
  typeName: string;
  isin: string | null;
  runId: string | null;
  message: string;
}

export interface AccountsSessionSummary {
  total: number;
  valid: number;
  expired: number;
  empty: number;
  invalid: number;
}