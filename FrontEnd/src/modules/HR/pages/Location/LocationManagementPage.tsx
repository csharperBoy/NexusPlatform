// src/modules/HR/pages/location/LocationManagementPage.tsx

import React, { useEffect, useState, useMemo } from "react";
import { locationApi } from "../../api/LocationApi";
import { LocationInfoView } from "../../models/LocationInfoView";
import { UpdateLocationCommand, CreateLocationCommand } from "../../models/LocationCommand";

export const LocationManagementPage: React.FC = () => {
  // --- States ---
  const [locations, setLocations] = useState<LocationInfoView[]>([]);
  const [initialLocations, setInitialLocations] = useState<LocationInfoView[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [saving, setSaving] = useState<boolean>(false);
  
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // استیت‌های سرچ
  const [columnSearch, setColumnSearch] = useState<Record<string, string>>({});

  // مدیریت تغییرات ویرایش درجا
  const [modifiedIds, setModifiedIds] = useState<Set<string>>(new Set());

  // --- استیت‌های مودال افزودن ---
  const [isAddModalOpen, setIsAddModalOpen] = useState<boolean>(false);
  const [newLocationTitle, setNewLocationTitle] = useState<string>("");
  const [isSubmittingNew, setIsSubmittingNew] = useState<boolean>(false);
  const [addModalError, setAddModalError] = useState<string | null>(null);

  // --- استیت‌های مودال حذف ---
  const [deleteTarget, setDeleteTarget] = useState<{ id: string; title: string; isModified: boolean } | null>(null);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);

  const initialLocationsMap = useMemo(() => {
    const map = new Map<string, LocationInfoView>();
    initialLocations.forEach((loc) => map.set(loc.id, loc));
    return map;
  }, [initialLocations]);

  // --- 1. دریافت اطلاعات اولیه ---
  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await locationApi.GetList();
      const list = data || [];
      setLocations(list);
      setInitialLocations(JSON.parse(JSON.stringify(list)));
      setModifiedIds(new Set());
    } catch (err: any) {
      setError(err?.message || "خطا در دریافت لیست مکان‌ها");
    } finally {
      setLoading(false);
    }
  };

  // --- 2. مدیریت ویرایش درجا ---
  const handleFieldChange = (id: string, field: "title", value: string) => {
    setLocations((prev) =>
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

  // --- 3. فیلتر هوشمند ---
  const filteredLocations = useMemo(() => {
    return locations.filter((loc) => {
      const initLoc = initialLocationsMap.get(loc.id);

      let matchesColumns = true;
      for (const [col, term] of Object.entries(columnSearch)) {
        if (!term.trim()) continue;
        const q = term.toLowerCase();

        if (col === "title") {
          const matchCur = (loc.title || "").toLowerCase().includes(q);
          const matchInit = (initLoc?.title || "").toLowerCase().includes(q);
          if (!matchCur && !matchInit) matchesColumns = false;
        }
      }

      return matchesColumns;
    });
  }, [locations, columnSearch, initialLocationsMap]);

  // --- 4. لغو و ذخیره تغییرات ویرایش درجا ---
  const handleResetChanges = () => {
    setLocations(JSON.parse(JSON.stringify(initialLocations)));
    setModifiedIds(new Set());
  };

  const handleSaveChanges = async () => {
    if (modifiedIds.size === 0) return;

    try {
      setSaving(true);
      setError(null);
      setSuccessMessage(null);

      const locationsMap = new Map<string, LocationInfoView>();
      locations.forEach((loc) => locationsMap.set(loc.id, loc));

      const commands: UpdateLocationCommand[] = Array.from(modifiedIds).map((id) => {
        const loc = locationsMap.get(id)!;
        return {
          id: loc.id,
          title: loc.title || null,
        };
      });

      await locationApi.batchUpdatelocations(commands);

      setSuccessMessage(`تعداد ${commands.length} تغییر با موفقیت ذخیره شد.`);
      setInitialLocations(JSON.parse(JSON.stringify(locations)));
      setModifiedIds(new Set());

      setTimeout(() => setSuccessMessage(null), 4000);
    } catch (err: any) {
      setError(err?.message || "خطا در ذخیره تغییرات اطلاعات مکان‌ها");
    } finally {
      setSaving(false);
    }
  };

  // --- 5. مودال افزودن مکان جدید ---
  const handleOpenAddModal = () => {
    setNewLocationTitle("");
    setAddModalError(null);
    setIsAddModalOpen(true);
  };

  const handleCloseAddModal = () => {
    if (isSubmittingNew) return;
    setIsAddModalOpen(false);
    setNewLocationTitle("");
    setAddModalError(null);
  };

  const handleCreateLocation = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newLocationTitle.trim()) {
      setAddModalError("لطفاً عنوان مکان را وارد کنید.");
      return;
    }

    try {
      setIsSubmittingNew(true);
      setAddModalError(null);

      const payload: CreateLocationCommand = {
        title: newLocationTitle.trim(),
      };

      await locationApi.create(payload);

      setSuccessMessage("مکان جدید با موفقیت ثبت شد.");
      setIsAddModalOpen(false);
      setNewLocationTitle("");
      
      await loadData();
      setTimeout(() => setSuccessMessage(null), 4000);
    } catch (err: any) {
      setAddModalError(err?.message || "خطا در ایجاد مکان جدید");
    } finally {
      setIsSubmittingNew(false);
    }
  };

  // --- 6. مودال حذف مکان ---
  const handleOpenDeleteModal = (loc: LocationInfoView) => {
    setDeleteTarget({
      id: loc.id,
      title: loc.title?.trim() || "بدون عنوان",
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

      await locationApi.delete(deleteTarget.id);

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

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[400px] text-gray-500 font-sans">
        در حال دریافت اطلاعات لیست مکان‌ها...
      </div>
    );
  }

  return (
    <div className="p-6 dir-rtl text-right font-sans bg-gray-50/50 min-h-screen">
      {/* هدر اصلی */}
      <div className="bg-white p-5 rounded-xl border border-gray-200 shadow-sm mb-5">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-gray-800 mb-1">مدیریت اطلاعات ارتباطی مکان‌ها</h1>
            <p className="text-sm text-gray-500">
              کل مکان‌ها: <span className="font-semibold text-gray-700">{locations.length}</span>
              {modifiedIds.size > 0 && (
                <span className="mr-3 text-amber-600 bg-amber-50 px-2 py-0.5 rounded border border-amber-200 text-xs font-medium">
                  {modifiedIds.size} تغییر ذخیره‌نشده
                </span>
              )}
            </p>
          </div>

          <div className="flex items-center gap-3">
            <button
              type="button"
              onClick={handleOpenAddModal}
              className="px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-sm font-medium shadow-sm transition-all flex items-center gap-1.5 cursor-pointer"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              افزودن مکان جدید
            </button>

            {modifiedIds.size > 0 && (
              <button
                type="button"
                onClick={handleResetChanges}
                disabled={saving}
                className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg text-sm hover:bg-gray-100 transition-colors disabled:opacity-50 cursor-pointer"
              >
                انصراف و بازنشانی
              </button>
            )}

            <button
              type="button"
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
      </div>

      {/* جدول مکان‌ها */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
        <table className="w-full text-right border-collapse">
          <thead>
            <tr className="border-b border-gray-200 text-gray-700 text-xs font-semibold">
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-3 w-12 text-center h-[38px] border-b border-gray-200 shadow-sm">
                ردیف
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 h-[38px] border-b border-gray-200 shadow-sm">
                عنوان
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 text-center w-28 h-[38px] border-b border-gray-200 shadow-sm">
                وضعیت
              </th>
              <th className="sticky top-0 z-20 bg-gray-100 py-2 px-4 text-center w-24 h-[38px] border-b border-gray-200 shadow-sm">
                عملیات
              </th>
            </tr>

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
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 border-b border-gray-200 shadow-sm"></th>
              <th className="sticky top-[38px] z-20 bg-gray-50 py-1.5 px-2 border-b border-gray-200 shadow-sm"></th>
            </tr>
          </thead>

          <tbody className="divide-y divide-gray-100 text-sm">
            {filteredLocations.length === 0 ? (
              <tr>
                <td colSpan={4} className="text-center py-12 text-gray-400">
                  هیچ مکانی یافت نشد.
                </td>
              </tr>
            ) : (
              filteredLocations.map((loc, index) => {
                const isModified = modifiedIds.has(loc.id);

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

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={loc.title || ""}
                        onChange={(e) => handleFieldChange(loc.id, "title", e.target.value)}
                        placeholder="عنوان..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 text-right outline-none bg-white hover:border-gray-400 transition-colors"
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

                    <td className="py-2 px-3 text-center">
                      <button
                        type="button"
                        onClick={() => handleOpenDeleteModal(loc)}
                        title="حذف مکان"
                        className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer"
                      >
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.8} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                        </svg>
                      </button>
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* --- مودال افزودن مکان جدید --- */}
      {isAddModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
          <div 
            className="bg-white rounded-xl shadow-xl w-full max-w-md overflow-hidden animate-in fade-in zoom-in duration-150"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="px-5 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50/50">
              <h3 className="font-bold text-gray-800 text-base">افزودن مکان جدید</h3>
              <button
                type="button"
                onClick={handleCloseAddModal}
                disabled={isSubmittingNew}
                className="text-gray-400 hover:text-gray-600 p-1 rounded-lg transition-colors cursor-pointer"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <form onSubmit={handleCreateLocation}>
              <div className="p-5 space-y-4">
                {addModalError && (
                  <div className="p-3 bg-red-50 border border-red-200 text-red-700 text-xs rounded-lg">
                    {addModalError}
                  </div>
                )}

                <div>
                  <label className="block text-xs font-semibold text-gray-700 mb-1.5">
                    عنوان مکان <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    autoFocus
                    placeholder="مثلاً: دفتر مرکزی، انبار ۲، ..."
                    value={newLocationTitle}
                    onChange={(e) => setNewLocationTitle(e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 outline-none transition-all"
                  />
                </div>
              </div>

              <div className="px-5 py-3.5 bg-gray-50 border-t border-gray-100 flex items-center justify-end gap-2">
                <button
                  type="button"
                  onClick={handleCloseAddModal}
                  disabled={isSubmittingNew}
                  className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg text-xs hover:bg-gray-100 transition-colors cursor-pointer"
                >
                  انصراف
                </button>
                <button
                  type="submit"
                  disabled={isSubmittingNew || !newLocationTitle.trim()}
                  className="px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-medium shadow-sm transition-all disabled:opacity-50 cursor-pointer flex items-center gap-1.5"
                >
                  {isSubmittingNew ? "در حال ثبت..." : "ثبت مکان"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* --- مودال تأیید حذف --- */}
      {deleteTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
          <div 
            className="bg-white rounded-xl shadow-xl w-full max-w-sm overflow-hidden animate-in fade-in zoom-in duration-150"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="p-6 text-center">
              {/* آیکون اخطار حذف */}
              <div className="w-12 h-12 rounded-full bg-red-100 text-red-600 mx-auto flex items-center justify-center mb-4">
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
              </div>

              <h3 className="font-bold text-gray-800 text-lg mb-2">تأیید حذف مکان</h3>
              
              <p className="text-sm text-gray-600 leading-relaxed mb-3">
                آیا از حذف مکان <span className="font-semibold text-gray-900">«{deleteTarget.title}»</span> اطمینان دارید؟
              </p>

              {deleteTarget.isModified && (
                <div className="mb-4 p-2.5 bg-amber-50 border border-amber-200 rounded-lg text-amber-800 text-xs">
                  این سطر دارای تغییرات ذخیره‌نشده است. با حذف آن، تغییرات نیز از بین خواهند رفت.
                </div>
              )}

              <p className="text-xs text-gray-400">این عملیات قابل بازگشت نیست.</p>
            </div>

            {/* دکمه‌های مودال حذف */}
            <div className="px-5 py-3.5 bg-gray-50 border-t border-gray-100 flex items-center justify-center gap-3">
              <button
                type="button"
                onClick={handleCloseDeleteModal}
                disabled={isDeleting}
                className="w-full py-2 border border-gray-300 text-gray-700 rounded-lg text-xs font-medium hover:bg-gray-100 transition-colors cursor-pointer"
              >
                انصراف
              </button>
              <button
                type="button"
                onClick={handleConfirmDelete}
                disabled={isDeleting}
                className="w-full py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg text-xs font-medium shadow-sm transition-all disabled:opacity-50 cursor-pointer flex items-center justify-center gap-1.5"
              >
                {isDeleting ? "در حال حذف..." : "حذف شود"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default LocationManagementPage;