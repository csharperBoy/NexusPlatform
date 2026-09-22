import { useEffect, useState } from "react";
import { useSchedulePlansPage } from "../../hooks/useSchedulePlansPage";
import { accountApi } from "../../api";
import { symbolApi } from "../../api";
import { SchedulePlanCard } from "./components/SchedulePlanCard";
import { SchedulePlanForm } from "./components/SchedulePlanForm";
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

  const [accounts, setAccounts] = useState<{ id: string; name: string }[]>([]);
  const [symbols, setSymbols] = useState<
    { symbolIsin: string; symbolName: string }[]
  >([]);
  const [showAddForm, setShowAddForm] = useState(false);

  /* ─── لود حساب‌ها و نمادها برای dropdown ─── */
  useEffect(() => {
    void accountApi
      .GetSelectionList()
      .then((list) =>
        setAccounts(list.map((x) => ({ id: x.value, name: x.label }))),
      )
      .catch(() => setAccounts([]));

    void symbolApi
      .GetList()
      .then((list) =>
        setSymbols(
          list.map((x) => ({
            symbolIsin: x.symbolIsin,
            symbolName: x.symbolName,
          })),
        ),
      )
      .catch(() => setSymbols([]));
  }, []);

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
          disabled={saving}
        >
          + پلن جدید
        </button>
      </div>

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
          accounts={accounts}
          symbols={symbols}
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