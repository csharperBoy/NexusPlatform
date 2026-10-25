import { useCallback, useEffect, useState } from "react";
import { currencyRateApi } from "../api/currencyRateApi";
import type { CurrencyRateView, CreateCurrencyRateCommand } from "../models";

export function useCurrencyRateManagement() {
  const [rates, setRates] = useState<CurrencyRateView[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(
    async (fromCurrency?: string, toCurrency?: string) => {
      setLoading(true);
      setError(null);
      try {
        const data = await currencyRateApi.GetList(fromCurrency, toCurrency);
        setRates(data);
      } catch (e) {
        setError(e instanceof Error ? e.message : "خطا در دریافت نرخ‌ها");
      } finally {
        setLoading(false);
      }
    },
    [],
  );

  useEffect(() => {
    void load();
  }, [load]);

  const handleCreate = useCallback(
    async (cmd: CreateCurrencyRateCommand): Promise<string> => {
      const id = await currencyRateApi.create(cmd);
      await load();
      return id;
    },
    [load],
  );

  return {
    rates,
    loading,
    error,
    refresh: load,
    handleCreate,
  };
}