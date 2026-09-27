import { useState } from "react";
import { useSchedulePlansPage } from "../../hooks/useSchedulePlansPage";
import { accountApi } from "../../api/accountApi";
import { symbolApi } from "../../api/symbolApi";
import { SchedulePlanCard } from "./components/SchedulePlanCard";
import { SchedulePlanForm } from "./components/SchedulePlanForm";
import { useSelectionList } from "@/core/hooks/useSelectionList";
import type {
  CreateSchedulePlanCommand,
  UpdateSchedulePlanCommand,
} from "../../models";

export function SchedulePlansManagementPage() {
  const {
    plans,
    loading,
    saving,
    error,
    successMessage,
    createPlan,
    updatePlan,
    deletePlan,
    enablePlan,
    disablePlan,
  } = useSchedulePlansPage();

  /* ─── لیست‌های انتخابی از GetSelectionList ─── */
  const accounts = useSelectionList(() => accountApi.GetSelectionList());
  const symbols = useSelectionList(() => symbolApi.GetSelectionList());

  const [showAddForm, setShowAddForm] = useState(false);

  const handleCreate = async (cmd: CreateSchedulePlanCommand) => {
    const ok = await createPlan(cmd);
    if (ok) setShowAddForm(false);
  };

  const handleSave = (cmd: UpdateSchedulePlanCommand) => {
    void updatePlan(cmd);
  };

  const sortedPlans = [...plans].sort((a, b) =>
    a.date.localeCompare(b.date),
  );

  /* ─── چک آماده بودن لیست‌های انتخابی ─── */
  const selectionsReady =
    !accounts.loading &&
    !symbols.loading &&
    accounts.items.length > 0 &&
    symbols.items.length > 0;

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-slate-800 bg-slate-900 p-4">
        <h1 className="text-lg font-bold text-slate-200">
          برنامه‌ریزی سفارشات
        </h1>
        <button
          type="button"
          className="rounded-lg bg-emerald-600 px-4 py-2 text-xs font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
          onClick={() => setShowAddForm(true)}
          disabled={saving || !selectionsReady}
          title={
            !selectionsReady
              ? "اول باید حساب و نماد تعریف شده باشه"
              : undefined
          }
        >
          + پلن جدید
        </button>
      </div>

      {/* اگه لیست‌های پایه خالی هستن */}
      {!accounts.loading && accounts.items.length === 0 && (
        <div className="rounded-lg bg-amber-950/30 p-3 text-xs text-amber-300">
          ⚠️ هیچ حساب کاربری تعریف نشده. اول از «مدیریت حساب‌ها» اضافه کن.
        </div>
      )}
      {!symbols.loading && symbols.items.length === 0 && (
        <div className="rounded-lg bg-amber-950/30 p-3 text-xs text-amber-300">
          ⚠️ هیچ نمادی تعریف نشده. اول از «مدیریت نمادها» اضافه کن.
        </div>
      )}

      {/* Feedback */}
      {error && (
        <div className="rounded-lg bg-red-950/30 p-3 text-xs text-red-400">
          {error}
        </div>
      )}
      {successMessage && (
        <div className="rounded-lg bg-emerald-950/30 p-3 text-xs text-emerald-400">
          {successMessage}
        </div>
      )}

      {/* Add form modal */}
      {showAddForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="w-full max-w-lg rounded-xl bg-slate-900 p-6 shadow-xl">
            <h3 className="mb-4 text-base font-bold text-slate-200">
              ایجاد پلن جدید
            </h3>
            <SchedulePlanForm
              onCancel={() => setShowAddForm(false)}
              onSubmit={handleCreate}
              saving={saving}
            />
          </div>
        </div>
      )}

      {/* Loading */}
      {loading && (
        <div className="rounded-xl border border-slate-800 bg-slate-900 p-6 text-center text-xs text-slate-500">
          در حال بارگذاری...
        </div>
      )}

      {/* Empty */}
      {!loading && sortedPlans.length === 0 && (
        <div className="rounded-xl border border-slate-800 bg-slate-900 p-6 text-center text-xs text-slate-500">
          هیچ پلنی ثبت نشده. روی «+ پلن جدید» بزن.
        </div>
      )}

      {/* Plans */}
      {sortedPlans.map((plan) => (
        <SchedulePlanCard
          key={plan.id}
          plan={plan}
          accounts={accounts.items}
          symbols={symbols.items}
          saving={saving}
          onSave={handleSave}
          onDelete={(id) => {
            if (confirm("پلن حذف بشه؟")) void deletePlan(id);
          }}
          onEnable={(id) => void enablePlan(id)}
          onDisable={(id) => void disablePlan(id)}
        />
      ))}
    </div>
  );
}