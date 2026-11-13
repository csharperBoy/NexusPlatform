import { useCallback, useEffect, useState } from "react";
import { dashboardApi } from "../api/dashboardApi";
import type { DashboardSummaryView } from "../models";

const AUTO_REFRESH_MS = 15_000;

export function useDashboard() {
  const [data, setData] = useState<DashboardSummaryView | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastRefreshAt, setLastRefreshAt] = useState<Date | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const summary = await dashboardApi.GetSummary();
      setData(summary);
      setLastRefreshAt(new Date());
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در دریافت داشبورد");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  useEffect(() => {
    const interval = setInterval(() => {
      void load();
    }, AUTO_REFRESH_MS);
    return () => clearInterval(interval);
  }, [load]);

  return { data, loading, error, refresh: load, lastRefreshAt };
}