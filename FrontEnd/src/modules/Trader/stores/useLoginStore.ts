import { create } from "zustand";
import { useAccountsStore, getTokenStatus } from "./useAccountsStore";
import { useLoginLogStore } from "./useLoginLogStore";
import { loginToEasyTrader } from "../api/authServerApi";

interface LoginState {
  /** حساب‌هایی که در حال لاگین هستن */
  busyIds: Set<string>;
  /** آخرین خطا */
  lastError: string | null;

  loginAccount: (id: string, opts?: { silent?: boolean }) => Promise<string | null>;
  loginAll: (opts?: { silent?: boolean }) => Promise<{ id: string; ok: boolean }[]>;
  isBusy: (id: string) => boolean;
}

export const useLoginStore = create<LoginState>()((set, get) => ({
  busyIds: new Set(),
  lastError: null,

  isBusy: (id) => get().busyIds.has(id),

  loginAccount: async (id, { silent = false } = {}) => {
    if (get().busyIds.has(id)) return null;
    const accountsStore = useAccountsStore.getState();
    const acc = accountsStore.getAccount(id);
    if (!acc) return null;
    if (!acc.username.trim() || !acc.password.trim()) {
      if (!silent) useLoginLogStore.getState().append(
        `[${acc.name}] نام کاربری یا رمز خالیه`,
        "err",
        id,
      );
      return null;
    }

    set((s) => ({ busyIds: new Set(s.busyIds).add(id) }));
    if (!silent)
      useLoginLogStore.getState().append(`[${acc.name}] در حال لاگین...`, "info", id);

    try {
      const token = await loginToEasyTrader(acc.username, acc.password);
      accountsStore.setToken(id, token);
      set({ lastError: null });
      useLoginLogStore.getState().append(`[${acc.name}] ✅ لاگین موفق`, "ok", id);
      return token;
    } catch (e) {
      const msg = e instanceof Error ? e.message : "خطای نامشخص";
      set({ lastError: msg });
      useLoginLogStore.getState().append(`[${acc.name}] ❌ ${msg}`, "err", id);
      return null;
    } finally {
      set((s) => {
        const next = new Set(s.busyIds);
        next.delete(id);
        return { busyIds: next };
      });
    }
  },

  loginAll: async (opts = {}) => {
    const accounts = useAccountsStore.getState().accounts;
    const results: { id: string; ok: boolean }[] = [];
    for (const acc of accounts) {
      const t = await get().loginAccount(acc.id, opts);
      results.push({ id: acc.id, ok: !!t });
    }
    return results;
  },
}));