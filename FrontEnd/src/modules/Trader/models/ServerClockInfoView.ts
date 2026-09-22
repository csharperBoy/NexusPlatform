export interface ServerClockInfoView {
  diff: number;
  offset: number;
  oneWayLatency: number;
  lastUpdatedAt: number;
  samplesCount: number;
  lastError?: string | null;
}

export interface SyncServerClockCommand {
  samples: number;
}