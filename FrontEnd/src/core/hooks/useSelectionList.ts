import { useEffect, useState } from "react";
import type { SelectionListDto } from "../models/SelectionListDto";

/**
 * هوک عمومی برای لود لیست‌های انتخابی از GetSelectionList بک‌اند.
 * هر جا dropdown نیاز داری، از این استفاده کن.
 */
export function useSelectionList(
  loader: () => Promise<SelectionListDto[]>,
  deps: unknown[] = [],
) {
  const [items, setItems] = useState<SelectionListDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const reload = async () => {
    try {
      setLoading(true);
      setError(null);
      const list = await loader();
      setItems(list ?? []);
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در دریافت لیست");
      setItems([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void reload();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);

  return { items, loading, error, reload };
}