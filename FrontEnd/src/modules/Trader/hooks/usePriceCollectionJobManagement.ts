import { useCallback, useEffect, useState } from "react";
import { priceCollectionJobApi } from "../api/priceCollectionApi";
import type {
  PriceCollectionJobView,
  CreatePriceCollectionJobCommand,
} from "../models";

export function usePriceCollectionJobManagement() {
  const [jobs, setJobs] = useState<PriceCollectionJobView[]>([]);
  const [loading, setLoading] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await priceCollectionJobApi.GetList();
      setJobs(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در دریافت لیست");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const handleCreate = useCallback(
    async (cmd: CreatePriceCollectionJobCommand): Promise<string> => {
      const id = await priceCollectionJobApi.create(cmd);
      await load();
      return id;
    },
    [load],
  );

  const handleStart = useCallback(
    async (id: string): Promise<void> => {
      setBusyId(id);
      try {
        await priceCollectionJobApi.start(id);
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
        await priceCollectionJobApi.stop(id);
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
    handleCreate,
    handleStart,
    handleStop,
  };
}