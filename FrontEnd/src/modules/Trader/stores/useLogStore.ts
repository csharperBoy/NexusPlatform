import { create } from "zustand";
import { logTimestamp } from "../utils";

export type LogType = "ok" | "err" | "info" | "send";

export interface LogEntry {
  t: string;
  msg: string;
  type: LogType;
}

interface LogState {
  entries: LogEntry[];
  append: (msg: string, type?: LogType) => void;
  clear: () => void;
}

const MAX_ENTRIES = 200;

export const useLogStore = create<LogState>()((set) => ({
  entries: [],
  append: (msg, type = "info") =>
    set((s) => ({
      entries: [...s.entries, { t: logTimestamp(), msg, type }].slice(
        -MAX_ENTRIES,
      ),
    })),
  clear: () => set({ entries: [] }),
}));