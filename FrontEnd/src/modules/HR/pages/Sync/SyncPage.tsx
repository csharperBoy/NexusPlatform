// src/modules/HR/pages/Sync/SyncPage.tsx
import React, { useState, useEffect } from "react";
import { SyncApi } from "../../api/SyncApi";
import {
  SyncResult,
  entityLabels,
  SyncEntityKey,
  SyncCommandBundle,
  SyncPreviewItem,
} from "../../models/SyncModels";
import { AxiosError } from "axios";
import { BatchResult } from "@/core/models/apiResults";

// ---------------------------------------------------------
// 1. Types & Configs
// ---------------------------------------------------------

type EntityConfig = {
  key: SyncEntityKey;
  label: string;
  previewApi: () => Promise<BatchResult<SyncCommandBundle>>;
  applyApi: (bundle: SyncCommandBundle) => Promise<BatchResult<SyncResult>>;
};

const entityConfigs: EntityConfig[] = [
  { key: "orgUnit", label: entityLabels.orgUnit, previewApi: SyncApi.SyncOrganizationUnitPreview, applyApi: SyncApi.ApplyOrganizationUnit },
  { key: "jobLevel", label: entityLabels.jobLevel, previewApi: SyncApi.SyncJobLevelPreview, applyApi: SyncApi.ApplyJobLevel },
  { key: "jobTitle", label: entityLabels.jobTitle, previewApi: SyncApi.SyncJobTitlePreview, applyApi: SyncApi.ApplyJobTitle },
  { key: "employment", label: entityLabels.employment, previewApi: SyncApi.SyncEmploymentsPreview, applyApi: SyncApi.ApplyEmployments },
  { key: "post", label: entityLabels.post, previewApi: SyncApi.SyncPostPreview, applyApi: SyncApi.ApplyPost },
  { key: "assignment", label: entityLabels.assignment, previewApi: SyncApi.SyncAssignmentsPreview, applyApi: SyncApi.ApplyAssignments },
];

type SyncStep = "idle" | "loading_preview" | "preview" | "applying" | "result" | "error";

// ---------------------------------------------------------
// 2. Helper Components (Icons & UI)
// ---------------------------------------------------------

const Spinner = () => (
  <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white"></div>
);

// ---------------------------------------------------------
// 3. Sub-Component: Entity Sync Card
// ---------------------------------------------------------

