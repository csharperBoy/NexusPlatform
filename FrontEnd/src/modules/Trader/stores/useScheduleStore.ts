import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";
import type { StateStorage } from "zustand/middleware";
import { storageAdapter } from "@/core/storage/storageAdapter";
import { uid, todayDateKey } from "../utils";
import type {
  ScheduledOrder,
  SchedulePlan,
  SchedulerRuntime,
  ExportedSchedule,
} from "../models";

const DEFAULT_LOGIN_AT = "08:30:00";
const DEFAULT_REFRESH_AT = "08:40:00";

const asyncStorage: StateStorage = {
  getItem: (name) => storageAdapter.getItem(name),
  setItem: async (name, value) => {
    await storageAdapter.setItem(name, value);
  },
  removeItem: async (name) => {
    await storageAdapter.removeItem(name);
  },
};

const initialRuntime: SchedulerRuntime = {
  status: "idle",
  message: "",
  currentDate: null,
  lastLoginDate: null,
  lastRefreshDate: null,
  firedKeys: [],
  lastLoginAt: null,
  lastRefreshAt: null,
};

interface ScheduleState {
  enabled: boolean;
  autoLoginAt: string;
  autoRefreshAt: string;
  plans: SchedulePlan[];
  runtime: SchedulerRuntime;

  /* config */
  setEnabled: (v: boolean) => void;
  setAutoLoginAt: (v: string) => void;
  setAutoRefreshAt: (v: string) => void;

  /* plans */
  addPlan: (date?: string, name?: string) => SchedulePlan;
  updatePlan: (id: string, patch: Partial<SchedulePlan>) => void;
  removePlan: (id: string) => void;
  duplicatePlan: (id: string, newDate: string) => void;

  /* orders in plan */
  addOrder: (
    planId: string,
    symbolIsin?: string,
    accountId?: string,
  ) => ScheduledOrder | null;
  updateOrder: (
    planId: string,
    orderId: string,
    patch: Partial<ScheduledOrder>,
  ) => void;
  removeOrder: (planId: string, orderId: string) => void;

  /* json */
  exportAll: () => string;
  importAll: (json: string) => { ok: boolean; error?: string; count?: number };

  /* runtime */
  setRuntime: (patch: Partial<SchedulerRuntime>) => void;
  resetRuntime: () => void;
  markFired: (planId: string, orderId: string) => void;
}

