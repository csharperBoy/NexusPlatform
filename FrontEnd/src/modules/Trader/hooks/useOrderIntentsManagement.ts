import { useCallback, useEffect, useState } from "react";
import { orderIntentApi } from "../api/ordersApi";
import type { OrderIntentQuery, OrderIntentView } from "../models";

const PAGE_SIZE = 100;

export function useOrderIntentsManagement() {
  const [intents, setIntents] = useState<OrderIntentView[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [filters, setFiltersState] = useState<OrderIntentQuery>({
    isin: "",
    state: null,
    isPaper: null,
    search: "",
    take: PAGE_SIZE,
  });

  const load = useCallback(async (query: OrderIntentQuery) => {
    setLoading(true);
    setError(null);
    try {
      const result = await orderIntentApi.Query({ ...query, skip: 0 });
      setIntents(result.items);
      setTotalCount(result.totalRecords);
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در دریافت تصمیم‌ها");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load(filters);
  }, [load, filters]);

  const loadMore = useCallback(async () => {
    if (loadingMore || intents.length >= totalCount) return;
    setLoadingMore(true);
    try {
      const result = await orderIntentApi.Query({
        ...filters,
        skip: intents.length,
        take: PAGE_SIZE,
      });
      setIntents((prev) => [...prev, ...result.items]);
      setTotalCount(result.totalRecords);
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در بارگذاری بیشتر");
    } finally {
      setLoadingMore(false);
    }
  }, [filters, intents.length, totalCount, loadingMore]);

  const setFilters = useCallback((patch: Partial<OrderIntentQuery>) => {
    setFiltersState((prev) => ({ ...prev, ...patch }));
  }, []);

  const resetFilters = useCallback(() => {
    setFiltersState({
      isin: "",
      state: null,
      isPaper: null,
      search: "",
      take: PAGE_SIZE,
    });
  }, []);

  const refresh = useCallback(() => load(filters), [load, filters]);

  return {
    intents,
    totalCount,
    loading,
    loadingMore,
    error,
    filters,
    setFilters,
    resetFilters,
    refresh,
    loadMore,
    canLoadMore: intents.length < totalCount,
  };
}