const EntitySyncCard: React.FC<{ config: EntityConfig }> = ({ config }) => {
  const [step, setStep] = useState<SyncStep>("idle");
  const [bundle, setBundle] = useState<SyncCommandBundle | null>(null);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [applyResult, setApplyResult] = useState<BatchResult<SyncResult> | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const getErrorMessage = (err: unknown): string => {
    if (err instanceof AxiosError) {
      if (err.response?.data) {
        return typeof err.response.data === "string"
          ? err.response.data
          : JSON.stringify(err.response.data);
      }
      return "خطا در برقراری ارتباط با سرور.";
    }
    return "خطای ناشناخته رخ داده است.";
  };

  // --- Actions ---

  const handlePreview = async () => {
    setStep("loading_preview");
    setErrorMessage(null);
    try {
      const res = await config.previewApi();
      if (res.succeeded && res.data) {
        setBundle(res.data);
        // به صورت پیش‌فرض همه آیتم‌ها را انتخاب می‌کنیم
        const allIds = [
          ...res.data.addCommands.map((i) => i.id),
          ...res.data.updateCommands.map((i) => i.id),
          ...res.data.deleteCommands.map((i) => i.id),
        ];
        setSelectedIds(new Set(allIds));
        setStep("preview");
      } else {
        setErrorMessage(res.errors?.join(" - ") || "خطا در دریافت پیش‌نمایش");
        setStep("error");
      }
    } catch (err) {
      setErrorMessage(getErrorMessage(err));
      setStep("error");
    }
  };

  const handleApply = async () => {
    if (!bundle) return;
    setStep("applying");
    setErrorMessage(null);

    // فیلتر کردن باندل فقط بر اساس آیتم‌های تیک‌خورده
    const filteredBundle: SyncCommandBundle = {
      addCommands: bundle.addCommands.filter((i) => selectedIds.has(i.id)),
      updateCommands: bundle.updateCommands.filter((i) => selectedIds.has(i.id)),
      deleteCommands: bundle.deleteCommands.filter((i) => selectedIds.has(i.id)),
      warnings: bundle.warnings,
    };

    try {
      const res = await config.applyApi(filteredBundle);
      setApplyResult(res);
      setStep("result");
    } catch (err) {
      setErrorMessage(getErrorMessage(err));
      setStep("error");
    }
  };

  const reset = () => {
    setStep("idle");
    setBundle(null);
    setApplyResult(null);
    setSelectedIds(new Set());
    setErrorMessage(null);
  };

  // --- Toggles ---

  const toggleSelection = (id: string) => {
    const newSet = new Set(selectedIds);
    if (newSet.has(id)) newSet.delete(id);
    else newSet.add(id);
    setSelectedIds(newSet);
  };

  const totalChanges = bundle
    ? bundle.addCommands.length + bundle.updateCommands.length + bundle.deleteCommands.length
    : 0;

  // --- Renders ---

  const renderChangesList = (title: string, items: SyncPreviewItem[], type: "add" | "update" | "delete") => {
    if (items.length === 0) return null;

    const colors = {
      add: "bg-emerald-50 border-emerald-200 text-emerald-800",
      update: "bg-blue-50 border-blue-200 text-blue-800",
      delete: "bg-rose-50 border-rose-200 text-rose-800",
    };

    return (
      <div className={`mt-4 border rounded-lg p-3 ${colors[type]}`}>
        <h4 className="font-bold mb-2 flex items-center justify-between">
          <span>{title} ({items.length})</span>
        </h4>
        <div className="space-y-2 max-h-60 overflow-y-auto pr-2 custom-scrollbar">
          {items.map((item) => (
            <label
              key={item.id}
              className="flex items-start gap-3 p-2 bg-white rounded border border-white/50 hover:border-gray-300 cursor-pointer transition shadow-sm"
            >
              <input
                type="checkbox"
                checked={selectedIds.has(item.id)}
                onChange={() => toggleSelection(item.id)}
                className="mt-1 w-4 h-4 text-blue-600 rounded focus:ring-blue-500"
              />
              <span className="text-sm leading-relaxed">{item.summary}</span>
            </label>
          ))}
        </div>
      </div>
    );
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden transition-all duration-300 hover:shadow-md">
      {/* Header */}
      <div className="p-5 flex items-center justify-between bg-gray-50/50">
        <div>
          <h3 className="text-lg font-bold text-gray-800">{config.label}</h3>
          {step === "idle" && <p className="text-sm text-gray-500 mt-1">آماده برای بررسی تغییرات</p>}
          {step === "preview" && (
            <p className="text-sm text-blue-600 mt-1 font-medium">
              {totalChanges > 0 ? `${totalChanges} تغییر یافت شد.` : "هیچ تغییری یافت نشد."}
            </p>
          )}
        </div>

        <div className="flex gap-2">
          {step === "idle" || step === "error" ? (
            <button
              onClick={handlePreview}
              className="flex items-center gap-2 px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition active:scale-95"
            >
              دریافت پیش‌نمایش
            </button>
          ) : null}

          {step === "loading_preview" && (
            <button disabled className="flex items-center gap-2 px-5 py-2.5 bg-blue-400 text-white rounded-lg font-medium">
              <Spinner /> در حال بررسی...
            </button>
          )}

          {step === "preview" && totalChanges > 0 && (
            <button
              onClick={handleApply}
              disabled={selectedIds.size === 0}
              className={`flex items-center gap-2 px-5 py-2.5 rounded-lg font-medium transition active:scale-95 ${
                selectedIds.size > 0
                  ? "bg-emerald-600 hover:bg-emerald-700 text-white"
                  : "bg-gray-300 text-gray-500 cursor-not-allowed"
              }`}
            >
              اعمال ({selectedIds.size}) مورد انتخاب شده
            </button>
          )}

          {step === "applying" && (
            <button disabled className="flex items-center gap-2 px-5 py-2.5 bg-emerald-400 text-white rounded-lg font-medium">
              <Spinner /> در حال اعمال...
            </button>
          )}

          {(step === "result" || step === "preview" || step === "error") && (
            <button
              onClick={reset}
              className="px-4 py-2.5 bg-gray-200 hover:bg-gray-300 text-gray-700 rounded-lg font-medium transition"
            >
              بازنشانی
            </button>
          )}
        </div>
      </div>

      {/* Body: Preview Content */}
      {step === "preview" && bundle && totalChanges > 0 && (
        <div className="p-5 border-t border-gray-100 bg-white">
          <div className="flex items-center justify-between mb-4 border-b pb-3">
            <span className="text-sm text-gray-600 font-medium">
              لطفاً مواردی که مایل به اعمال آن‌ها هستید را انتخاب کنید:
            </span>
            <div className="flex gap-4 text-sm font-semibold">
              <button
                onClick={() => {
                  const all = [...bundle.addCommands, ...bundle.updateCommands, ...bundle.deleteCommands].map(i => i.id);
                  setSelectedIds(new Set(all));
                }}
                className="text-blue-600 hover:text-blue-800"
              >
                انتخاب همه
              </button>
              <button
                onClick={() => setSelectedIds(new Set())}
                className="text-gray-500 hover:text-gray-700"
              >
                لغو انتخاب همه
              </button>
            </div>
          </div>

          {renderChangesList("✨ موارد جدید (ایجاد)", bundle.addCommands, "add")}
          {renderChangesList("📝 موارد نیازمند ویرایش", bundle.updateCommands, "update")}
          {renderChangesList("🗑 موارد نیازمند حذف", bundle.deleteCommands, "delete")}
          
          {bundle.warnings?.length > 0 && (
             <div className="mt-4 p-3 bg-amber-50 text-amber-800 border border-amber-200 rounded-lg text-sm">
                <span className="font-bold">⚠️ هشدارها:</span>
                <ul className="list-disc pr-5 mt-1">
                  {bundle.warnings.map((w, i) => <li key={i}>{w}</li>)}
                </ul>
             </div>
          )}
        </div>
      )}

      {/* Body: Empty State */}
      {step === "preview" && totalChanges === 0 && (
        <div className="p-8 text-center text-gray-500 bg-gray-50 border-t border-gray-100">
          <span className="text-4xl block mb-2">🎉</span>
          اطلاعات این بخش کاملاً بروز است و نیازی به همگام‌سازی ندارد.
        </div>
      )}

      {/* Body: Error State */}
      {errorMessage && (
        <div className="p-5 border-t border-red-100 bg-red-50">
          <div className="text-red-700 text-sm font-medium flex gap-2">
             <span>❌</span> {errorMessage}
          </div>
        </div>
      )}

      {/* Body: Result State */}
      {step === "result" && applyResult && (
        <div className="p-5 border-t border-gray-100 bg-slate-50">
           <h4 className="font-bold text-slate-800 mb-4 text-lg">📊 نتیجه همگام‌سازی</h4>
           
           {applyResult.data && (
             <div className="flex gap-6 text-sm mb-4 bg-white p-4 rounded-lg border shadow-sm">
               <span className="text-emerald-600 font-bold">➕ افزوده: {applyResult.data.addedCount}</span>
               <span className="text-blue-600 font-bold">✏️ بروزرسانی: {applyResult.data.updatedCount}</span>
               <span className="text-rose-600 font-bold">➖ حذف: {applyResult.data.deletedCount}</span>
             </div>
           )}

          {applyResult.successMessages?.length > 0 && (
             <div className="mb-3 text-emerald-700 text-sm bg-emerald-50/50 p-3 rounded-lg border border-emerald-100">
               <span className="font-bold block mb-1">✅ عملیات‌های موفق:</span>
               <ul className="list-disc list-inside space-y-1">
                 {applyResult.successMessages.map((msg, idx) => <li key={idx}>{msg}</li>)}
               </ul>
             </div>
           )}

           {applyResult.errors?.length > 0 && (
             <div className="text-rose-700 text-sm bg-rose-50/50 p-3 rounded-lg border border-rose-100">
               <span className="font-bold block mb-1">❌ خطاها در حین اعمال:</span>
               <ul className="list-disc list-inside space-y-1">
                 {applyResult.errors.map((err, idx) => <li key={idx}>{err}</li>)}
               </ul>
             </div>
           )}
        </div>
      )}
    </div>
  );
};

// ---------------------------------------------------------
// 4. Main Page Component
// ---------------------------------------------------------

export const SyncPage: React.FC = () => {
  return (
    <div className="max-w-5xl mx-auto p-4 sm:p-6 lg:p-8">
      {/* Header Section */}
      <div className="mb-8">
        <h1 className="text-3xl font-extrabold text-gray-900 tracking-tight">
          همگام‌سازی با سیستم‌های اطلاعاتی (ایریسا)
        </h1>
        <p className="text-gray-500 mt-2 text-sm leading-relaxed max-w-3xl">
          در این بخش می‌توانید اطلاعات پایه‌ی منابع انسانی را با سیستم مرجع تطبیق دهید. 
          ابتدا پیش‌نمایش تغییرات را دریافت کنید، موارد دلخواه را تایید کرده و سپس آن‌ها را در پایگاه داده اعمال نمایید.
        </p>
      </div>

      {/* Entities List */}
      <div className="space-y-6">
        {entityConfigs.map((config) => (
          <EntitySyncCard key={config.key} config={config} />
        ))}
      </div>
    </div>
  );
};