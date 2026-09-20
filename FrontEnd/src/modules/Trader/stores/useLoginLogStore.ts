import { create } from "zustand";
import { logTimestamp } from "../utils";

export type LogType = "ok" | "err" | "info" | "send";

export interface LogEntry {
  t: string;
  msg: string;
  type: LogType;
  accountId?: string;
}

interface LoginLogState {
  entries: LogEntry[];
  append: (msg: string, type?: LogType, accountId?: string) => void;
  clear: () => void;
}

const MAX_ENTRIES = 50;

export const useLoginLogStore = create<LoginLogState>()((set) => ({
  entries: [],
  append: (msg, type = "info", accountId) =>
    set((s) => ({
      entries: [
        ...s.entries,
        { t: logTimestamp(), msg, type, accountId },
      ].slice(-MAX_ENTRIES),
    })),
  clear: () => set({ entries: [] }),
}));