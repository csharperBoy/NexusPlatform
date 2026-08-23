import React, { useEffect, useState, useMemo, useRef } from "react";
import * as XLSX from "xlsx";
import { employmentApi } from "../../api/EmploymentApi";
import { locationApi } from "../../api/LocationApi";
import { EmploymentInfoView } from "../../models/EmploymentInfoView";
import { LocationInfoView as Location } from "../../models/LocationInfoView";
import { UpdateEmploymentCommand } from "../../models/EmploymentCommand";
import { SelectionListDto } from "@/core/models/SelectionListDto";

type EditableField =
  | "employmentCode"
  | "firstName"
  | "lastName"
  | "nationalCode"
  | "locationsId"; // استفاده از locationsId برای آی‌دی مکان‌های انتخاب شده

export interface EmploymentItem {
  id: string;
  employmentCode?: string;
  firstName?: string;
  lastName?: string;
  nationalCode?: string;
  locationsId?: string[];
}
export const useEmploymentManagement = () => {
  // --- States ---
  const [employments, setEmployments] = useState<EmploymentInfoView[]>([]);
  const [initialEmployments, setInitialEmployments] = useState<EmploymentInfoView[]>([]);
  const [locations, setLocations] = useState<SelectionListDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  
    // --- استیت‌های مودال حذف ---
    const [deleteTarget, setDeleteTarget] = useState<{ id: string; title: string; isModified: boolean } | null>(null);
    const [isDeleting, setIsDeleting] = useState<boolean>(false);
  
  // استیت‌های سرچ
  const [globalSearch, setGlobalSearch] = useState<string>("");
  const [columnSearch, setColumnSearch] = useState<Record<string, string>>({});

  // مدیریت تغییرات
  const [modifiedIds, setModifiedIds] = useState<Set<string>>(new Set());

  // ریف مربوط به آپلود فایل اکسل
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const initialEmploymentsMap = useMemo(() => {
    const map = new Map<string, EmploymentInfoView>();
    initialEmployments.forEach((emp) => map.set(emp.id, emp));
    return map;
  }, [initialEmployments]);

  // مپ برای دسترسی سریع به عنوان مکان‌ها بر اساس Value/ID
  const locationMap = useMemo(() => {
    const map = new Map<string, string>();
    locations.forEach((loc) => map.set(loc.value, loc.display || loc.label));
    return map;
  }, [locations]);

  // --- 1. دریافت اطلاعات اولیه ---
  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);

      const [empData, locationList] = await Promise.all([
        employmentApi.GetList(),
        locationApi.GetSelectionList(),
      ]);

      const list = empData || [];

      // تبدیل emp.locations (که Location[] است) به locationsId (که string[] است)
      const normalizedList = list.map((emp: EmploymentInfoView & { locationsId?: string[] }) => {
        const locIds = Array.isArray(emp.locations)
          ? emp.locations.map((l: Location) => l.id)
          : [];

        return {
          ...emp,
          locationsId: locIds,
        };
      });

      setEmployments(normalizedList);
      setInitialEmployments(JSON.parse(JSON.stringify(normalizedList)));
      setLocations(locationList || []);
      setModifiedIds(new Set());
    } catch (err: any) {
      setError(err?.message || "خطا در دریافت اطلاعات اولیه");
    } finally {
      setLoading(false);
    }
  };

  // --- 2. مدیریت ویرایش درجا ---
  const handleFieldChange = (
    id: string,
    field: EditableField,
    value: string | string[]
  ) => {
    setEmployments((prev) =>
      prev.map((item) => {
        if (item.id === id) {
          return { ...item, [field]: value };
        }
        return item;
      })
    );
    setModifiedIds((prev) => new Set(prev).add(id));
  };

  const handleColumnSearch = (column: string, value: string) => {
    setColumnSearch((prev) => ({ ...prev, [column]: value }));
  };

  const handleGlobalSearch = (value: string) => {
    setGlobalSearch(value);
  };

  // --- 3. بارگذاری فایل اکسل ---
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

        setEmployments((prevEmployments) => {
          const empByCodeMap = new Map<string, EmploymentInfoView>();
          prevEmployments.forEach((emp) => {
            if (emp.employmentCode) {
              empByCodeMap.set(String(emp.employmentCode).trim(), emp);
            }
          });

          const nextEmployments = prevEmployments.map((emp) => ({ ...emp }));

          excelRows.forEach((row) => {
            const empCodeKey = Object.keys(row).find((k) =>
              ["کد پرسنلی", "کدپرسنلی", "employmentcode", "empcode", "کد"].includes(
                k.trim().toLowerCase()
              )
            );
            const firstNameKey = Object.keys(row).find((k) =>
              ["نام", "firstname", "first_name"].includes(k.trim().toLowerCase())
            );
            const lastNameKey = Object.keys(row).find((k) =>
              ["نام خانوادگی", "نام_خانوادگی", "lastname", "last_name"].includes(
                k.trim().toLowerCase()
              )
            );
            const nationalCodeKey = Object.keys(row).find((k) =>
              ["کد ملی", "کدملی", "nationalcode", "national_code"].includes(
                k.trim().toLowerCase()
              )
            );
            const locationKey = Object.keys(row).find((k) =>
              ["محل استقرار", "مکان", "محل_استقرار", "location", "locations", "locationid"].includes(
                k.trim().toLowerCase()
              )
            );

            if (!empCodeKey) return;

            const rawEmpCode = row[empCodeKey];
            if (rawEmpCode === undefined || rawEmpCode === null) return;
            const empCodeStr = String(rawEmpCode).trim();

            const matchedEmp = empByCodeMap.get(empCodeStr);
            if (matchedEmp) {
              const targetIndex = nextEmployments.findIndex((emp) => emp.id === matchedEmp.id);
              if (targetIndex === -1) return;

              let isRowChanged = false;

              if (firstNameKey && row[firstNameKey] !== undefined) {
                const newVal = String(row[firstNameKey] ?? "").trim();
                if (nextEmployments[targetIndex].firstName !== newVal) {
                  nextEmployments[targetIndex].firstName = newVal;
                  isRowChanged = true;
                }
              }

              if (lastNameKey && row[lastNameKey] !== undefined) {
                const newVal = String(row[lastNameKey] ?? "").trim();
                if (nextEmployments[targetIndex].lastName !== newVal) {
                  nextEmployments[targetIndex].lastName = newVal;
                  isRowChanged = true;
                }
              }

              if (nationalCodeKey && row[nationalCodeKey] !== undefined) {
                const newVal = String(row[nationalCodeKey] ?? "").trim();
                if (nextEmployments[targetIndex].nationalCode !== newVal) {
                  nextEmployments[targetIndex].nationalCode = newVal;
                  isRowChanged = true;
                }
              }

              // پردازش مکان‌ها (جدا شده با کاما در فایل اکسل)
              if (locationKey && row[locationKey] !== undefined) {
                const rawLocStr = String(row[locationKey] ?? "").trim();
                const locParts = rawLocStr
                  .split(/[,،;]/)
                  .map((s) => s.trim())
                  .filter(Boolean);

                const matchedLocIds: string[] = [];

                locParts.forEach((part) => {
                  const matchedLocation = locations.find(
                    (l) =>
                      l.value.toLowerCase() === part.toLowerCase() ||
                      l.display?.toLowerCase() === part.toLowerCase() ||
                      l.label?.toLowerCase() === part.toLowerCase()
                  );

                  if (matchedLocation) {
                    matchedLocIds.push(matchedLocation.value);
                  } else {
                    matchedLocIds.push(part);
                  }
                });

                const currentLocs = (nextEmployments[targetIndex] as any).locationsId || [];
                
                const isLocChanged =
                  JSON.stringify([...currentLocs].sort()) !==
                  JSON.stringify([...matchedLocIds].sort());

                if (isLocChanged) {
                  (nextEmployments[targetIndex] as any).locationsId = matchedLocIds;
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
            setSuccessMessage(`اطلاعات ${updatedCount} کارمند با موفقیت از فایل اکسل اعمال شد.`);
            setTimeout(() => setSuccessMessage(null), 4000);
          } else {
            alert("هیچ رکوردی تغییر نکرد یا کد پرسنلی منطبقی پیدا نشد.");
          }

          return nextEmployments;
        });
      } catch (err: any) {
        setError("خطا در پردازش فایل اکسل: " + (err?.message || "فرمت فایل نامعتبر است"));
      } finally {
        e.target.value = "";
      }
    };

    reader.readAsArrayBuffer(file);
  };

  // تابع کمکی برای استخراج رشته عناوین مکان‌ها جهت استفاده در جستجو
  const getLocationNames = (emp: any): string => {
    if (!emp) return "";
    
    const locIds: string[] = emp.locationsId || [];
    
    // اگر locationsId پر باشد از روی locationMap عناوین را می‌خوانیم
    if (locIds.length > 0) {
      return locIds
        .map((id) => locationMap.get(id) || "")
        .filter(Boolean)
        .join(" ")
        .toLowerCase();
    }

    // اگر به هر دلیلی locationsId هنوز محاسبه نشده بود، از خود emp.locations می‌خوانیم
    if (Array.isArray(emp.locations)) {
      return emp.locations
        .map((l: Location) => l.title || "")
        .filter(Boolean)
        .join(" ")
        .toLowerCase();
    }

    return "";
  };

  // --- 4. فیلتر هوشمند مسطح ---
  const filteredEmployments = useMemo(() => {
    const normalizedGlobal = globalSearch.trim().toLowerCase();

    return employments.filter((emp: any) => {
      const initEmp: any = initialEmploymentsMap.get(emp.id);

      const empLocationName = getLocationNames(emp);
      const initLocationName = getLocationNames(initEmp);

      const matchesGlobal =
        !normalizedGlobal ||
        (emp.employmentCode || "").toLowerCase().includes(normalizedGlobal) ||
        (emp.firstName || "").toLowerCase().includes(normalizedGlobal) ||
        (emp.lastName || "").toLowerCase().includes(normalizedGlobal) ||
        (emp.nationalCode || "").toLowerCase().includes(normalizedGlobal) ||
        empLocationName.includes(normalizedGlobal) ||
        (initEmp &&
          ((initEmp.employmentCode || "").toLowerCase().includes(normalizedGlobal) ||
            (initEmp.firstName || "").toLowerCase().includes(normalizedGlobal) ||
            (initEmp.lastName || "").toLowerCase().includes(normalizedGlobal) ||
            (initEmp.nationalCode || "").toLowerCase().includes(normalizedGlobal) ||
            initLocationName.includes(normalizedGlobal)));

      let matchesColumns = true;
      for (const [col, term] of Object.entries(columnSearch)) {
        if (!term.trim()) continue;
        const q = term.toLowerCase();

        if (col === "employmentCode") {
          const matchCur = (emp.employmentCode || "").toLowerCase().includes(q);
          const matchInit = (initEmp?.employmentCode || "").toLowerCase().includes(q);
          if (!matchCur && !matchInit) matchesColumns = false;
        }
        if (col === "firstName") {
          const matchCur = (emp.firstName || "").toLowerCase().includes(q);
          const matchInit = (initEmp?.firstName || "").toLowerCase().includes(q);
          if (!matchCur && !matchInit) matchesColumns = false;
        }
        if (col === "lastName") {
          const matchCur = (emp.lastName || "").toLowerCase().includes(q);
          const matchInit = (initEmp?.lastName || "").toLowerCase().includes(q);
          if (!matchCur && !matchInit) matchesColumns = false;
        }
        if (col === "nationalCode") {
          const matchCur = (emp.nationalCode || "").toLowerCase().includes(q);
          const matchInit = (initEmp?.nationalCode || "").toLowerCase().includes(q);
          if (!matchCur && !matchInit) matchesColumns = false;
        }
        if (col === "locationsId" || col === "locations" || col === "locationId") {
          const matchCur = empLocationName.includes(q);
          const matchInit = initLocationName.includes(q);
          if (!matchCur && !matchInit) matchesColumns = false;
        }
      }

      return matchesGlobal && matchesColumns;
    });
  }, [employments, globalSearch, columnSearch, initialEmploymentsMap, locationMap]);

  // --- 5. لغو و ذخیره تغییرات ---
  const handleResetChanges = () => {
    if (window.confirm("آیا از لغو تمام تغییرات اعمال شده اطمینان دارید؟")) {
      setEmployments(JSON.parse(JSON.stringify(initialEmployments)));
      setModifiedIds(new Set());
    }
  };

  const handleSaveChanges = async () => {
    if (modifiedIds.size === 0) return;

    try {
      setSaving(true);
      setError(null);
      setSuccessMessage(null);

      const employmentsMap = new Map<string, EmploymentInfoView>();
      employments.forEach((emp) => employmentsMap.set(emp.id, emp));

      const commands: UpdateEmploymentCommand[] = Array.from(modifiedIds).map((id) => {
        const emp: any = employmentsMap.get(id)!;

        return {
          id: emp.id,
          employmentCode: emp.employmentCode || null,
          firstName: emp.firstName || null,
          lastName: emp.lastName || null,
          nationalCode: emp.nationalCode || null,
          locationsId: Array.isArray(emp.locationsId) ? emp.locationsId : [], // ارسال لیست Guid ها به DTO
        } as any;
      });

      await employmentApi.batchUpdateemployments(commands);

      setSuccessMessage(`تعداد ${commands.length} تغییر با موفقیت ذخیره شد.`);
      setInitialEmployments(JSON.parse(JSON.stringify(employments)));
      setModifiedIds(new Set());

      setTimeout(() => setSuccessMessage(null), 4000);
    } catch (err: any) {
      setError(err?.message || "خطا در ذخیره تغییرات اطلاعات کارمندان");
    } finally {
      setSaving(false);
    }
  };
  // --- 6. مودال حذف  ---
    const handleOpenDeleteModal = (loc: EmploymentItem) => {
      setDeleteTarget({
        id: loc.id,
        title: `${loc.firstName?.trim()} ${loc.lastName?.trim()}` || "بدون عنوان",
        isModified: modifiedIds.has(loc.id),
      });
    };
  
    const handleCloseDeleteModal = () => {
      if (isDeleting) return;
      setDeleteTarget(null);
    };
  
    const handleConfirmDelete = async () => {
      if (!deleteTarget) return;
  
      try {
        setIsDeleting(true);
        setError(null);
  
        await employmentApi.delete(deleteTarget.id);
  
        setSuccessMessage(`مکان "${deleteTarget.title}" با موفقیت حذف شد.`);
        setDeleteTarget(null);
  
        await loadData();
        setTimeout(() => setSuccessMessage(null), 4000);
      } catch (err: any) {
        setError(err?.message || "خطا در حذف مکان");
        setDeleteTarget(null);
      } finally {
        setIsDeleting(false);
      }
    };

  return {
    employments,    
    loading,
    initialEmployments,
    locations,
    saving,
    error,
    successMessage,
    deleteTarget,isDeleting,
    globalSearch,
    handleGlobalSearch,
    columnSearch,
    modifiedIds,
    fileInputRef,
    initialEmploymentsMap,
    locationMap,
    loadData,
    handleFieldChange,
    handleColumnSearch,
    handleExcelImport,
    filteredEmployments,
    handleResetChanges,
    handleSaveChanges,
    handleOpenDeleteModal,handleCloseDeleteModal,handleConfirmDelete
  };
};