export interface ServerClockSample {
  offset: number;
  rtt: number;
  serverTimestamp: number;
  rawDiff: number;
  ts: number;
}

export interface ServerClockInfo {
  samples: number;
  lastRtt: number;
  lastSync: Date | null;
  offset: number;
  diff: number;
  min: number;
  max: number;
  lastError: string | null;
  busy: boolean;
}