import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";
import type { StateStorage } from "zustand/middleware";
import { storageAdapter } from "@/core/storage/storageAdapter";

const asyncStorage: StateStorage = {
  getItem: (name) => storageAdapter.getItem(name),
  setItem: async (name, value) => {
    await storageAdapter.setItem(name, value);
  },
  removeItem: async (name) => {
    await storageAdapter.removeItem(name);
  },
};

export interface ServerClockState {
  offset: number;
  rtt: number;
  oneWayLatency: number;
  manualLeadTimeMs: number;
  lastSync: string | null;
  samplesCount: number;
  minOffset: number;
  maxOffset: number;
  busy: boolean;
  lastError: string | null;

  setManualLeadTimeMs: (ms: number) => void;
  setClockData: (
    data: Partial<{
      offset: number;
      rtt: number;
      samplesCount: number;
      minOffset: number;
      maxOffset: number;
      lastSync: string | null;
      busy: boolean;
      lastError: string | null;
    }>,
  ) => void;
  getTotalLeadTimeMs: () => number;
  getClientFireTime: (targetServerMs: number) => number;
}

export const useServerClockStore = create<ServerClockState>()(
  persist(
    (set, get) => ({
      offset: 0,
      rtt: 0,
      oneWayLatency: 0,
      manualLeadTimeMs: 0,
      lastSync: null,
      samplesCount: 0,
      minOffset: 0,
      maxOffset: 0,
      busy: false,
      lastError: null,

      setManualLeadTimeMs: (ms) => set({ manualLeadTimeMs: Math.max(0, ms) }),

      setClockData: (data) =>
        set((state) => {
          const nextRtt = data.rtt ?? state.rtt;
          return {
            ...state,
            ...data,
            rtt: nextRtt,
            oneWayLatency: Math.round(nextRtt / 2),
          };
        }),

      getTotalLeadTimeMs: () => {
        const { oneWayLatency, manualLeadTimeMs } = get();
        return oneWayLatency + manualLeadTimeMs;
      },

      getClientFireTime: (targetServerMs) => {
        const { offset, oneWayLatency, manualLeadTimeMs } = get();
        const totalLead = oneWayLatency + manualLeadTimeMs;
        /* clientFire = targetServerMs - offset - totalLead */
        return targetServerMs - offset - totalLead;
      },
    }),
    {
      name: "trader:server-clock:v1",
      storage: createJSONStorage(() => asyncStorage),
      partialize: (s) => ({
        offset: s.offset,
        rtt: s.rtt,
        oneWayLatency: s.oneWayLatency,
        manualLeadTimeMs: s.manualLeadTimeMs,
      }),
    },
  ),
);
