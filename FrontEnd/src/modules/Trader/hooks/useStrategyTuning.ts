// src/modules/Trader/hooks/useStrategyTuning.ts
import { useCallback, useEffect, useState } from "react";
import { strategyTuningApi } from "../api/strategyTuningApi";
import type {
  StrategyTuningRunSummaryView, StrategyTuningRunDetailView,
} from "../models";

export function useStrategyTuningList() {
  const [runs, setRuns] = useState<StrategyTuningRunSummaryView[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true); setError(null);
    try { setRuns(await strategyTuningApi.GetList()); }
    catch (e) { setError(e instanceof Error ? e.message : "خطا"); setRuns([]); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { void load(); }, [load]);
  return { runs, loading, error, refresh: load };
}

export function useStrategyTuningDetail(id: string | undefined) {
  const [run, setRun] = useState<StrategyTuningRunDetailView | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!id) return;
    setLoading(true); setError(null);
    try { setRun(await strategyTuningApi.GetById(id)); }
    catch (e) { setError(e instanceof Error ? e.message : "خطا"); }
    finally { setLoading(false); }
  }, [id]);

  useEffect(() => { void load(); }, [load]);

  useEffect(() => {
    if (!run) return;
    if (run.status !== 0 && run.status !== 1) return;
    const t = setInterval(() => void load(), 5000);
    return () => clearInterval(t);
  }, [run?.status, load]);

  return { run, loading, error, refresh: load };
}