import { useCallback, useEffect, useState } from "react";
import { schedulePlanApi } from "../api/schedulePlanApi";
import type {
  SchedulePlanInfoView,
  CreateSchedulePlanCommand,
  UpdateSchedulePlanCommand,
} from "../models";

export function useSchedulePlansPage() {
  const [plans, setPlans] = useState<SchedulePlanInfoView[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const reload = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await schedulePlanApi.GetList();
      setPlans(data ?? []);
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در دریافت پلن‌ها");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    reload();
  }, [reload]);

  /* ─── feedback auto-clear ─── */
  useEffect(() => {
    if (!successMessage) return;
    const t = setTimeout(() => setSuccessMessage(null), 3000);
    return () => clearTimeout(t);
  }, [successMessage]);

  const createPlan = useCallback(
    async (cmd: CreateSchedulePlanCommand) => {
      setSaving(true);
      setError(null);
      try {
        await schedulePlanApi.create(cmd);
        await reload();
        setSuccessMessage("پلن با موفقیت ثبت شد");
        return true;
      } catch (e) {
        setError(e instanceof Error ? e.message : "خطا در ثبت پلن");
        return false;
      } finally {
        setSaving(false);
      }
    },
    [reload],
  );

  const updatePlan = useCallback(
    async (cmd: UpdateSchedulePlanCommand) => {
      setSaving(true);
      setError(null);
      try {
        await schedulePlanApi.update(cmd);
        await reload();
        setSuccessMessage("تغییرات ذخیره شد");
        return true;
      } catch (e) {
        setError(e instanceof Error ? e.message : "خطا در ذخیره تغییرات");
        return false;
      } finally {
        setSaving(false);
      }
    },
    [reload],
  );

  const deletePlan = useCallback(
    async (id: string) => {
      setSaving(true);
      setError(null);
      try {
        await schedulePlanApi.delete(id);
        await reload();
        setSuccessMessage("پلن حذف شد");
        return true;
      } catch (e) {
        setError(e instanceof Error ? e.message : "خطا در حذف پلن");
        return false;
      } finally {
        setSaving(false);
      }
    },
    [reload],
  );

  const enablePlan = useCallback(
    async (id: string) => {
      try {
        await schedulePlanApi.enable(id);
        await reload();
      } catch (e) {
        setError(e instanceof Error ? e.message : "خطا در فعال‌سازی");
      }
    },
    [reload],
  );

  const disablePlan = useCallback(
    async (id: string) => {
      try {
        await schedulePlanApi.disable(id);
        await reload();
      } catch (e) {
        setError(e instanceof Error ? e.message : "خطا در غیرفعال‌سازی");
      }
    },
    [reload],
  );

  return {
    plans,
    loading,
    saving,
    error,
    successMessage,
    reload,
    createPlan,
    updatePlan,
    deletePlan,
    enablePlan,
    disablePlan,
  };
}