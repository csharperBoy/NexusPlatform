// src/modules/HR/pages/employment/EmploymentManagementPage.tsx

import React, { useEffect, useState, useMemo, useRef } from "react";
import * as XLSX from "xlsx";
import { employmentApi } from "../../api/EmploymentApi";
import { locationApi } from "../../api/LocationApi"; // وارد کردن API مکان‌ها
import { EmploymentInfoView } from "../../models/EmploymentInfoView";
import { UpdateEmploymentCommand } from "../../models/EmploymentCommand";
import { SelectionListDto } from "@/core/models/SelectionListDto"; // وارد کردن مدل SelectionListDto

type EditableField =
  | "employmentCode"
  | "firstName"
  | "lastName"
  | "nationalCode"
  | "locationId"; // افزودن فیلد locationId

export const EmploymentManagementPage: React.FC = () => {
  // --- States ---
  const [employments, setEmployments] = useState<EmploymentInfoView[]>([]);
  const [initialEmployments, setInitialEmployments] = useState<EmploymentInfoView[]>([]);
  const [locations, setLocations] = useState<SelectionListDto[]>([]); // استیت گزینه‌های مکان
  const [loading, setLoading] = useState<boolean>(true);
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

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

      // دریافت هم‌زمان کارمندان و لیست مکان‌ها
      const [empData, locationList] = await Promise.all([
        employmentApi.GetList(),
        locationApi.GetSelectionList(),
      ]);

      const list = empData || [];
      
      // مپ کردن اولیه locationsId به locationId برای کار ساده‌تر با Dropdown
      const normalizedList = list.map((emp: any) => ({
        ...emp,
        locationId: emp.locationId || (Array.isArray(emp.locationsId) && emp.locationsId.length > 0 ? emp.locationsId[0] : ""),
      }));

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
  const handleFieldChange = (id: string, field: EditableField, value: string) => {
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
              ["محل استقرار", "مکان", "محل_استقرار", "location", "locationid"].includes(
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

              // پشتیبانی از مپ کردن محل استقرار بر اساس عنوان یا ID
              if (locationKey && row[locationKey] !== undefined) {
                const rawLoc = String(row[locationKey] ?? "").trim();
                const matchedLocation = locations.find(
                  (l) =>
                    l.value.toLowerCase() === rawLoc.toLowerCase() ||
                    l.display.toLowerCase() === rawLoc.toLowerCase() ||
                    l.label.toLowerCase() === rawLoc.toLowerCase()
                );

                const newLocId = matchedLocation ? matchedLocation.value : rawLoc;
                if ((nextEmployments[targetIndex] as any).locationId !== newLocId) {
                  (nextEmployments[targetIndex] as any).locationId = newLocId;
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

  // --- 4. فیلتر هوشمند مسطح ---
  const filteredEmployments = useMemo(() => {
    const normalizedGlobal = globalSearch.trim().toLowerCase();

    return employments.filter((emp: any) => {
      const initEmp: any = initialEmploymentsMap.get(emp.id);

      const empLocationName = (locationMap.get(emp.locationId || "") || "").toLowerCase();
      const initLocationName = (locationMap.get(initEmp?.locationId || "") || "").toLowerCase();

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
        if (col === "locationId") {
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
        
        // مپ کردن آی‌دی مکان به لیست locationsId جهت ارسال به بک‌اند C# (List<Guid>)
        const selectedLocationId = emp.locationId || null;
        const locationsIdList = selectedLocationId ? [selectedLocationId] : [];

        return {
          id: emp.id,
          employmentCode: emp.employmentCode || null,
          firstName: emp.firstName || null,
          lastName: emp.lastName || null,
          nationalCode: emp.nationalCode || null,
          locationsId: locationsIdList, // ساختار مورد نیاز DTO بک‌اند
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

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[400px] text-gray-500 font-sans">
        در حال دریافت اطلاعات لیست کارمندان و مکان‌ها...
      </div>
    );
  }

  return (
    <div className="p-6 dir-rtl text-right font-sans bg-gray-50/50 min-h-screen">
      {/* هدر اصلی */}
      <div className="bg-white p-5 rounded-xl border border-gray-200 shadow-sm mb-5">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-gray-800 mb-1">مدیریت و ویرایش اطلاعات کارمندان</h1>
            <p className="text-sm text-gray-500">
              کل کارمندان: <span className="font-semibold text-gray-700">{employments.length}</span>
              {modifiedIds.size > 0 && (
                <span className="mr-3 text-amber-600 bg-amber-50 px-2 py-0.5 rounded border border-amber-200 text-xs font-medium">
                  {modifiedIds.size} تغییر ذخیره‌نشده
                </span>
              )}
            </p>
          </div>

          <div className="flex items-center gap-3">
            <input
              type="file"
              ref={fileInputRef}
              onChange={handleExcelImport}
              accept=".xlsx, .xls"
              className="hidden"
            />

            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              disabled={saving}
              className="px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-sm font-medium transition-colors disabled:opacity-50 flex items-center gap-2 cursor-pointer shadow-sm"
              title="بارگذاری اکسل جهت به‌روزرسانی اطلاعات کارمندان"
            >
              📊 بارگذاری از اکسل
            </button>

            {modifiedIds.size > 0 && (
              <button
                onClick={handleResetChanges}
                disabled={saving}
                className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg text-sm hover:bg-gray-100 transition-colors disabled:opacity-50"
              >
                انصراف و بازنشانی
              </button>
            )}

            <button
              onClick={handleSaveChanges}
              disabled={modifiedIds.size === 0 || saving}
              className={`px-5 py-2 rounded-lg text-sm font-medium shadow-sm transition-all flex items-center gap-2 ${
                modifiedIds.size > 0
                  ? "bg-blue-600 hover:bg-blue-700 text-white cursor-pointer"
                  : "bg-gray-200 text-gray-400 cursor-not-allowed"
              }`}
            >
              {saving ? "در حال ذخیره..." : "ذخیره تغییرات"}
            </button>
          </div>
        </div>

        {error && (
          <div className="mt-4 p-3 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg">
            {error}
          </div>
        )}
        {successMessage && (
          <div className="mt-4 p-3 bg-green-50 border border-green-200 text-green-700 text-sm rounded-lg">
            {successMessage}
          </div>
        )}

        {/* سرچ اصلی */}
        <div className="flex items-center justify-between gap-4 mt-5 pt-4 border-t border-gray-100">
          <div className="w-72">
            <input
              type="text"
              placeholder="جستجوی کلی در تمام فیلدها..."
              value={globalSearch}
              onChange={(e) => setGlobalSearch(e.target.value)}
              className="w-full px-3 py-1.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 outline-none"
            />
          </div>
        </div>
      </div>

      {/* جدول کارمندان */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
        <table className="w-full text-right border-collapse">
          <thead>
            {/* ردیف اول: عناوین ستون‌ها */}
            <tr className="border-b border-gray-200 text-gray-700 text-xs font-semibold">
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-3 w-12 text-center h-[38px] border-b border-gray-200 shadow-sm">
                ردیف
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 w-36 h-[38px] border-b border-gray-200 shadow-sm">
                کد پرسنلی
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 h-[38px] border-b border-gray-200 shadow-sm">
                نام
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 h-[38px] border-b border-gray-200 shadow-sm">
                نام خانوادگی
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 w-44 h-[38px] border-b border-gray-200 shadow-sm">
                کد ملی
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 w-52 h-[38px] border-b border-gray-200 shadow-sm">
                محل استقرار
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 text-center w-28 h-[38px] border-b border-gray-200 shadow-sm">
                وضعیت
              </th>
            </tr>

            {/* ردیف دوم: اینپوت‌های سرچ ستونی */}
            <tr className="border-b border-gray-200">
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 border-b border-gray-200 shadow-sm"></th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ کد پرسنلی..."
                  value={columnSearch["employmentCode"] || ""}
                  onChange={(e) => handleColumnSearch("employmentCode", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500 font-mono"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ نام..."
                  value={columnSearch["firstName"] || ""}
                  onChange={(e) => handleColumnSearch("firstName", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ نام خانوادگی..."
                  value={columnSearch["lastName"] || ""}
                  onChange={(e) => handleColumnSearch("lastName", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ کد ملی..."
                  value={columnSearch["nationalCode"] || ""}
                  onChange={(e) => handleColumnSearch("nationalCode", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500 font-mono"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ محل استقرار..."
                  value={columnSearch["locationId"] || ""}
                  onChange={(e) => handleColumnSearch("locationId", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 border-b border-gray-200 shadow-sm"></th>
            </tr>
          </thead>

          <tbody className="divide-y divide-gray-100 text-sm">
            {filteredEmployments.length === 0 ? (
              <tr>
                <td colSpan={7} className="text-center py-12 text-gray-400">
                  هیچ کارمندی یافت نشد.
                </td>
              </tr>
            ) : (
              filteredEmployments.map((emp: any, index) => {
                const isModified = modifiedIds.has(emp.id);

                return (
                  <tr
                    key={emp.id}
                    className={`transition-colors hover:bg-gray-50/80 ${
                      isModified ? "bg-amber-50/40" : ""
                    }`}
                  >
                    <td className="py-3 px-3 text-center text-xs text-gray-400 font-mono">
                      {index + 1}
                    </td>

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={emp.employmentCode || ""}
                        onChange={(e) => handleFieldChange(emp.id, "employmentCode", e.target.value)}
                        placeholder="کد پرسنلی..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 font-mono text-center dir-ltr outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={emp.firstName || ""}
                        onChange={(e) => handleFieldChange(emp.id, "firstName", e.target.value)}
                        placeholder="نام..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 text-right outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={emp.lastName || ""}
                        onChange={(e) => handleFieldChange(emp.id, "lastName", e.target.value)}
                        placeholder="نام خانوادگی..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 text-right outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={emp.nationalCode || ""}
                        onChange={(e) => handleFieldChange(emp.id, "nationalCode", e.target.value)}
                        placeholder="کد ملی..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 font-mono text-center dir-ltr outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    {/* ستون انتخاب محل استقرار */}
                    <td className="py-2 px-3">
                      <select
                        value={emp.locationId || ""}
                        onChange={(e) => handleFieldChange(emp.id, "locationId", e.target.value)}
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 text-right outline-none bg-white hover:border-gray-400 transition-colors cursor-pointer"
                      >
                        <option value="">-- انتخاب محل استقرار --</option>
                        {locations.map((loc) => (
                          <option key={loc.value} value={loc.value}>
                            {loc.display || loc.label}
                          </option>
                        ))}
                      </select>
                    </td>

                    <td className="py-3 px-4 text-center">
                      {isModified ? (
                        <span className="inline-block text-[10px] bg-amber-100 text-amber-800 border border-amber-300 px-2 py-0.5 rounded-full font-medium">
                          تغییر یافته
                        </span>
                      ) : (
                        <span className="text-gray-300 text-xs">-</span>
                      )}
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default EmploymentManagementPage;