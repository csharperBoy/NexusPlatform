export enum CollectionJobStatus {
  Pending = 0,
  Running = 1,
  Completed = 2,
  Failed = 3,
  Cancelled = 4,
}

export const COLLECTION_STATUS_LABELS: Record<CollectionJobStatus, string> = {
  [CollectionJobStatus.Pending]: "در انتظار",
  [CollectionJobStatus.Running]: "در حال جمع‌آوری",
  [CollectionJobStatus.Completed]: "تکمیل شده",
  [CollectionJobStatus.Failed]: "خطا",
  [CollectionJobStatus.Cancelled]: "لغو شده",
};

export const COLLECTION_STATUS_COLORS: Record<CollectionJobStatus, string> = {
  [CollectionJobStatus.Pending]: "bg-slate-700 text-slate-300",
  [CollectionJobStatus.Running]: "bg-blue-900/40 text-blue-300",
  [CollectionJobStatus.Completed]: "bg-emerald-900/40 text-emerald-300",
  [CollectionJobStatus.Failed]: "bg-red-900/40 text-red-300",
  [CollectionJobStatus.Cancelled]: "bg-amber-900/40 text-amber-300",
};

export interface HistoricalDataCollectionJobSummaryView {
  id: string;
  name: string;
  status: number;
  statusName: string;
  totalCombinations: number;
  completedCombinations: number;
  totalCandlesCollected: number;
  progressPercent: number;
  currentIsin: string | null;
  currentTimeFrame: number | null;
  startedAt: string | null;
  completedAt: string | null;
  durationMs: number | null;
  errorMessage: string | null;
  createdAt: string;
}

export interface HistoricalDataCollectionJobDetailView
  extends HistoricalDataCollectionJobSummaryView {
  isins: string[];
  timeFrames: number[];
  dateFrom: string;
  dateTo: string;
}

export interface CollectHistoricalDataCommand {
  name: string;
  isins: string[];
  timeFrames: number[];
  dateFrom: string;
  dateTo: string;
}

/* ─── TimeFrame helpers ─── */
export const COLLECTION_TIMEFRAME_OPTIONS = [
  { value: 1, label: "M1" },
  { value: 5, label: "M5" },
  { value: 15, label: "M15" },
  { value: 30, label: "M30" },
  { value: 60, label: "H1" },
  { value: 1440, label: "D1" },
  { value: 10080, label: "W1" },
];

export const TIMEFRAME_LABEL_MAP: Record<number, string> = {
  1: "M1",
  5: "M5",
  15: "M15",
  30: "M30",
  60: "H1",
  240: "H4",
  1440: "D1",
  10080: "W1",
};