import { useQuery } from "@tanstack/react-query";
import { dashboardApi } from "../api/dashboardApi";
import type { DashboardSummaryView } from "../models";

export const DASHBOARD_SUMMARY_KEY = ["trader", "dashboard", "summary"] as const;

const AUTO_REFRESH_MS = 15_000;

/**
 * Shared via TanStack Query across all 6 Trader widgets.
 * Each widget calls useDashboardSummary() independently — React Query
 * deduplicates to ONE HTTP call per 15s interval, regardless of how many
 * widgets are mounted.
 */
export function useDashboardSummary() {
  return useQuery<DashboardSummaryView>({
    queryKey: DASHBOARD_SUMMARY_KEY,
    queryFn: () => dashboardApi.GetSummary(),
    refetchInterval: AUTO_REFRESH_MS,
    staleTime: 10_000,
    retry: 2,
  });
}