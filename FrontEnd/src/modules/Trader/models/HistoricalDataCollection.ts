import type { SelectionListDto } from "@/core/models/SelectionListDto";

/* ═══════════════ Enums ═══════════════ */

export enum HistoricalCollectionMode {
  Backfill = 0,
  Recurring = 1,
}

export const HISTORICAL_MODE_LABELS: Record<HistoricalCollectionMode, string> = {
  [HistoricalCollectionMode.Backfill]: "Backfill",
  [HistoricalCollectionMode.Recurring]: "تکرارشونده",
};

export const HISTORICAL_MODE_COLORS: Record<HistoricalCollectionMode, string> = {
  [HistoricalCollectionMode.Backfill]: "bg-purple-900/40 text-purple-300",
  [HistoricalCollectionMode.Recurring]: "bg-indigo-900/40 text-indigo-300",
};

export enum HistoricalJobState {
  Created = 0,
  Running = 1,
  Stopped = 2,
  Completed = 3,
  Faulted = 4,
}

export const HISTORICAL_JOB_STATE_LABELS: Record<HistoricalJobState, string> = {
  [HistoricalJobState.Created]: "ایجاد شده",
  [HistoricalJobState.Running]: "در حال اجرا",
  [HistoricalJobState.Stopped]: "متوقف",
  [HistoricalJobState.Completed]: "تکمیل شده",
  [HistoricalJobState.Faulted]: "خطا",
};

export const HISTORICAL_JOB_STATE_COLORS: Record<HistoricalJobState, string> = {
  [HistoricalJobState.Created]: "bg-slate-700 text-slate-300",
  [HistoricalJobState.Running]: "bg-emerald-900/40 text-emerald-300",
  [HistoricalJobState.Stopped]: "bg-amber-900/40 text-amber-300",
  [HistoricalJobState.Completed]: "bg-blue-900/40 text-blue-300",
  [HistoricalJobState.Faulted]: "bg-red-900/40 text-red-300",
};

/* ═══════════════ Views ═══════════════ */

export interface HistoricalDataCollectionJobSummaryView {
  id: string;
  name: string;
  mode: number;
  modeName: string;
  state: number;
  stateName: string;
  schedulerRecurringId: string | null;
  schedulerJobId: string | null;
  recurrencePatternJson: string | null;
  totalCombinations: number;
  completedCombinations: number;
  totalCandlesCollected: number;
  backfillProgressPercent: number | null;
  currentIsin: string | null;
  currentTimeFrame: number | null;
  startedAt: string | null;
  stoppedAt: string | null;
  lastRunAt: string | null;
  lastRunInserted: number;
  lastRunSkipped: number;
  completedAt: string | null;
  durationMs: number | null;
  lastError: string | null;
  createdAt: string;
}

export interface HistoricalDataCollectionJobDetailView
  extends HistoricalDataCollectionJobSummaryView {
  isins: string[];
  timeFrames: number[];
  dateFrom: string;
  dateTo: string | null;
}

/* ═══════════════ Commands ═══════════════ */

export interface CreateHistoricalBackfillCommand {
  name: string;
  isins: string[];
  timeFrames: number[];
  dateFrom: string;   // ISO
  dateTo: string;     // ISO
}

export interface CreateHistoricalRecurringCommand {
  name: string;
  isins: string[];
  timeFrames: number[];
  dateFrom: string;   // ISO
  recurrencePatternJson: string;
}

/* ═══════════════ Timeframe options ═══════════════ */

export const COLLECTION_TIMEFRAME_OPTIONS = [
  { value: 1,     label: "۱ دقیقه" },
  { value: 5,     label: "۵ دقیقه" },
  { value: 15,    label: "۱۵ دقیقه" },
  { value: 30,    label: "۳۰ دقیقه" },
  { value: 60,    label: "۱ ساعت" },
  { value: 240,   label: "۴ ساعت" },
  { value: 1440,  label: "روزانه" },
  { value: 10080, label: "هفتگی" },
];

export const TIMEFRAME_LABEL_MAP: Record<number, string> = Object.fromEntries(
  COLLECTION_TIMEFRAME_OPTIONS.map((o) => [o.value, o.label]),
);

/* ═══════════════ Helpers ═══════════════ */

/** Convert a YYYY-MM-DD input to end-of-day UTC ISO. */
export function toEndOfDayUtc(dateInput: string): string {
  return new Date(`${dateInput}T23:59:59Z`).toISOString();
}

/** Convert a YYYY-MM-DD input to start-of-day UTC ISO. */
export function toStartOfDayUtc(dateInput: string): string {
  return new Date(`${dateInput}T00:00:00Z`).toISOString();
}

/** Format an ISO date to YYYY-MM-DD for `<input type="date">`. */
export function toDateInputValue(iso: string | null): string {
  if (!iso) return "";
  return iso.slice(0, 10);
}

/* Re-export SelectionListDto shape hint used in the modal — convenience. */
export type IsinSelectionOption = SelectionListDto;