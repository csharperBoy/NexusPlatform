import { useState, useEffect, useMemo, useRef } from "react";
import { BaseEntity, GenericCrudApi, TableFeatures } from "../types";
import { SelectionListDto } from "@/core/models/SelectionListDto";

export interface UseGenericCrudOptions<T extends BaseEntity, TCreateCmd, TUpdateCmd> {
  api: GenericCrudApi<T, TCreateCmd, TUpdateCmd>;
  // لیستی از توابع GetSelectionList برای فیلدهای Select در جدول
  selectionApis?: Record<string, () => Promise<SelectionListDto[]>>;
  mapToUpdateCommand?: (entity: T) => TUpdateCmd;
  features?: TableFeatures<T>;
}

export const useGenericCrud = <T extends BaseEntity, TCreateCmd = any, TUpdateCmd = any>({
  api,
  selectionApis = {},
  mapToUpdateCommand,
  features,
}: UseGenericCrudOptions<T, TCreateCmd, TUpdateCmd>) => {
  const [items, setItems] = useState<T[]>([]);
  const [initialItems, setInitialItems] = useState<T[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // لیست‌های کشویی و مپ‌های سریع آن‌ها برای رندر بهینه
  const [selections, setSelections] = useState<Record<string, SelectionListDto[]>>({});
  const selectionMaps = useMemo(() => {
    const maps: Record<string, Map<string, string>> = {};
    Object.entries(selections).forEach(([key, list]) => {
      maps[key] = new Map(list.map((s) => [s.value, s.display || s.label]));
    });
    return maps;
  }, [selections]);

  // مدیریت تغییرات درجا
  const [modifiedIds, setModifiedIds] = useState<Set<string>>(new Set());
  
  // جستجو
  const [globalSearch, setGlobalSearch] = useState<string>("");
  const [columnSearch, setColumnSearch] = useState<Record<string, string>>({});

  // استیت مودال‌ها
  const [deleteTarget, setDeleteTarget] = useState<{ id: string; title: string } | null>(null);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);
  const [isAddModalOpen, setIsAddModalOpen] = useState<boolean>(false);

  const fileInputRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);

      const selectionKeys = Object.keys(selectionApis);
      const selectionPromises = selectionKeys.map((k) => selectionApis[k]());
      
      const [listData, ...selectionResults] = await Promise.all([
        api.GetList(),
        ...selectionPromises,
      ]);

      const newSelections: Record<string, SelectionListDto[]> = {};
      selectionKeys.forEach((key, idx) => {
        newSelections[key] = selectionResults[idx] || [];
      });

      setItems(listData || []);
      setInitialItems(JSON.parse(JSON.stringify(listData || [])));
      setSelections(newSelections);
      setModifiedIds(new Set());
    } catch (err: any) {
      setError(err?.message || "خطا در دریافت اطلاعات سیستم");
    } finally {
      setLoading(false);
    }
  };

  const handleFieldChange = (id: string, field: keyof T, value: any) => {
    setItems((prev) =>
      prev.map((item) => (item.id === id ? { ...item, [field]: value } : item))
    );
    setModifiedIds((prev) => new Set(prev).add(id));
  };

  const handleSaveChanges = async () => {
    if (modifiedIds.size === 0 || !mapToUpdateCommand) return;
    try {
      setSaving(true);
      setError(null);
      const commands = Array.from(modifiedIds).map((id) => {
        const item = items.find((x) => x.id === id)!;
        return mapToUpdateCommand(item);
      });

      await api.batchUpdate(commands);
      
      setSuccessMessage(`تعداد ${commands.length} تغییر با موفقیت ذخیره شد.`);
      setInitialItems(JSON.parse(JSON.stringify(items)));
      setModifiedIds(new Set());
      
      setTimeout(() => setSuccessMessage(null), 4000);
    } catch (err: any) {
      setError(err?.message || "خطا در ذخیره تغییرات");
    } finally {
      setSaving(false);
    }
  };

  const handleResetChanges = () => {
    if (window.confirm("آیا از لغو تمام تغییرات اطمینان دارید؟")) {
      setItems(JSON.parse(JSON.stringify(initialItems)));
      setModifiedIds(new Set());
    }
  };

  const handleConfirmDelete = async () => {
    if (!deleteTarget) return;
    try {
      setIsDeleting(true);
      await api.delete(deleteTarget.id);
      setSuccessMessage(`رکورد مورد نظر با موفقیت حذف شد.`);
      setDeleteTarget(null);
      await loadData();
    } catch (err: any) {
      setError(err?.message || "خطا در حذف اطلاعات");
    } finally {
      setIsDeleting(false);
    }
  };

  const handleCreate = async (command: TCreateCmd) => {
    try {
      setSaving(true);
      await api.create(command);
      setSuccessMessage("رکورد جدید با موفقیت ایجاد شد.");
      setIsAddModalOpen(false);
      await loadData();
      setTimeout(() => setSuccessMessage(null), 4000);
    } catch (err: any) {
      setError(err?.message || "خطا در ثبت رکورد جدید");
    } finally {
      setSaving(false);
    }
  };

  // فیلتر کردن هوشمند دیتاها
  const filteredItems = useMemo(() => {
    return items.filter((item) => {
      // جستجوی سراسری
      const gQuery = globalSearch.trim().toLowerCase();
      if (gQuery) {
        const hasMatch = Object.entries(item).some(([k, val]) => {
          // اگر فیلد از نوع لیست انتخابی باشد، عنوان آن را جستجو کن
          let searchVal = String(val ?? "");
          if (selectionMaps[k] && selectionMaps[k].has(searchVal)) {
            searchVal = selectionMaps[k].get(searchVal)!;
          }
          return searchVal.toLowerCase().includes(gQuery);
        });
        if (!hasMatch) return false;
      }

      // جستجوی ستونی
      for (const [colKey, term] of Object.entries(columnSearch)) {
        if (!term.trim()) continue;
        let val = String(item[colKey as keyof T] ?? "");
        
        if (selectionMaps[colKey] && selectionMaps[colKey].has(val)) {
          val = selectionMaps[colKey].get(val)!;
        }

        if (!val.toLowerCase().includes(term.toLowerCase())) return false;
      }
      
      return true;
    });
  }, [items, globalSearch, columnSearch, selectionMaps]);
