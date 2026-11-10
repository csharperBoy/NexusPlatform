/* ═══════════════ PriceSource ═══════════════ */

export enum PriceSourceType {
  Tgju = 1,
  TelegramChannel = 2,
  CustomApi = 3,
}

export const PRICE_SOURCE_TYPE_LABELS: Record<PriceSourceType, string> = {
  [PriceSourceType.Tgju]: "TGJU",
  [PriceSourceType.TelegramChannel]: "کانال تلگرام",
  [PriceSourceType.CustomApi]: "API سفارشی",
};

export interface PriceSourceView {
  id: string;
  code: string;
  name: string;
  type: number;
  typeName: string;
  configJson: string;
  fromAsset: string;
  toAsset: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreatePriceSourceCommand {
  code: string;
  name: string;
  type: number;
  configJson: string;
  fromAsset: string;
  toAsset: string;
}

export interface UpdatePriceSourceCommand {
  id: string;
  name: string;
  configJson: string;
  fromAsset: string;
  toAsset: string;
}

/* ═══════════════ PriceCollectionJob ═══════════════ */

export enum PriceCollectionMode {
  Recurring = 0,
  Backfill = 1,
}

export const PRICE_COLLECTION_MODE_LABELS: Record<PriceCollectionMode, string> = {
  [PriceCollectionMode.Recurring]: "تکرارشونده",
  [PriceCollectionMode.Backfill]: "Backfill",
};

export const PRICE_COLLECTION_MODE_COLORS: Record<PriceCollectionMode, string> = {
  [PriceCollectionMode.Recurring]: "bg-indigo-900/40 text-indigo-300",
  [PriceCollectionMode.Backfill]: "bg-purple-900/40 text-purple-300",
};

export enum PriceCollectionJobState {
  Created = 0,
  Running = 1,
  Stopped = 2,
  Completed = 3,
  Faulted = 4,
}

export const PRICE_COLLECTION_JOB_STATE_LABELS: Record<PriceCollectionJobState, string> = {
  [PriceCollectionJobState.Created]: "ایجاد شده",
  [PriceCollectionJobState.Running]: "در حال اجرا",
  [PriceCollectionJobState.Stopped]: "متوقف",
  [PriceCollectionJobState.Completed]: "تکمیل شده",
  [PriceCollectionJobState.Faulted]: "خطا",
};

export const PRICE_COLLECTION_JOB_STATE_COLORS: Record<PriceCollectionJobState, string> = {
  [PriceCollectionJobState.Created]: "bg-slate-700 text-slate-300",
  [PriceCollectionJobState.Running]: "bg-emerald-900/40 text-emerald-300",
  [PriceCollectionJobState.Stopped]: "bg-amber-900/40 text-amber-300",
  [PriceCollectionJobState.Completed]: "bg-blue-900/40 text-blue-300",
  [PriceCollectionJobState.Faulted]: "bg-red-900/40 text-red-300",
};

export interface PriceCollectionJobView {
  id: string;
  priceSourceId: string;
  priceSourceCode: string;
  priceSourceName: string;
  mode: number;
  modeName: string;
  state: number;
  stateName: string;
  recurrencePatternJson: string | null;
  schedulerRecurringId: string | null;
  schedulerJobId: string | null;
  startedAt: string | null;
  stoppedAt: string | null;
  lastRunAt: string | null;
  lastRunInserted: number;
  lastRunSkipped: number;
  totalInserted: number;
  backfillProgressPercent: number | null;
  lastError: string | null;
  createdAt: string;
}

export interface CreatePriceCollectionJobCommand {
  priceSourceId: string;
  mode: number;
  recurrencePatternJson?: string | null;
}

/* ═══════════════ Recurrence Pattern (mirror Backend) ═══════════════ */

export enum RecurrenceType {
  Daily = 0,
  Weekly = 1,
  Monthly = 2,
  Minutely = 3,
  Hourly = 4,
  Cron = 5,
}

export const RECURRENCE_TYPE_LABELS: Record<RecurrenceType, string> = {
  [RecurrenceType.Minutely]: "هر چند دقیقه",
  [RecurrenceType.Hourly]: "هر چند ساعت",
  [RecurrenceType.Daily]: "روزانه",
  [RecurrenceType.Cron]: "Cron Expression",
  [RecurrenceType.Weekly]: "هفتگی (B2)",
  [RecurrenceType.Monthly]: "ماهانه (B2)",
};

export interface RecurrencePatternJson {
  type: number;
  interval: number;
  daysOfWeek: number[] | null;
  daysOfMonth: number[] | null;
  cronExpression: string | null;
}

/**
 * Construct JSON مطابق Scheduler.Domain.RecurrencePattern.
 * فقط typeهای Phase 1 (Minutely/Hourly/Daily/Cron) تولید می‌شوند.
 */
export function buildRecurrencePattern(
  type: RecurrenceType,
  interval: number,
  cronExpression: string | null,
): string {
  const pattern: RecurrencePatternJson = {
    type,
    interval: Math.max(1, interval),
    daysOfWeek: null,
    daysOfMonth: null,
    cronExpression: type === RecurrenceType.Cron ? (cronExpression ?? "") : null,
  };
  return JSON.stringify(pattern);
}

export function formatRecurrencePattern(json: string | null): string {
  if (!json) return "—";
  try {
    const p = JSON.parse(json) as RecurrencePatternJson;
    switch (p.type) {
      case RecurrenceType.Minutely: return `هر ${p.interval} دقیقه`;
      case RecurrenceType.Hourly: return `هر ${p.interval} ساعت`;
      case RecurrenceType.Daily: return `هر ${p.interval} روز`;
      case RecurrenceType.Cron: return `Cron: ${p.cronExpression}`;
      default: return json;
    }
  } catch {
    return json;
  }
}