import { useState, useEffect, useMemo, useCallback } from "react";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import {
  BaseEntity,
  GenericColumnDef,
  UseGenericCrudOptions,
  DeleteTarget,
} from "../types";

export function useGenericCrud<T extends BaseEntity, TCreateCmd, TUpdateCmd>({
  api,
  columns,
  selectionApis,
  mapToUpdateCommand,
  mapToCreateCommand,
  transformApiData,
  excelMatchKey,
}: UseGenericCrudOptions<T, TCreateCmd, TUpdateCmd>) {
  const [items, setItems] = useState<T[]>([]);
  const [initialItems, setInitialItems] = useState<T[]>([]);
  const [selectionLists, setSelectionLists] = useState<Record<string, SelectionListDto[]>>({});
  const [loading, setLoading] = useState<boolean>(false);
  const [saving, setSaving] = useState<boolean>(false);

  const [globalSearch, setGlobalSearch] = useState<string>("");
  const [columnFilters, setColumnFilters] = useState<Record<string, string>>({});

  const [isAddModalOpen, setIsAddModalOpen] = useState<boolean>(false);
  const [deleteTarget, setDeleteTarget] = useState<DeleteTarget<T> | null>(null);

  // 1. Fetch Initial Data & Selection Lists
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

  // 2. Handle Inline Cell Editing
  const handleFieldChange = useCallback((id: string | number, field: keyof T, value: any) => {
    setItems((prev) =>
      prev.map((item) => (item.id === id ? { ...item, [field]: value } : item))
    );
  }, []);

  // 3. Track Modified Items
  const modifiedItems = useMemo(() => {
    return items.filter((item) => {
      const init = initialItems.find((x) => x.id === item.id);
      if (!init) return true;
      return JSON.stringify(item) !== JSON.stringify(init);
    });
  }, [items, initialItems]);

  const hasChanges = modifiedItems.length > 0;

  // 4. Save All Modified Items (Bulk Update)
  const handleSaveAll = useCallback(async () => {
    if (!hasChanges) return;
    setSaving(true);
    try {
      const updateCmds: TUpdateCmd[] = modifiedItems.map((item) =>
        mapToUpdateCommand ? mapToUpdateCommand(item) : (item as unknown as TUpdateCmd)
      );
      await api.batchUpdate(updateCmds);
      await fetchData();
    } catch (error) {
      console.error("Failed to save changes:", error);
    } finally {
      setSaving(false);
    }
  }, [hasChanges, modifiedItems, mapToUpdateCommand, api, fetchData]);

  // 5. Create Single Item
  const handleCreate = useCallback(
    async (formData: Record<string, any>) => {
      setSaving(true);
      try {
        const createCmd = mapToCreateCommand
          ? mapToCreateCommand(formData)
          : (formData as TCreateCmd);
        await api.create(createCmd);
        setIsAddModalOpen(false);
        await fetchData();
      } catch (error) {
        console.error("Failed to create record:", error);
      } finally {
        setSaving(false);
      }
    },
    [mapToCreateCommand, api, fetchData]
  );

  // 6. Delete Management
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

  const confirmDelete = useCallback(async () => {
    if (!deleteTarget) return;
    setSaving(true);
    try {
      await api.delete(deleteTarget.item.id);
      setDeleteTarget(null);
      await fetchData();
    } catch (error) {
      console.error("Failed to delete record:", error);
    } finally {
      setSaving(false);
    }
  }, [deleteTarget, api, fetchData]);

  // 7. Excel Import & Merge Logic
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
              id: `temp-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
              ...row,
            } as unknown as T);
          }
        });
        return next;
      });
    },
    [excelMatchKey]
  );

  // 8. Search & Filtering using SelectionListDto (value, label, display)
  const filteredItems = useMemo(() => {
    return items.filter((item) => {
      // Global Search
      if (globalSearch.trim()) {
        const query = globalSearch.toLowerCase();
        const matchesGlobal = columns.some((col) => {
          const val = item[col.key as keyof T];
          if (val == null) return false;

          // Multi-Select / Array Search
          if (Array.isArray(val) && col.selectionKey && selectionLists[col.selectionKey]) {
            const options = selectionLists[col.selectionKey];
            return val.some((v) => {
              const opt = options.find((o) => String(o.value) === String(v));
              if (!opt) return false;
              return (
                opt.label?.toLowerCase().includes(query) ||
                opt.display?.toLowerCase().includes(query)
              );
            });
          }

          // Single Select Search
          if (col.selectionKey && selectionLists[col.selectionKey]) {
            const opt = selectionLists[col.selectionKey].find(
              (o) => String(o.value) === String(val)
            );
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
        const val = item[colKey as keyof T];
        if (val == null) return false;

        if (Array.isArray(val) && colDef?.selectionKey && selectionLists[colDef.selectionKey]) {
          const options = selectionLists[colDef.selectionKey];
          const matchInArray = val.some((v) => {
            const opt = options.find((o) => String(o.value) === String(v));
            return (
              opt?.label?.toLowerCase().includes(filterVal) ||
              opt?.display?.toLowerCase().includes(filterVal)
            );
          });
          if (!matchInArray) return false;
        } else if (colDef?.selectionKey && selectionLists[colDef.selectionKey]) {
          const opt = selectionLists[colDef.selectionKey].find(
            (o) => String(o.value) === String(val)
          );
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
  }, [items, globalSearch, columnFilters, columns, selectionLists]);

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