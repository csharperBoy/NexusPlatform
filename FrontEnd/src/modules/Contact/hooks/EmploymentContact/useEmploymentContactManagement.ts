// src/modules/HR/hooks/useEmploymentContactManagement.ts

import { useState, useEffect, useMemo, useRef } from "react";
import * as XLSX from "xlsx";
import { employmentContactApi } from "../../api/EmploymentContactApi";
import { EmploymentContactInfoView } from "../../models/EmploymentContactInfoView";
import { UpdateEmploymentContactCommand } from "../../models/EmploymentContactCommand";

// پارس کردن ورودی‌ها به آرایه‌ای از رشته‌ها
const parsePhoneValues = (rawVal: any): string[] => {
  if (rawVal === undefined || rawVal === null) return [];
  if (Array.isArray(rawVal)) {
    return rawVal.map((v) => String(v).trim()).filter(Boolean);
  }
  return String(rawVal)
    .split(/[,/;\n\r]+/)
    .map((v) => v.trim())
    .filter(Boolean);
};

// مقایسه امن دو آرایه با پشتیبانی از null/undefined
const areArraysEqual = (
  arr1?: string[] | null,
  arr2?: string[] | null
): boolean => {
  const safe1 = arr1 ?? [];
  const safe2 = arr2 ?? [];
  if (safe1.length !== safe2.length) return false;
  return safe1.every((val, idx) => val === safe2[idx]);
};

