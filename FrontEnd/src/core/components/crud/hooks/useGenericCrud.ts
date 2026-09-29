import { useState, useEffect, useMemo, useCallback } from "react";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import {
  GenericColumnDef,
  UseGenericCrudOptions,
  DeleteTarget,
} from "../types";
import { ApiOptions } from "@/core/api/apiOptions";
import { BaseEntity } from "@/core/models/BaseEntity";

export function useGenericCrud<T extends BaseEntity, TCreateCmd, TUpdateCmd>({
  api,
  apiOptions,
  columns,
  selectionApis,
  mapToUpdateCommand,
  mapToCreateCommand,
  transformApiData,
  excelMatchKey,
}: UseGenericCrudOptions<T, TCreateCmd, TUpdateCmd>) {
  const [globalSearch, setGlobalSearch] = useState<string>("");
  const [columnFilters, setColumnFilters] = useState<Record<string, string>>({});

  const [items, setItems] = useState<T[]>([]);
  const [initialItems, setInitialItems] = useState<T[]>([]);
  const [selectionLists, setSelectionLists] = useState<
    Record<string, SelectionListDto[]>
  >({});
  const [loading, setLoading] = useState<boolean>(false);
  const [saving, setSaving] = useState<boolean>(false);

  const [isAddModalOpen, setIsAddModalOpen] = useState<boolean>(false);
  const [deleteTarget, setDeleteTarget] = useState<DeleteTarget<T> | null>(null);

  // ─── helper: گرفتن گزینه‌های یک ستون (static یا dynamic) ───
  // ← NEW
  const getColOptions = useCallback(
    (col: GenericColumnDef<T>): SelectionListDto[] | null => {
      if (col.staticOptions) return col.staticOptions;
      if (col.selectionKey && selectionLists[col.selectionKey])
        return selectionLists[col.selectionKey];
      return null;
    },
    [selectionLists]
  );

  // ─── Fetch ───
  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const listPromise = api.getList();
      const selectionPromises = selectionApis
        ? Object.entries(selectionApis).map(async ([key, fetcher]) => {
            const res = await fetcher();
            return { key, data: res };
          })
        : [];

      const [listData, ...selections] = await Promise.all([
        listPromise,
        ...selectionPromises,
      ]);

      const processedList = transformApiData
        ? transformApiData(listData || [])
        : listData || [];

      setItems(processedList);
      setInitialItems(JSON.parse(JSON.stringify(processedList)));

      const selObj: Record<string, SelectionListDto[]> = {};
      selections.forEach((sel: any) => {
        selObj[sel.key] = sel.data;
      });
      setSelectionLists(selObj);
    } catch (error) {
      console.error("Failed to fetch CRUD data:", error);
    } finally {
      setLoading(false);
    }
  }, [api, selectionApis, transformApiData]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  // ─── Inline Cell Editing با نرمال‌سازی valueType ───
  const handleFieldChange = useCallback(
    (id: string | number, field: keyof T, value: any) => {
      const col = columns.find((c) => String(c.key) === String(field));
      let normalized = value;

      if (col?.valueType === "number") {
        normalized =
          value === "" || value == null || value === "null"
            ? null
            : Number(value);
        if (typeof normalized === "number" && isNaN(normalized))
          normalized = null;
      } else if (col?.valueType === "boolean") {
        normalized = !!value;
      }

      setItems((prev) =>
        prev.map((item) =>
          item.id === id ? { ...item, [field]: normalized } : item
        )
      );
    },
    [columns]
  );

  // ─── Track Modified Items ───
  const modifiedItems = useMemo(() => {
    return items.filter((item) => {
      const init = initialItems.find((x) => x.id === item.id);
      if (!init) return true;
      return JSON.stringify(item) !== JSON.stringify(init);
    });
  }, [items, initialItems]);

  const hasChanges = modifiedItems.length > 0;

  // ─── Save All ───
  const handleSaveAll = useCallback(
    async (options?: ApiOptions) => {
      if (!hasChanges) return;
      setSaving(true);
      try {
        const updateCmds: TUpdateCmd[] = modifiedItems.map((item) =>
          mapToUpdateCommand
            ? mapToUpdateCommand(item)
            : (item as unknown as TUpdateCmd)
        );

        const mergedOptions = { ...apiOptions, ...options };
        const res = await api.batchUpdate(updateCmds, mergedOptions);

        if (res && res._isOfflineQueued) {
          alert("ارتباط با اینترنت قطع است. تغییرات شما در صف ذخیره شد.");
          setInitialItems(JSON.parse(JSON.stringify(items)));
        } else {
          await fetchData();
        }
      } catch (error) {
        console.error("Failed to save changes:", error);
      } finally {
        setSaving(false);
      }
    },
    [
      hasChanges,
      modifiedItems,
      mapToUpdateCommand,
      api,
      apiOptions,
      fetchData,
      items,
    ]
  );

  // ─── Create ───
  const handleCreate = useCallback(
    async (formData: Record<string, any>, options?: ApiOptions) => {
      setSaving(true);
      try {
        // نکته: نرمال‌سازی valueType در GenericAddModal انجام می‌شود،
        // پس اینجا formData از قبل تمیز است.
        const createCmd = mapToCreateCommand
          ? mapToCreateCommand(formData)
          : (formData as TCreateCmd);

        const mergedOptions = { ...apiOptions, ...options };
        const res = await api.create(createCmd, mergedOptions);

        if (res && res._isOfflineQueued) {
          const tempId = `temp-${Date.now()}`;
          const newItem = { id: tempId, ...formData } as unknown as T;
          setItems((prev) => [newItem, ...prev]);
          setInitialItems((prev) => [newItem, ...prev]);
          alert("ارتباط با اینترنت قطع است. رکورد در صف ثبت قرار گرفت.");
        } else {
          await fetchData();
        }

        setIsAddModalOpen(false);
      } catch (error) {
        console.error("Failed to create record:", error);
      } finally {
        setSaving(false);
      }
    },
    [mapToCreateCommand, api, apiOptions, fetchData]
  );

  // ─── Delete ───
  const confirmDelete = useCallback(
    async (options?: ApiOptions) => {
      if (!deleteTarget) return;
      setSaving(true);
      try {
        const mergedOptions = { ...apiOptions, ...options };
        const res = await api.delete(deleteTarget.item.id, mergedOptions);

        if (res && res._isOfflineQueued) {
          setItems((prev) =>
            prev.filter((i) => i.id !== deleteTarget.item.id)
          );
          setInitialItems((prev) =>
            prev.filter((i) => i.id !== deleteTarget.item.id)
          );
          alert("ارتباط با اینترنت قطع است. رکورد برای حذف در صف قرار گرفت.");
        } else {
          await fetchData();
        }

        setDeleteTarget(null);
      } catch (error) {
        console.error("Failed to delete record:", error);
      } finally {
        setSaving(false);
      }
    },
    [deleteTarget, api, apiOptions, fetchData]
  );

  const prepareDelete = useCallback(
    (item: T) => {
      const initialItem = initialItems.find((x) => x.id === item.id);
      const isModified = initialItem
        ? JSON.stringify(item) !== JSON.stringify(initialItem)
        : true;

      setDeleteTarget({ item, isModified });
    },
    [initialItems]
  );

  // ─── Excel Import ───
  const handleExcelImport = useCallback(
    (importedData: Partial<T>[]) => {
      setItems((prev) => {
        const next = [...prev];
        importedData.forEach((row) => {
          let existingIndex = -1;
          if (excelMatchKey && row[excelMatchKey]) {
            existingIndex = next.findIndex(
              (item) => item[excelMatchKey] === row[excelMatchKey]
            );
          }

          if (existingIndex > -1) {
            next[existingIndex] = { ...next[existingIndex], ...row };
          } else {
            next.push({
              id: `temp-${Date.now()}-${Math.random()
                .toString(36)
                .substr(2, 9)}`,
              ...row,
            } as unknown as T);
          }
        });
        return next;
      });
    },
    [excelMatchKey]
  );

  // ─── Search & Filtering (با پشتیبانی staticOptions) ───
  const filteredItems = useMemo(() => {
    return items.filter((item) => {
      // Global Search
      if (globalSearch.trim()) {
        const query = globalSearch.toLowerCase();
        const matchesGlobal = columns.some((col) => {
          if (col.getFilterValue) {
            const customVal = col.getFilterValue(item);
            return customVal?.toLowerCase().includes(query);
          }
          const val = item[col.key as keyof T];
          if (val == null) return false;

          // ← NEW: استفاده از getColOptions (static یا dynamic)
          const opts = getColOptions(col);

          // Multi-Select / Array Search
          if (Array.isArray(val) && opts) {
            return val.some((v) => {
              const opt = opts.find((o) => String(o.value) === String(v));
              if (!opt) return false;
              return (
                opt.label?.toLowerCase().includes(query) ||
                opt.display?.toLowerCase().includes(query)
              );
            });
          }

          // Single Select Search
          if (opts) {
            const opt = opts.find((o) => String(o.value) === String(val));
            if (
              opt?.label?.toLowerCase().includes(query) ||
              opt?.display?.toLowerCase().includes(query)
            ) {
              return true;
            }
          }

          return String(val).toLowerCase().includes(query);
        });
        if (!matchesGlobal) return false;
      }

      // Column Filters
      for (const colKey in columnFilters) {
        const filterVal = columnFilters[colKey]?.toLowerCase();
        if (!filterVal) continue;

        const colDef = columns.find((c) => String(c.key) === colKey);

        if (colDef?.getFilterValue) {
          const customVal =
            colDef.getFilterValue(item)?.toLowerCase() || "";
          if (!customVal.includes(filterVal)) return false;
          continue;
        }

        const val = item[colKey as keyof T];
        if (val == null) return false;

        // ← NEW: استفاده از getColOptions
        const opts = colDef ? getColOptions(colDef) : null;

        if (Array.isArray(val) && opts) {
          const matchInArray = val.some((v) => {
            const opt = opts.find((o) => String(o.value) === String(v));
            return (
              opt?.label?.toLowerCase().includes(filterVal) ||
              opt?.display?.toLowerCase().includes(filterVal)
            );
          });
          if (!matchInArray) return false;
        } else if (opts) {
          const opt = opts.find((o) => String(o.value) === String(val));
          const matched =
            opt?.label?.toLowerCase().includes(filterVal) ||
            opt?.display?.toLowerCase().includes(filterVal);
          if (!matched) return false;
        } else if (!String(val).toLowerCase().includes(filterVal)) {
          return false;
        }
      }

      return true;
    });
  }, [
    items,
    globalSearch,
    columnFilters,
    columns,
    selectionLists,
    getColOptions,
  ]);

  return {
    items: filteredItems,
    rawItems: items,
    selectionLists,
    loading,
    saving,
    hasChanges,
    modifiedCount: modifiedItems.length,
    globalSearch,
    setGlobalSearch,
    columnFilters,
    setColumnFilters,
    isAddModalOpen,
    setIsAddModalOpen,
    deleteTarget,
    setDeleteTarget,
    handleFieldChange,
    handleSaveAll,
    handleCreate,
    prepareDelete,
    confirmDelete,
    handleExcelImport,
    refresh: fetchData,
  };
}