// src/modules/HR/pages/locationContact/LocationContactManagementPage.tsx

import React, { useEffect, useState, useMemo, useRef } from "react";
import * as XLSX from "xlsx";
import { locationContactApi } from "../../api/LocationContactApi";
import { LocationContactInfoView } from "../../models/LocationContactInfoView";
import { UpdateLocationContactCommand } from "../../models/LocationContactCommand";

export const LocationContactManagementPage: React.FC = () => {
  // --- States ---
  const [locationContacts, setLocationContacts] = useState<LocationContactInfoView[]>([]);
  const [initialLocationContacts, setInitialLocationContacts] = useState<LocationContactInfoView[]>([]);
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

  const initialLocationContactsMap = useMemo(() => {
    const map = new Map<string, LocationContactInfoView>();
    initialLocationContacts.forEach((loc) => map.set(loc.id, loc));
    return map;
  }, [initialLocationContacts]);

  // --- 1. دریافت اطلاعات اولیه ---
  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await locationContactApi.GetList();
      const list = data || [];
      setLocationContacts(list);
      setInitialLocationContacts(JSON.parse(JSON.stringify(list)));
      setModifiedIds(new Set());
    } catch (err: any) {
      setError(err?.message || "خطا در دریافت لیست مکانان");
    } finally {
      setLoading(false);
    }
  };

  // --- 2. مدیریت ویرایش درجا ---
  const handleFieldChange = (id: string, field:"title" | "orgPhone" | "orgMobile", value: string) => {
    setLocationContacts((prev) =>
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


  // --- 4. فیلتر هوشمند مسطح ---
  const filteredLocationContacts = useMemo(() => {
    const normalizedGlobal = globalSearch.trim().toLowerCase();

    return locationContacts.filter((loc) => {
      const initLoc = initialLocationContactsMap.get(loc.id);

      
      const matchesGlobal =
        !normalizedGlobal ||
        (loc.title || "").toLowerCase().includes(normalizedGlobal) ||
        (loc.orgPhone || "").toLowerCase().includes(normalizedGlobal) ||
        (loc.orgMobile || "").toLowerCase().includes(normalizedGlobal) ||
        (initLoc &&
          (
            (initLoc.title || "").toLowerCase().includes(normalizedGlobal) ||
            (initLoc.orgPhone || "").toLowerCase().includes(normalizedGlobal) ||
            (initLoc.orgMobile || "").toLowerCase().includes(normalizedGlobal)));

      let matchesColumns = true;
      for (const [col, term] of Object.entries(columnSearch)) {
        if (!term.trim()) continue;
        const q = term.toLowerCase();

        
        if (col === "title") {
          const matchCur = (loc.title || "").toLowerCase().includes(q);
          const matchInit = (initLoc?.title || "").toLowerCase().includes(q);
          if (!matchCur && !matchInit) matchesColumns = false;
        }
        if (col === "orgPhone") {
          const matchCur = (loc.orgPhone || "").toLowerCase().includes(q);
          const matchInit = (initLoc?.orgPhone || "").toLowerCase().includes(q);
          if (!matchCur && !matchInit) matchesColumns = false;
        }
        if (col === "orgMobile") {
          const matchCur = (loc.orgMobile || "").toLowerCase().includes(q);
          const matchInit = (initLoc?.orgMobile || "").toLowerCase().includes(q);
          if (!matchCur && !matchInit) matchesColumns = false;
        }
      }

      return matchesGlobal && matchesColumns;
    });
  }, [locationContacts, globalSearch, columnSearch, initialLocationContactsMap]);

  // --- 5. لغو و ذخیره تغییرات ---
  const handleResetChanges = () => {
    if (window.confirm("آیا از لغو تمام تغییرات اعمال شده اطمینان دارید؟")) {
      setLocationContacts(JSON.parse(JSON.stringify(initialLocationContacts)));
      setModifiedIds(new Set());
    }
  };

  const handleSaveChanges = async () => {
    if (modifiedIds.size === 0) return;

    try {
      setSaving(true);
      setError(null);
      setSuccessMessage(null);

      const locationContactsMap = new Map<string, LocationContactInfoView>();
      locationContacts.forEach((loc) => locationContactsMap.set(loc.id, loc));

      const commands: UpdateLocationContactCommand[] = Array.from(modifiedIds).map((id) => {
        const loc = locationContactsMap.get(id)!;
        return {
          id: loc.id,
          title: loc.title || null,
          officePhone: loc.orgPhone || null,
          orgMobile: loc.orgMobile || null,
        };
      });

      await locationContactApi.batchUpdateLocationsContact(commands);

      setSuccessMessage(`تعداد ${commands.length} تغییر با موفقیت ذخیره شد.`);
      setInitialLocationContacts(JSON.parse(JSON.stringify(locationContacts)));
      setModifiedIds(new Set());

      setTimeout(() => setSuccessMessage(null), 4000);
    } catch (err: any) {
      setError(err?.message || "خطا در ذخیره تغییرات اطلاعات مکانان");
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[400px] text-gray-500 font-sans">
        در حال دریافت اطلاعات لیست مکانان...
      </div>
    );
  }

  return (
    <div className="p-6 dir-rtl text-right font-sans bg-gray-50/50 min-h-screen">
      {/* هدر اصلی */}
      <div className="bg-white p-5 rounded-xl border border-gray-200 shadow-sm mb-5">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-gray-800 mb-1">مدیریت اطلاعات ارتباطی مکانان</h1>
            <p className="text-sm text-gray-500">
              کل مکانان: <span className="font-semibold text-gray-700">{locationContacts.length}</span>
              {modifiedIds.size > 0 && (
                <span className="mr-3 text-amber-600 bg-amber-50 px-2 py-0.5 rounded border border-amber-200 text-xs font-medium">
                  {modifiedIds.size} تغییر ذخیره‌نشده
                </span>
              )}
            </p>
          </div>

          <div className="flex items-center gap-3">
           

          

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

      {/* جدول مکان ها با هدرهای چسبان */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
        <table className="w-full text-right border-collapse">
          <thead>
            {/* ردیف اول: عناوین ستون‌ها (موقعیت چسبان top-0 با ارتفاع 38px) */}
            <tr className="border-b border-gray-200 text-gray-700 text-xs font-semibold">
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-3 w-12 text-center h-[38px] border-b border-gray-200 shadow-sm">
                ردیف
              </th>
              
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 h-[38px] border-b border-gray-200 shadow-sm">
               عنوان
              </th>
             
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 w-44 h-[38px] border-b border-gray-200 shadow-sm">
                تلفن داخلی
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 w-48 h-[38px] border-b border-gray-200 shadow-sm">
                موبایل سازمانی
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 text-center w-28 h-[38px] border-b border-gray-200 shadow-sm">
                وضعیت
              </th>
            </tr>

            {/* ردیف دوم: اینپوت‌های سرچ ستونی (موقعیت چسبان top-[38px]) */}
            <tr className="border-b border-gray-200">
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 border-b border-gray-200 shadow-sm"></th>
              
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ عنوان..."
                  value={columnSearch["title"] || ""}
                  onChange={(e) => handleColumnSearch("title", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ داخلی..."
                  value={columnSearch["orgPhone"] || ""}
                  onChange={(e) => handleColumnSearch("orgPhone", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500 font-mono"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ موبایل..."
                  value={columnSearch["orgMobile"] || ""}
                  onChange={(e) => handleColumnSearch("orgMobile", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500 font-mono"
                />
              </th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 border-b border-gray-200 shadow-sm"></th>
            </tr>
          </thead>

          <tbody className="divide-y divide-gray-100 text-sm">
            {filteredLocationContacts.length === 0 ? (
              <tr>
                <td colSpan={7} className="text-center py-12 text-gray-400">
                  هیچ مکانی یافت نشد.
                </td>
              </tr>
            ) : (
              filteredLocationContacts.map((loc, index) => {
                const isModified = modifiedIds.has(loc.id);
                const title = loc.title.trim() || "-";

                return (
                  <tr
                    key={loc.id}
                    className={`transition-colors hover:bg-gray-50/80 ${
                      isModified ? "bg-amber-50/40" : ""
                    }`}
                  >
                    <td className="py-3 px-3 text-center text-xs text-gray-400 font-mono">
                      {index + 1}
                    </td>

                   

                    {/* <td className="py-3 px-4 font-medium text-gray-800">
                      {title}
                    </td> */}

                   <td className="py-2 px-3">
                      <input
                        type="text"
                        value={loc.title || ""}
                        onChange={(e) => handleFieldChange(loc.id, "title", e.target.value)}
                        placeholder="عنوان..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 font-mono text-center dir-ltr outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={loc.orgPhone || ""}
                        onChange={(e) => handleFieldChange(loc.id, "orgPhone", e.target.value)}
                        placeholder="داخلی..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 font-mono text-center dir-ltr outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={loc.orgMobile || ""}
                        onChange={(e) => handleFieldChange(loc.id, "orgMobile", e.target.value)}
                        placeholder="موبایل..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 font-mono text-center dir-ltr outline-none bg-white hover:border-gray-400 transition-colors"
                      />
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

export default LocationContactManagementPage;