import { useCallback, useEffect, useRef, useState } from "react";
import { accountApi } from "../api/accountApi";
import { tradingRunApi } from "../api/tradingRunApi";
import { riskApi } from "../api/riskApi";
import type {
  AccountInfoView,
  TradingRunInfoView,
  KillSwitchStatusView,
  LossStreakState,
} from "../models";
import { KillSwitchScope } from "../models";

const AUTO_REFRESH_MS = 15_000;

export interface RiskActionContext {
  /** مثل "activate-global"، "reset:runId:accountId" — برای disable کردن دکمه */
  key: string;
}

export function useRiskDashboard() {
  const [killSwitch, setKillSwitch] = useState<KillSwitchStatusView | null>(null);
  const [activeCircuits, setActiveCircuits] = useState<LossStreakState[]>([]);
  const [accounts, setAccounts] = useState<AccountInfoView[]>([]);
  const [runs, setRuns] = useState<TradingRunInfoView[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [busyKey, setBusyKey] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [lastRefreshAt, setLastRefreshAt] = useState<Date | null>(null);

  const initialLoadDone = useRef(false);

  const load = useCallback(async () => {
    if (!initialLoadDone.current) setLoading(true);
    setError(null);

    try {
      const [ks, active, accs, rns] = await Promise.all([
        riskApi.KillSwitchStatus(),
        riskApi.CircuitBreakerActive(),
        accountApi.GetList(),
        tradingRunApi.GetList(),
      ]);
      setKillSwitch(ks);
      setActiveCircuits(active);
      setAccounts(accs);
      setRuns(rns);
      setLastRefreshAt(new Date());
    } catch (e) {
      setError(
        e instanceof Error ? e.message : "خطا در دریافت وضعیت ریسک",
      );
    } finally {
      initialLoadDone.current = true;
      setLoading(false);
    }
  }, []);

  // Initial load + auto-refresh
  useEffect(() => {
    void load();
    const id = setInterval(() => {
      // فقط اگر هیچ action در حال اجرا نیست، refresh کن
      if (!busyKey) void load();
    }, AUTO_REFRESH_MS);
    return () => clearInterval(id);
  }, [load, busyKey]);

  /* ═══════════════ Kill Switch ═══════════════ */

  const activateGlobalKillSwitch = useCallback(
    async (reason: string | null) => {
      const key = "activate-global";
      setBusyKey(key);
      try {
        await riskApi.KillSwitchActivate({
          scope: KillSwitchScope.Global,
          accountId: null,
          reason,
        });
        await load();
      } finally {
        setBusyKey(null);
      }
    },
    [load],
  );

  const deactivateGlobalKillSwitch = useCallback(async () => {
    const key = "deactivate-global";
    setBusyKey(key);
    try {
      await riskApi.KillSwitchDeactivate({
        scope: KillSwitchScope.Global,
        accountId: null,
      });
      await load();
    } finally {
      setBusyKey(null);
    }
  }, [load]);

  const activateAccountKillSwitch = useCallback(
    async (accountId: string, reason: string | null) => {
      const key = `activate:${accountId}`;
      setBusyKey(key);
      try {
        await riskApi.KillSwitchActivate({
          scope: KillSwitchScope.Account,
          accountId,
          reason,
        });
        await load();
      } finally {
        setBusyKey(null);
      }
    },
    [load],
  );

  const deactivateAccountKillSwitch = useCallback(
    async (accountId: string) => {
      const key = `deactivate:${accountId}`;
      setBusyKey(key);
      try {
        await riskApi.KillSwitchDeactivate({
          scope: KillSwitchScope.Account,
          accountId,
        });
        await load();
      } finally {
        setBusyKey(null);
      }
    },
    [load],
  );

  /* ═══════════════ Circuit Breaker ═══════════════ */

  const resetCircuit = useCallback(
    async (runId: string, accountId: string, reason: string | null) => {
      const key = `reset:${runId}:${accountId}`;
      setBusyKey(key);
      try {
        await riskApi.CircuitBreakerReset({ runId, accountId, reason });
        await load();
      } finally {
        setBusyKey(null);
      }
    },
    [load],
  );

  const resetAllCircuits = useCallback(async () => {
    const key = "reset-all";
    setBusyKey(key);
    try {
      await riskApi.CircuitBreakerReset({});
      await load();
    } finally {
      setBusyKey(null);
    }
  }, [load]);

  return {
    killSwitch,
    activeCircuits,
    accounts,
    runs,
    loading,
    busyKey,
    error,
    lastRefreshAt,
    refresh: load,
    activateGlobalKillSwitch,
    deactivateGlobalKillSwitch,
    activateAccountKillSwitch,
    deactivateAccountKillSwitch,
    resetCircuit,
    resetAllCircuits,
  };
}