export const useEmploymentContactManagement = () => {
  const [employmentContacts, setEmploymentContacts] = useState<EmploymentContactInfoView[]>([]);
  const [initialEmploymentContacts, setInitialEmploymentContacts] = useState<EmploymentContactInfoView[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const [globalSearch, setGlobalSearch] = useState<string>("");
  const [columnSearch, setColumnSearch] = useState<Record<string, string>>({});
  const [modifiedIds, setModifiedIds] = useState<Set<string>>(new Set());

  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const initialEmploymentContactsMap = useMemo(() => {
    const map = new Map<string, EmploymentContactInfoView>();
    initialEmploymentContacts.forEach((emp) => map.set(emp.id, emp));
    return map;
  }, [initialEmploymentContacts]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await employmentContactApi.GetList();
      const list = (data || []).map((item: any) => ({
        ...item,
        employmentContactPhone: Array.isArray(item.employmentContactPhone)
          ? item.employmentContactPhone
          : item.employmentContactPhone
          ? [item.employmentContactPhone]
          : [],
        employmentContactMobile: Array.isArray(item.employmentContactMobile)
          ? item.employmentContactMobile
          : item.employmentContactMobile
          ? [item.employmentContactMobile]
          : [],
      }));

      setEmploymentContacts(list);
      setInitialEmploymentContacts(JSON.parse(JSON.stringify(list)));
      setModifiedIds(new Set());
    } catch (err: any) {
      setError(err?.message || "خطا در دریافت لیست کارمندان");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleFieldChange = (
    id: string,
    field: "employmentContactPhone" | "employmentContactMobile",
    newValues: string[]
  ) => {
    setEmploymentContacts((prev) =>
      prev.map((item) => {
        if (item.id === id) {
          return { ...item, [field]: newValues };
        }
        return item;
      })
    );

    const initialEmp = initialEmploymentContactsMap.get(id);
    const initialValues = initialEmp ? initialEmp[field] ?? [] : [];

    setModifiedIds((prev) => {
      const next = new Set(prev);
      if (!areArraysEqual(initialValues, newValues)) {
        next.add(id);
      } else {
        const currentEmp = employmentContacts.find((e) => e.id === id);
        const otherField =
          field === "employmentContactPhone"
            ? "employmentContactMobile"
            : "employmentContactPhone";
        const otherInitial = initialEmp ? initialEmp[otherField] ?? [] : [];
        const otherCurrent = currentEmp ? currentEmp[otherField] ?? [] : [];

        if (areArraysEqual(otherInitial, otherCurrent)) {
          next.delete(id);
        }
      }
      return next;
    });
  };

  const handleColumnSearch = (column: string, value: string) => {
    setColumnSearch((prev) => ({ ...prev, [column]: value }));
  };

  const handleExcelImport = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (evt) => {
      try {
        setError(null);
        const data = evt.target?.result;
        const workbook = XLSX.read(data, { type: "array" });
        const firstSheetName = workbook.SheetNames[0];
        const worksheet = workbook.Sheets[firstSheetName];

        const excelRows = XLSX.utils.sheet_to_json<Record<string, any>>(worksheet);

        if (!excelRows || excelRows.length === 0) {
          alert("فایل اکسل انتخاب شده خالی است یا فرمت معتبری ندارد.");
          return;
        }

        let updatedCount = 0;
        const newModifiedIds = new Set(modifiedIds);

        setEmploymentContacts((prevEmploymentContacts) => {
          const empByCodeMap = new Map<string, EmploymentContactInfoView>();
          prevEmploymentContacts.forEach((emp) => {
            if (emp.employmentCode) {
              empByCodeMap.set(String(emp.employmentCode).trim(), emp);
            }
          });

          const nextEmploymentContacts = prevEmploymentContacts.map((emp) => ({
            ...emp,
            employmentContactPhone: [...(emp.employmentContactPhone ?? [])],
            employmentContactMobile: [...(emp.employmentContactMobile ?? [])],
          }));

          excelRows.forEach((row) => {
            const empCodeKey = Object.keys(row).find((k) =>
              ["کد پرسنلی", "کدپرسنلی", "employmentContactcode", "empcode", "کد"].includes(
                k.trim().toLowerCase()
              )
            );
            const phoneKey = Object.keys(row).find((k) =>
              ["تلفن داخلی", "داخلی", "employmentContactPhone", "phone"].includes(
                k.trim().toLowerCase()
              )
            );
            const mobileKey = Object.keys(row).find((k) =>
              ["موبایل سازمانی", "موبایل", "employmentContactMobile", "mobile"].includes(
                k.trim().toLowerCase()
              )
            );

            if (!empCodeKey) return;

            const rawEmpCode = row[empCodeKey];
            if (rawEmpCode === undefined || rawEmpCode === null) return;
            const empCodeStr = String(rawEmpCode).trim();

            const matchedEmp = empByCodeMap.get(empCodeStr);
            if (matchedEmp) {
              const targetIndex = nextEmploymentContacts.findIndex(
                (emp) => emp.id === matchedEmp.id
              );
              if (targetIndex === -1) return;

              let isRowChanged = false;

              if (phoneKey && row[phoneKey] !== undefined) {
                const newPhones = parsePhoneValues(row[phoneKey]);
                if (
                  !areArraysEqual(
                    nextEmploymentContacts[targetIndex].employmentContactPhone,
                    newPhones
                  )
                ) {
                  nextEmploymentContacts[targetIndex].employmentContactPhone = newPhones;
                  isRowChanged = true;
                }
              }

              if (mobileKey && row[mobileKey] !== undefined) {
                const newMobiles = parsePhoneValues(row[mobileKey]);
                if (
                  !areArraysEqual(
                    nextEmploymentContacts[targetIndex].employmentContactMobile,
                    newMobiles
                  )
                ) {
                  nextEmploymentContacts[targetIndex].employmentContactMobile = newMobiles;
                  isRowChanged = true;
                }
              }

              if (isRowChanged) {
                updatedCount++;
                newModifiedIds.add(matchedEmp.id);
              }
            }
          });

          setModifiedIds(newModifiedIds);

          if (updatedCount > 0) {
            setSuccessMessage(
              `اطلاعات ${updatedCount} کارمند با موفقیت از فایل اکسل اعمال شد.`
            );
            setTimeout(() => setSuccessMessage(null), 4000);
          } else {
            alert("هیچ رکوردی تغییر نکرد یا کد پرسنلی منطبقی پیدا نشد.");
          }

          return nextEmploymentContacts;
        });
      } catch (err: any) {
        setError("خطا در پردازش فایل اکسل: " + (err?.message || "فرمت فایل نامعتبر است"));
      } finally {
        e.target.value = "";
      }
    };

    reader.readAsArrayBuffer(file);
  };

  const filteredEmploymentContacts = useMemo(() => {
    const normalizedGlobal = globalSearch.trim().toLowerCase();

    return employmentContacts.filter((emp) => {
      const initEmp = initialEmploymentContactsMap.get(emp.id);

      const fullName = `${emp.firstName || ""} ${emp.lastName || ""}`;
      const initFullName = initEmp
        ? `${initEmp.firstName || ""} ${initEmp.lastName || ""}`
        : fullName;

      // پشتیبانی امن از null و undefined در سرچ
      const matchInArray = (arr?: string[] | null, q?: string) =>
        q ? (arr ?? []).some((val) => val.toLowerCase().includes(q)) : false;

      const matchesGlobal =
        !normalizedGlobal ||
        (emp.employmentCode || "").toLowerCase().includes(normalizedGlobal) ||
        fullName.toLowerCase().includes(normalizedGlobal) ||
        (emp.nationalCode || "").toLowerCase().includes(normalizedGlobal) ||
        matchInArray(emp.employmentContactPhone, normalizedGlobal) ||
        matchInArray(emp.employmentContactMobile, normalizedGlobal) ||
        (initEmp &&
          ((initEmp.employmentCode || "").toLowerCase().includes(normalizedGlobal) ||
            initFullName.toLowerCase().includes(normalizedGlobal) ||
            (initEmp.nationalCode || "").toLowerCase().includes(normalizedGlobal) ||
            matchInArray(initEmp.employmentContactPhone, normalizedGlobal) ||
            matchInArray(initEmp.employmentContactMobile, normalizedGlobal)));

      let matchesColumns = true;
      for (const [col, term] of Object.entries(columnSearch)) {
        if (!term.trim()) continue;
        const q = term.toLowerCase();

        if (col === "employmentCode") {
          const matchCur = (emp.employmentCode || "").toLowerCase().includes(q);
          const matchInit = (initEmp?.employmentCode || "").toLowerCase().includes(q);
          if (!matchCur && !matchInit) matchesColumns = false;
        }
        if (col === "fullName") {
          const matchCur = fullName.toLowerCase().includes(q);
          const matchInit = initFullName.toLowerCase().includes(q);
          if (!matchCur && !matchInit) matchesColumns = false;
        }
        if (col === "nationalCode") {
          const matchCur = (emp.nationalCode || "").toLowerCase().includes(q);
          const matchInit = (initEmp?.nationalCode || "").toLowerCase().includes(q);
          if (!matchCur && !matchInit) matchesColumns = false;
        }
        if (col === "employmentContactPhone") {
          const matchCur = matchInArray(emp.employmentContactPhone, q);
          const matchInit = matchInArray(initEmp?.employmentContactPhone, q);
          if (!matchCur && !matchInit) matchesColumns = false;
        }
        if (col === "employmentContactMobile") {
          const matchCur = matchInArray(emp.employmentContactMobile, q);
          const matchInit = matchInArray(initEmp?.employmentContactMobile, q);
          if (!matchCur && !matchInit) matchesColumns = false;
        }
      }

      return matchesGlobal && matchesColumns;
    });
  }, [employmentContacts, globalSearch, columnSearch, initialEmploymentContactsMap]);

  const handleResetChanges = () => {
    if (window.confirm("آیا از لغو تمام تغییرات اعمال شده اطمینان دارید؟")) {
      setEmploymentContacts(JSON.parse(JSON.stringify(initialEmploymentContacts)));
      setModifiedIds(new Set());
    }
  };

  const handleSaveChanges = async () => {
    if (modifiedIds.size === 0) return;

    try {
      setSaving(true);
      setError(null);
      setSuccessMessage(null);

      const employmentContactsMap = new Map<string, EmploymentContactInfoView>();
      employmentContacts.forEach((emp) => employmentContactsMap.set(emp.id, emp));

      const commands: UpdateEmploymentContactCommand[] = Array.from(modifiedIds).map((id) => {
        const emp = employmentContactsMap.get(id)!;
        return {
          id: emp.id,
          officePhones: emp.employmentContactPhone ?? [],
          orgMobiles: emp.employmentContactMobile ?? [],
        };
      });

      await employmentContactApi.batchUpdateEmploymentsContact(commands);

      setSuccessMessage(`تعداد ${commands.length} تغییر با موفقیت ذخیره شد.`);
      setInitialEmploymentContacts(JSON.parse(JSON.stringify(employmentContacts)));
      setModifiedIds(new Set());

      setTimeout(() => setSuccessMessage(null), 4000);
    } catch (err: any) {
      setError(err?.message || "خطا در ذخیره تغییرات اطلاعات کارمندان");
    } finally {
      setSaving(false);
    }
  };

  return {
    employmentContacts,
    filteredEmploymentContacts,
    loading,
    saving,
    error,
    successMessage,
    globalSearch,
    setGlobalSearch,
    columnSearch,
    modifiedIds,
    fileInputRef,
    handleFieldChange,
    handleColumnSearch,
    handleExcelImport,
    handleResetChanges,
    handleSaveChanges,
    loadData,
  };
};