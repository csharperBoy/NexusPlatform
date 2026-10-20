import { useCallback, useEffect, useState } from "react";
import { tradingRunApi } from "../api/tradingRunApi";
import type {
  TradingRunInfoView,
  CreateTradingRunCommand,
  UpdateTradingRunCommand,
} from "../models";
import { StopRunBehavior } from "../models";

export function useTradingRunManagement() {
  const [runs, setRuns] = useState<TradingRunInfoView[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await tradingRunApi.GetList();
      setRuns(data);
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
    async (cmd: CreateTradingRunCommand): Promise<string> => {
      const id = await tradingRunApi.create(cmd);
      await load();
      return id;
    },
    [load],
  );

  const handleUpdate = useCallback(
    async (cmd: UpdateTradingRunCommand): Promise<void> => {
      await tradingRunApi.update(cmd);
      await load();
    },
    [load],
  );

  const handleStart = useCallback(
    async (id: string): Promise<void> => {
      setBusyId(id);
      try {
        await tradingRunApi.start(id);
        await load();
      } finally {
        setBusyId(null);
      }
    },
    [load],
  );

  const handleStop = useCallback(
    async (id: string, behavior: StopRunBehavior): Promise<void> => {
      setBusyId(id);
      try {
        await tradingRunApi.stop(id, behavior);
        await load();
      } finally {
        setBusyId(null);
      }
    },
    [load],
  );

  const handleDelete = useCallback(
    async (id: string): Promise<void> => {
      setBusyId(id);
      try {
        await tradingRunApi.delete(id);
        await load();
      } finally {
        setBusyId(null);
      }
    },
    [load],
  );

  return {
    runs,
    loading,
    busyId,
    error,
    refresh: load,
    handleCreate,
    handleUpdate,
    handleStart,
    handleStop,
    handleDelete,
  };
}