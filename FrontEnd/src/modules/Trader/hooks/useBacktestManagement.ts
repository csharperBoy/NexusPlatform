import { useCallback, useEffect, useState } from "react";
import { backtestApi } from "../api/backtestApi";
import type { BacktestRunSummaryView } from "../models";

export function useBacktestManagement(sourceRunId?: string) {
  const [runs, setRuns] = useState<BacktestRunSummaryView[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await backtestApi.GetList(sourceRunId);
      setRuns(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در دریافت بکتست‌ها");
      setRuns([]);
    } finally {
      setLoading(false);
    }
  }, [sourceRunId]);

  useEffect(() => {
    void load();
  }, [load]);

  const handleCancel = useCallback(
    async (id: string) => {
      try {
        await backtestApi.Cancel(id);
        await load();
      } catch (e) {
        alert(e instanceof Error ? e.message : "خطا در لغو");
      }
    },
    [load],
  );

  return { runs, loading, error, refresh: load, handleCancel };
}