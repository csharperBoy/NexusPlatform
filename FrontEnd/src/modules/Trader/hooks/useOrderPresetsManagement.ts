import { useCallback, useEffect, useState } from "react";
import { orderPresetApi } from "../api/orderPresetApi";
import type {
  OrderPresetView,
  CreateOrderPresetCommand,
  UpdateOrderPresetCommand,
} from "../models";

export function useOrderPresetsManagement() {
  const [presets, setPresets] = useState<OrderPresetView[]>([]);
  const [loading, setLoading] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await orderPresetApi.GetList();
      setPresets(data);
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
    async (cmd: CreateOrderPresetCommand): Promise<string> => {
      const id = await orderPresetApi.create(cmd);
      await load();
      return id;
    },
    [load],
  );

  const handleUpdate = useCallback(
    async (cmd: UpdateOrderPresetCommand): Promise<void> => {
      await orderPresetApi.update(cmd);
      await load();
    },
    [load],
  );

  const handleDelete = useCallback(
    async (id: string): Promise<void> => {
      setBusyId(id);
      try {
        await orderPresetApi.delete(id);
        await load();
      } finally {
        setBusyId(null);
      }
    },
    [load],
  );

  return {
    presets,
    loading,
    busyId,
    error,
    refresh: load,
    handleCreate,
    handleUpdate,
    handleDelete,
  };
}