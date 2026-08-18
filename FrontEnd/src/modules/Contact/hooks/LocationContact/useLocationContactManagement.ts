// src/modules/HR/hooks/useLocationContactManagement.ts

import { useState, useEffect, useMemo, useCallback } from "react";
import { locationContactApi } from "../../api/LocationContactApi";
import { LocationContactInfoView } from "../../models/LocationContactInfoView";
import { UpdateLocationContactCommand } from "../../models/LocationContactCommand";

export const useLocationContactManagement = () => {
  const [locationContacts, setLocationContacts] = useState<LocationContactInfoView[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const [globalSearch, setGlobalSearch] = useState<string>("");
  const [modifiedIds, setModifiedIds] = useState<Set<string>>(new Set());

  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await locationContactApi.GetList();

      const formattedList: LocationContactInfoView[] = (data || []).map((item: any) => ({
        ...item,
        orgPhone: Array.isArray(item.orgPhone)
          ? item.orgPhone
          : item.orgPhone
          ? [item.orgPhone]
          : [],
        orgMobile: Array.isArray(item.orgMobile)
          ? item.orgMobile
          : item.orgMobile
          ? [item.orgMobile]
          : [],
      }));

      setLocationContacts(formattedList);
      setModifiedIds(new Set());
    } catch (err: any) {
      setError(err?.message || "خطا در دریافت اطلاعات مخاطبین واحدها");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleFieldChange = useCallback(
    (id: string, field: "orgPhone" | "orgMobile", values: string[]) => {
      setLocationContacts((prev) =>
        prev.map((item) => (item.id === id ? { ...item, [field]: values } : item))
      );
      setModifiedIds((prev) => new Set(prev).add(id));
    },
    []
  );

  const filteredLocationContacts = useMemo(() => {
    if (!globalSearch.trim()) return locationContacts;

    const q = globalSearch.toLowerCase().trim();
    const matchInArray = (arr?: string[] | null) =>
      (arr ?? []).some((v) => v.toLowerCase().includes(q));

    return locationContacts.filter((loc) => {
      return (
        loc.title?.toLowerCase().includes(q) ||
        matchInArray(loc.orgPhone) ||
        matchInArray(loc.orgMobile)
      );
    });
  }, [locationContacts, globalSearch]);

  const handleSaveChanges = useCallback(async () => {
    if (modifiedIds.size === 0) return;

    try {
      setSaving(true);
      setError(null);
      setSuccessMessage(null);

      // ساخت دستورات با اطمینان از عدم ارسال تایپ ناسازگار
      const commands: UpdateLocationContactCommand[] = Array.from(modifiedIds).map((id) => {
        const loc = locationContacts.find((x) => x.id === id)!;
        return {
          id: loc.id,
          title: loc.title,
          officePhone: loc.orgPhone ?? [], // استفاده از Nullish Coalescing برای رفع خطای null
          orgMobile: loc.orgMobile ?? [],   // استفاده از Nullish Coalescing برای رفع خطای null
        };
      });

      await locationContactApi.batchUpdateLocationsContact(commands);
      setSuccessMessage("تغییرات با موفقیت ذخیره شدند.");
      setModifiedIds(new Set());
    } catch (err: any) {
      setError(err?.message || "خطا در ذخیره‌سازی تغییرات");
    } finally {
      setSaving(false);
    }
  }, [modifiedIds, locationContacts]);

  return {
    locationContacts: filteredLocationContacts,
    loading,
    saving,
    error,
    successMessage,
    globalSearch,
    setGlobalSearch,
    isModified: modifiedIds.size > 0,
    modifiedCount: modifiedIds.size,
    handleFieldChange,
    handleSaveChanges,
    reload: loadData,
  };
};