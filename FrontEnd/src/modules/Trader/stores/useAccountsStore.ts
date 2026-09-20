import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";
import type { StateStorage } from "zustand/middleware";
import { storageAdapter } from "@/core/storage/storageAdapter";
import { uid } from "../utils";
import { obfuscate, deobfuscate } from "../utils/obfuscate";
import type { Account, TokenStatus } from "../models";
import { decodeJwtExp } from "../utils/jwt";

const DEFAULT_ACCOUNTS: Account[] = [
  {
    id: "acc-default-1",
    name: "حساب اصلی",
    username: "",
    password: "",
    token: "",
  },
];

/* ─── storage با obfuscation شفاف ─── */
const obfuscatingStorage: StateStorage = {
  getItem: async (name) => {
    const raw = await storageAdapter.getItem(name);
    if (!raw) return null;
    try {
      const parsed = JSON.parse(raw);
      if (Array.isArray(parsed?.state?.accounts)) {
        parsed.state.accounts = parsed.state.accounts.map(
          (a: Account) => ({
            ...a,
            password: a.password ? deobfuscate(a.password) : "",
          }),
        );
      }
      return JSON.stringify(parsed);
    } catch {
      return raw;
    }
  },
  setItem: async (name, value) => {
    try {
      const parsed = JSON.parse(value);
      if (Array.isArray(parsed?.state?.accounts)) {
        parsed.state.accounts = parsed.state.accounts.map(
          (a: Account) => ({
            ...a,
            password: a.password ? obfuscate(a.password) : "",
          }),
        );
      }
      await storageAdapter.setItem(name, JSON.stringify(parsed));
    } catch {
      await storageAdapter.setItem(name, value);
    }
  },
  removeItem: async (name) => {
    await storageAdapter.removeItem(name);
  },
};

/* ─── helper: token status ─── */
export function getTokenStatus(token: string | undefined): TokenStatus {
  if (!token?.trim()) return { state: "empty" };
  const exp = decodeJwtExp(token);
  if (!exp) return { state: "invalid" };
  const now = Date.now();
  if (exp < now) return { state: "expired", exp };
  return { state: "valid", exp, remainMs: exp - now };
}

interface AccountsState {
  accounts: Account[];
  autoLogin: boolean;

  /* CRUD */
  getAccount: (id: string | null) => Account | null;
  addAccount: (overrides?: Partial<Account>) => Account;
  updateAccount: (id: string, patch: Partial<Account>) => void;
  removeAccount: (id: string) => void;
  setToken: (id: string, token: string) => void;
  clearAllTokens: () => void;
  clearAllCredentials: () => void;
  resetToDefaults: () => void;
  setAutoLogin: (v: boolean) => void;
}

export const useAccountsStore = create<AccountsState>()(
  persist(
    (set, get) => ({
      accounts: DEFAULT_ACCOUNTS,
      autoLogin: false,

      getAccount: (id) =>
        id ? get().accounts.find((a) => a.id === id) ?? null : null,

      addAccount: (overrides = {}) => {
        const acc: Account = {
          id: uid(),
          name: `حساب ${get().accounts.length + 1}`,
          username: "",
          password: "",
          token: "",
          ...overrides,
        };
        set((s) => ({ accounts: [...s.accounts, acc] }));
        return acc;
      },

      updateAccount: (id, patch) =>
        set((s) => ({
          accounts: s.accounts.map((a) =>
            a.id === id ? { ...a, ...patch } : a,
          ),
        })),

      removeAccount: (id) =>
        set((s) => ({
          accounts:
            s.accounts.length > 1
              ? s.accounts.filter((a) => a.id !== id)
              : s.accounts,
        })),

      setToken: (id, token) =>
        set((s) => ({
          accounts: s.accounts.map((a) =>
            a.id === id ? { ...a, token } : a,
          ),
        })),

      clearAllTokens: () =>
        set((s) => ({
          accounts: s.accounts.map((a) => ({ ...a, token: "" })),
        })),

      clearAllCredentials: () =>
        set((s) => ({
          accounts: s.accounts.map((a) => ({
            ...a,
            username: "",
            password: "",
            token: "",
          })),
        })),

      resetToDefaults: () => set({ accounts: DEFAULT_ACCOUNTS }),

      setAutoLogin: (v) => set({ autoLogin: v }),
    }),
    {
      name: "trader:accounts:v1",
      storage: createJSONStorage(() => obfuscatingStorage),
      partialize: (state) => ({
        accounts: state.accounts,
        autoLogin: state.autoLogin,
      }),
    },
  ),
);

/* ─── selector: تعداد حساب‌های معتبر ─── */
export const selectValidCount = (state: AccountsState) =>
  state.accounts.filter((a) => getTokenStatus(a.token).state === "valid")
    .length;