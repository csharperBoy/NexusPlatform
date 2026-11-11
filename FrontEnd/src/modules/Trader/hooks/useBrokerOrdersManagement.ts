import { useCallback, useEffect, useState } from "react";
import { brokerOrderApi } from "../api/ordersApi";
import type { BrokerOrderQuery, BrokerOrderView } from "../models";

const PAGE_SIZE = 100;

export function useBrokerOrdersManagement() {
  const [orders, setOrders] = useState<BrokerOrderView[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [filters, setFiltersState] = useState<BrokerOrderQuery>({
    isin: "",
    state: null,
    isPaper: null,
    search: "",
    take: PAGE_SIZE,
  });

  const load = useCallback(async (query: BrokerOrderQuery) => {
    setLoading(true);
    setError(null);
    try {
      const result = await brokerOrderApi.Query({ ...query, skip: 0 });
      setOrders(result.items);
      setTotalCount(result.totalRecords);
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در دریافت سفارش‌ها");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load(filters);
  }, [load, filters]);

  const loadMore = useCallback(async () => {
    if (loadingMore || orders.length >= totalCount) return;
    setLoadingMore(true);
    try {
      const result = await brokerOrderApi.Query({
        ...filters,
        skip: orders.length,
        take: PAGE_SIZE,
      });
      setOrders((prev) => [...prev, ...result.items]);
      setTotalCount(result.totalRecords);
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در بارگذاری بیشتر");
    } finally {
      setLoadingMore(false);
    }
  }, [filters, orders.length, totalCount, loadingMore]);

  const setFilters = useCallback(
    (patch: Partial<BrokerOrderQuery>) => {
      setFiltersState((prev) => ({ ...prev, ...patch }));
    },
    [],
  );

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
    orders,
    totalCount,
    loading,
    loadingMore,
    error,
    filters,
    setFilters,
    resetFilters,
    refresh,
    loadMore,
    canLoadMore: orders.length < totalCount,
  };
}