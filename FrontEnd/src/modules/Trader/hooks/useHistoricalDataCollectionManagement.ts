import { useCallback, useEffect, useState } from "react";
import { historicalDataCollectionApi } from "../api/historicalDataCollectionApi";
import type {
  CreateHistoricalBackfillCommand,
  CreateHistoricalRecurringCommand,
  HistoricalDataCollectionJobSummaryView,
} from "../models";
import { HistoricalJobState } from "../models";

export function useHistoricalDataCollectionManagement() {
  const [jobs, setJobs] = useState<HistoricalDataCollectionJobSummaryView[]>([]);
  const [loading, setLoading] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await historicalDataCollectionApi.GetList();
      setJobs(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در دریافت لیست");
    } finally {
      setLoading(false);
    }
  }, []);

  /* Initial load */
  useEffect(() => {
    void load();
  }, [load]);

  /* Auto-poll while any job is Running (for Backfill progress) */
  useEffect(() => {
    const hasRunning = jobs.some(
      (j) => j.state === HistoricalJobState.Running,
    );
    if (!hasRunning) return;

    const interval = setInterval(() => {
      void load();
    }, 5000);
    return () => clearInterval(interval);
  }, [jobs, load]);

  /* ─── Commands ─── */

  const handleCreateBackfill = useCallback(
    async (cmd: CreateHistoricalBackfillCommand): Promise<string> => {
      const id = await historicalDataCollectionApi.CreateBackfill(cmd);
      await load();
      return id;
    },
    [load],
  );

  const handleCreateRecurring = useCallback(
    async (cmd: CreateHistoricalRecurringCommand): Promise<string> => {
      const id = await historicalDataCollectionApi.CreateRecurring(cmd);
      await load();
      return id;
    },
    [load],
  );

  const handleStart = useCallback(
    async (id: string): Promise<void> => {
      setBusyId(id);
      try {
        await historicalDataCollectionApi.Start(id);
        await load();
      } finally {
        setBusyId(null);
      }
    },
    [load],
  );

  const handleStop = useCallback(
    async (id: string): Promise<void> => {
      setBusyId(id);
      try {
        await historicalDataCollectionApi.Stop(id);
        await load();
      } finally {
        setBusyId(null);
      }
    },
    [load],
  );

  return {
    jobs,
    loading,
    busyId,
    error,
    refresh: load,
    handleCreateBackfill,
    handleCreateRecurring,
    handleStart,
    handleStop,
  };
}