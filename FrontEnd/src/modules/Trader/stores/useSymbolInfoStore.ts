import { create } from "zustand";
import { fetchSymbolInfo } from "../api";
import type { SymbolInfo } from "../models";

const TTL_MS = 5 * 60 * 1000; // ۵ دقیقه

interface SymbolInfoState {
  cache: Record<string, SymbolInfo>;
  loading: Record<string, boolean>;
  errors: Record<string, string | null>;

  getInfo: (isin: string) => SymbolInfo | null;
  isLoading: (isin: string) => boolean;
  getError: (isin: string) => string | null;
  ensureInfo: (
    isin: string,
    token: string,
    force?: boolean,
  ) => Promise<SymbolInfo | null>;
  clearAll: () => void;
}

export const useSymbolInfoStore = create<SymbolInfoState>()((set, get) => ({
  cache: {},
  loading: {},
  errors: {},

  getInfo: (isin) => {
    const c = get().cache[isin];
    if (!c) return null;
    if (Date.now() - c.fetchedAt > TTL_MS) return null; // منقضی
    return c;
  },

  isLoading: (isin) => !!get().loading[isin],
  getError: (isin) => get().errors[isin] ?? null,

  ensureInfo: async (isin, token, force = false) => {
    if (!isin || !token?.trim()) return null;

    /* cache تازه؟ */
    if (!force) {
      const c = get().cache[isin];
      if (c && Date.now() - c.fetchedAt < TTL_MS) return c;
    }

    /* در حال fetch؟ */
    if (get().loading[isin]) return null;

    set((s) => ({
      loading: { ...s.loading, [isin]: true },
      errors: { ...s.errors, [isin]: null },
    }));

    try {
      const info = await fetchSymbolInfo(token, isin);
      set((s) => ({
        cache: { ...s.cache, [isin]: info },
        loading: { ...s.loading, [isin]: false },
        errors: { ...s.errors, [isin]: null },
      }));
      return info;
    } catch (e) {
      const msg = e instanceof Error ? e.message : "خطای نامشخص";
      set((s) => ({
        loading: { ...s.loading, [isin]: false },
        errors: { ...s.errors, [isin]: msg },
      }));
      return null;
    }
  },

  clearAll: () => set({ cache: {}, loading: {}, errors: {} }),
}));