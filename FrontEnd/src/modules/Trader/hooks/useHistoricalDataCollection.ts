import { useCallback, useEffect, useMemo, useState } from "react";
import { historicalDataCollectionApi } from "../api/historicalDataCollectionApi";
import type { HistoricalDataCollectionJobSummaryView } from "../models";
import { CollectionJobStatus } from "../models";

export function useHistoricalDataCollection() {
  const [jobs, setJobs] = useState<HistoricalDataCollectionJobSummaryView[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await historicalDataCollectionApi.GetList();
      setJobs(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در دریافت jobها");
      setJobs([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  // Poll if any job is pending/running
  const hasActive = useMemo(
    () =>
      jobs.some(
        (j) =>
          j.status === CollectionJobStatus.Pending ||
          j.status === CollectionJobStatus.Running,
      ),
    [jobs],
  );

  useEffect(() => {
    if (!hasActive) return;
    const t = setInterval(() => void load(), 5000);
    return () => clearInterval(t);
  }, [hasActive, load]);

  const handleCancel = useCallback(
    async (id: string) => {
      try {
        await historicalDataCollectionApi.Cancel(id);
        await load();
      } catch (e) {
        alert(e instanceof Error ? e.message : "خطا در لغو");
      }
    },
    [load],
  );

  return {
    jobs,
    loading,
    error,
    hasActive,
    refresh: load,
    handleCancel,
  };
}