export const useScheduleStore = create<ScheduleState>()(
  persist(
    (set, get) => ({
      enabled: false,
      autoLoginAt: DEFAULT_LOGIN_AT,
      autoRefreshAt: DEFAULT_REFRESH_AT,
      plans: [],
      runtime: initialRuntime,

      setEnabled: (v) =>
        set((s) => ({
          enabled: v,
          runtime: v
            ? initialRuntime
            : { ...s.runtime, status: "idle", message: "" },
        })),

      setAutoLoginAt: (v) => set({ autoLoginAt: v }),
      setAutoRefreshAt: (v) => set({ autoRefreshAt: v }),

      /* ───── Plans ───── */
      addPlan: (date, name = "برنامه جدید") => {
        const p: SchedulePlan = {
          id: uid(),
          name,
          date: date ?? todayDateKey(),
          enabled: true,
          orders: [],
        };
        set((s) => ({ plans: [...s.plans, p] }));
        return p;
      },

      updatePlan: (id, patch) =>
        set((s) => ({
          plans: s.plans.map((p) => (p.id === id ? { ...p, ...patch } : p)),
        })),

      removePlan: (id) =>
        set((s) => ({ plans: s.plans.filter((p) => p.id !== id) })),

      duplicatePlan: (id, newDate) =>
        set((s) => {
          const src = s.plans.find((p) => p.id === id);
          if (!src) return s;
          const copy: SchedulePlan = {
            id: uid(),
            name: `${src.name} (کپی)`,
            date: newDate,
            enabled: src.enabled,
            orders: src.orders.map((o) => ({ ...o, id: uid() })),
          };
          return { plans: [...s.plans, copy] };
        }),

      /* ───── Orders ───── */
      addOrder: (planId, symbolIsin = "", accountId = "") => {
        const o: ScheduledOrder = {
          id: uid(),
          accountId,
          symbolIsin,
          side: 0,
          mode: "quantity",
          quantity: "100",
          totalValue: "10000000",
          time: "08:45:00.000",
        };
        set((s) => ({
          plans: s.plans.map((p) =>
            p.id === planId ? { ...p, orders: [...p.orders, o] } : p,
          ),
        }));
        return o;
      },

      updateOrder: (planId, orderId, patch) =>
        set((s) => ({
          plans: s.plans.map((p) =>
            p.id === planId
              ? {
                  ...p,
                  orders: p.orders.map((o) =>
                    o.id === orderId ? { ...o, ...patch } : o,
                  ),
                }
              : p,
          ),
        })),

      removeOrder: (planId, orderId) =>
        set((s) => ({
          plans: s.plans.map((p) =>
            p.id === planId
              ? { ...p, orders: p.orders.filter((o) => o.id !== orderId) }
              : p,
          ),
        })),

      /* ───── JSON ───── */
      exportAll: () => {
        const s = get();
        const data: ExportedSchedule = {
          version: 1,
          exportedAt: new Date().toISOString(),
          enabled: s.enabled,
          autoLoginAt: s.autoLoginAt,
          autoRefreshAt: s.autoRefreshAt,
          plans: s.plans.map((p) => ({
            name: p.name,
            date: p.date,
            enabled: p.enabled,
            orders: p.orders.map((o) => ({
              accountId: o.accountId,
              symbolIsin: o.symbolIsin,
              side: o.side,
              mode: o.mode,
              quantity: o.quantity,
              totalValue: o.totalValue,
              time: o.time,
            })),
          })),
        };
        return JSON.stringify(data, null, 2);
      },

      importAll: (json) => {
        try {
          const data = JSON.parse(json) as ExportedSchedule;
          if (data.version !== 1) {
            return { ok: false, error: `نسخه نامعتبر: ${data.version}` };
          }
          if (!Array.isArray(data.plans)) {
            return { ok: false, error: "plans آرایه نیست" };
          }

          const newPlans: SchedulePlan[] = data.plans.map((p) => ({
            id: uid(),
            name: p.name || "برنامه واردشده",
            date: p.date || todayDateKey(),
            enabled: p.enabled ?? true,
            orders: (p.orders || []).map((o) => ({
              id: uid(),
              accountId: o.accountId || "",
              symbolIsin: o.symbolIsin || "",
              side: o.side === 1 ? (1 as const) : (0 as const),
              mode: o.mode === "totalValue" ? "totalValue" : "quantity",
              quantity: String(o.quantity ?? "100"),
              totalValue: String(o.totalValue ?? "10000000"),
              time: o.time || "08:45:00.000",
            })),
          }));

          set((s) => ({
            enabled: data.enabled ?? s.enabled,
            autoLoginAt: data.autoLoginAt || s.autoLoginAt,
            autoRefreshAt: data.autoRefreshAt || s.autoRefreshAt,
            plans: [...s.plans, ...newPlans],
          }));

          return { ok: true, count: newPlans.length };
        } catch (e) {
          return {
            ok: false,
            error: e instanceof Error ? e.message : "خطای JSON",
          };
        }
      },

      /* ───── Runtime ───── */
      setRuntime: (patch) =>
        set((s) => ({ runtime: { ...s.runtime, ...patch } })),

      resetRuntime: () => set({ runtime: initialRuntime }),

      markFired: (planId, orderId) =>
        set((s) => {
          const key = `${planId}:${orderId}`;
          if (s.runtime.firedKeys.includes(key)) return s;
          return {
            runtime: {
              ...s.runtime,
              firedKeys: [...s.runtime.firedKeys, key],
            },
          };
        }),
    }),
    {
      name: "trader:schedule:v2",
      storage: createJSONStorage(() => asyncStorage),
      partialize: (s) => ({
        enabled: s.enabled,
        autoLoginAt: s.autoLoginAt,
        autoRefreshAt: s.autoRefreshAt,
        plans: s.plans,
        runtime: s.runtime,
      }),
      merge: (persisted, current) => {
        const p = (persisted ?? {}) as Partial<ScheduleState>;
        return {
          ...current,
          ...p,
          runtime: {
            ...initialRuntime,
            ...(p.runtime ?? {}),
            status: "idle",
            message: "",
          },
        };
      },
    },
  ),
);