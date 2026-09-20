import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";
import type { StateStorage } from "zustand/middleware";
import { storageAdapter } from "@/core/storage/storageAdapter";
import type { Symbol } from "../models";

const DEFAULT_SYMBOLS: Symbol[] = [
  {
    symbolName: "لبن",
    symbolIsin: "IRO3LABN0001",
    price: 11380,
    quantity: 50000,
    side: 0,
    validityType: 0,
    commission: 0.003632,
    orderModelType: 1,
    orderFrom: 34,
  },
  {
    symbolName: "داروند",
    symbolIsin: "IRO1DRVN0001",
    price: 6040,
    quantity: 300000,
    side: 0,
    validityType: 0,
    commission: 0.003712,
    orderModelType: 1,
    orderFrom: 34,
  },
  {
    symbolName: "غزنجان",
    symbolIsin: "IRO1ZNJN0001",
    price: 7130,
    quantity: 300000,
    side: 0,
    validityType: 0,
    commission: 0.003712,
    orderModelType: 1,
    orderFrom: 34,
  },
];

const asyncStorage: StateStorage = {
  getItem: (name) => storageAdapter.getItem(name),
  setItem: async (name, value) => {
    await storageAdapter.setItem(name, value);
  },
  removeItem: async (name) => {
    await storageAdapter.removeItem(name);
  },
};

interface SymbolsState {
  symbols: Symbol[];
  getSymbol: (isin: string) => Symbol | undefined;
  addSymbol: (s: Symbol) => void;
  addSymbols: (list: Symbol[]) => void;
  updateSymbol: (isin: string, patch: Partial<Symbol>) => void;
  removeSymbol: (isin: string) => void;
  clearAll: () => void;
  resetToDefaults: () => void;
}

export const useSymbolsStore = create<SymbolsState>()(
  persist(
    (set, get) => ({
      symbols: DEFAULT_SYMBOLS,

      getSymbol: (isin) => get().symbols.find((s) => s.symbolIsin === isin),

      addSymbol: (s) =>
        set((state) => {
          const idx = state.symbols.findIndex(
            (x) => x.symbolIsin === s.symbolIsin,
          );
          if (idx >= 0) {
            const copy = [...state.symbols];
            copy[idx] = { ...copy[idx], ...s };
            return { symbols: copy };
          }
          return { symbols: [...state.symbols, s] };
        }),

      addSymbols: (list) =>
        set((state) => {
          const copy = [...state.symbols];
          for (const s of list) {
            const idx = copy.findIndex((x) => x.symbolIsin === s.symbolIsin);
            if (idx >= 0) copy[idx] = { ...copy[idx], ...s };
            else copy.push(s);
          }
          return { symbols: copy };
        }),

      updateSymbol: (isin, patch) =>
        set((state) => ({
          symbols: state.symbols.map((x) =>
            x.symbolIsin === isin ? { ...x, ...patch } : x,
          ),
        })),

      removeSymbol: (isin) =>
        set((state) => ({
          symbols: state.symbols.filter((x) => x.symbolIsin !== isin),
        })),

      clearAll: () => set({ symbols: [] }),

      resetToDefaults: () => set({ symbols: DEFAULT_SYMBOLS }),
    }),
    {
      name: "trader:symbols:v1",
      storage: createJSONStorage(() => asyncStorage),
    },
  ),
);