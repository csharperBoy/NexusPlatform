import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";
import type { StateStorage } from "zustand/middleware";
import { storageAdapter } from "@/core/storage/storageAdapter";
import { uid, todayDateKey, logTimestamp } from "../utils";
import type {
  ScheduledOrder,
  SchedulePlan,
  PlanRuntimeState,
  SchedulerRuntime,
  SchedulerStatus,
  ExportedSchedule,
} from "../models";
import { PlanLogEntry } from "../models/ScheduledOrder";

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

const emptyPlanState = (): PlanRuntimeState => ({
  lastLoginDate: null,
  lastRefreshDate: null,
  firedOrderIds: [],
  clockSyncedForOrderId: null,
  logs: [],
});

const initialRuntime = (): SchedulerRuntime => ({
  currentDate: null,
  planStates: {},
  status: "idle",
  message: "",
  planMessages: {},
});

interface ScheduleState {
  enabled: boolean;
  plans: SchedulePlan[];
  runtime: SchedulerRuntime;

  /* config */
  setEnabled: (v: boolean) => void;

  /* plans */
  addPlan: (date?: string, name?: string) => SchedulePlan;
  updatePlan: (id: string, patch: Partial<SchedulePlan>) => void;
  removePlan: (id: string) => void;
  duplicatePlan: (id: string, newDate: string) => void;
  toggleCollapsed: (id: string) => void;

  /* orders */
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
  resetRuntime: () => void;
  resetForDay: (date: string) => void;
  setStatus: (status: SchedulerStatus, message: string) => void;
  setPlanMessage: (planId: string, message: string) => void;
  getPlanState: (planId: string) => PlanRuntimeState;
  setPlanState: (planId: string, patch: Partial<PlanRuntimeState>) => void;
  markOrderFired: (planId: string, orderId: string) => void;
   appendPlanLog: (planId: string, msg: string, type: PlanLogEntry["type"]) => void;
  clearPlanLogs: (planId: string) => void;
}

export const useScheduleStore = create<ScheduleState>()(
  persist(
    (set, get) => ({
      enabled: false,
      plans: [],
      runtime: initialRuntime(),

      setEnabled: (v) =>
        set((s) => ({
          enabled: v,
          runtime: v ? initialRuntime() : s.runtime,
        })),

      /* ───── Plans ───── */
      addPlan: (date, name = "برنامه جدید") => {
        const p: SchedulePlan = {
          id: uid(),
          name,
          date: date ?? todayDateKey(),
          enabled: true,
          autoLoginAt: DEFAULT_LOGIN_AT,
          autoRefreshAt: DEFAULT_REFRESH_AT,
          collapsed: false,
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
        set((s) => {
          const nextRuntime = { ...s.runtime };
          delete nextRuntime.planStates[id];
          delete nextRuntime.planMessages[id];
          return {
            plans: s.plans.filter((p) => p.id !== id),
            runtime: nextRuntime,
          };
        }),

      duplicatePlan: (id, newDate) =>
        set((s) => {
          const src = s.plans.find((p) => p.id === id);
          if (!src) return s;
          const copy: SchedulePlan = {
            ...src,
            id: uid(),
            name: `${src.name} (کپی)`,
            date: newDate,
            collapsed: false,
            orders: src.orders.map((o) => ({ ...o, id: uid() })),
          };
          return { plans: [...s.plans, copy] };
        }),

      toggleCollapsed: (id) =>
        set((s) => ({
          plans: s.plans.map((p) =>
            p.id === id ? { ...p, collapsed: !p.collapsed } : p,
          ),
        })),

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
          version: 2,
          exportedAt: new Date().toISOString(),
          enabled: s.enabled,
          plans: s.plans.map((p) => ({
            name: p.name,
            date: p.date,
            enabled: p.enabled,
            autoLoginAt: p.autoLoginAt,
            autoRefreshAt: p.autoRefreshAt,
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
          if (data.version !== 2) {
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
            autoLoginAt: p.autoLoginAt || DEFAULT_LOGIN_AT,
            autoRefreshAt: p.autoRefreshAt || DEFAULT_REFRESH_AT,
            collapsed: true,
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
      resetRuntime: () => set({ runtime: initialRuntime() }),

      resetForDay: (date) =>
        set({
          runtime: {
            currentDate: date,
            planStates: {},
            status: "waiting",
            message: "",
            planMessages: {},
          },
        }),

      setStatus: (status, message) =>
        set((s) => ({ runtime: { ...s.runtime, status, message } })),

      setPlanMessage: (planId, message) =>
        set((s) => ({
          runtime: {
            ...s.runtime,
            planMessages: { ...s.runtime.planMessages, [planId]: message },
          },
        })),

      getPlanState: (planId) =>
        get().runtime.planStates[planId] ?? emptyPlanState(),

      setPlanState: (planId, patch) =>
        set((s) => {
          const current = s.runtime.planStates[planId] ?? emptyPlanState();
          return {
            runtime: {
              ...s.runtime,
              planStates: {
                ...s.runtime.planStates,
                [planId]: { ...current, ...patch },
              },
            },
          };
        }),
      appendPlanLog: (planId, msg, type) =>
        set((s) => {
          const current = s.runtime.planStates[planId] ?? emptyPlanState();
          const newLog: PlanLogEntry = {
            t: logTimestamp(),
            msg,
            type,
          };
          return {
            runtime: {
              ...s.runtime,
              planStates: {
                ...s.runtime.planStates,
                [planId]: {
                  ...current,
                  logs: [...current.logs, newLog].slice(-200),
                },
              },
            },
          };
        }),

      clearPlanLogs: (planId) =>
        set((s) => {
          const current = s.runtime.planStates[planId] ?? emptyPlanState();
          return {
            runtime: {
              ...s.runtime,
              planStates: {
                ...s.runtime.planStates,
                [planId]: { ...current, logs: [] },
              },
            },
          };
        }),
      markOrderFired: (planId, orderId) =>
        set((s) => {
          const current = s.runtime.planStates[planId] ?? emptyPlanState();
          if (current.firedOrderIds.includes(orderId)) return s;
          return {
            runtime: {
              ...s.runtime,
              planStates: {
                ...s.runtime.planStates,
                [planId]: {
                  ...current,
                  firedOrderIds: [...current.firedOrderIds, orderId],
                },
              },
            },
          };
        }),
    }),
    {
      name: "trader:schedule:v3",
      storage: createJSONStorage(() => asyncStorage),
      partialize: (s) => ({
        enabled: s.enabled,
        plans: s.plans,
        runtime: s.runtime,
      }),
      merge: (persisted, current) => {
        const p = (persisted ?? {}) as Partial<ScheduleState>;
        return {
          ...current,
          ...p,
          runtime: p.runtime ?? initialRuntime(),
        };
      },
    },
  ),
);