// افزودن به انتهای هوک useGenericCrud قبل از return:

const handleExcelImport = async (e: React.ChangeEvent<HTMLInputElement>) => {
  const file = e.target.files?.[0];
  if (!file || !features?.excelMapper) return;

  try {
    const XLSX = await import("xlsx");
    const reader = new FileReader();

    reader.onload = (evt) => {
      try {
        const bstr = evt.target?.result;
        const wb = XLSX.read(bstr, { type: "binary" });
        const wsname = wb.SheetNames[0];
        const ws = wb.Sheets[wsname];
        const rawData = XLSX.utils.sheet_to_json<Record<string, any>>(ws);

        const newItems: T[] = [];
        rawData.forEach((row, idx) => {
          const mapped = features.excelMapper!(row, {} as T);
          if (mapped) {
            const newItem = {
              id: `imported-${Date.now()}-${idx}`,
              ...mapped,
            } as T;
            newItems.push(newItem);
          }
        });

        if (newItems.length > 0) {
          setItems((prev) => [...prev, ...newItems]);
          setModifiedIds((prev) => {
            const next = new Set(prev);
            newItems.forEach((item) => next.add(item.id));
            return next;
          });
          setSuccessMessage(`تعداد ${newItems.length} رکورد از فایل اکسل بارگذاری شد.`);
        }
      } catch (err: any) {
        setError("خطا در پردازش فایل اکسل: " + err?.message);
      }
    };

    reader.readAsBinaryString(file);
  } catch (err: any) {
    setError("خطا در بارگذاری کتابخانه اکسل");
  } finally {
    if (fileInputRef.current) fileInputRef.current.value = "";
  }
};

  return {
    items: filteredItems,
    selections,
    selectionMaps,
    loading,
    saving,
    error,
    successMessage,
    modifiedIds,
    globalSearch,
    columnSearch,
    deleteTarget,
    isDeleting,
    isAddModalOpen,
    fileInputRef,
    handleExcelImport,
    setGlobalSearch,
    setColumnSearch: (col: string, val: string) => setColumnSearch((prev) => ({ ...prev, [col]: val })),
    setDeleteTarget,
    setIsAddModalOpen,
    handleFieldChange,
    handleSaveChanges,
    handleResetChanges,
    handleConfirmDelete,
    handleCreate,
    loadData,
  };
};