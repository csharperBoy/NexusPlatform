import React, { useMemo, useState } from "react";
import { useCurrencyRateManagement } from "../../hooks/useCurrencyRateManagement";
import type { CreateCurrencyRateCommand } from "../../models";

/* ─── Helpers ─── */
function parseUtcDate(s: string): Date {
  if (!s) return new Date();
  const hasTz = /[Zz]|[+-]\d{2}:?\d{2}$/.test(s);
  return new Date(hasTz ? s : s + "Z");
}

function formatTehran(s: string): string {
  try {
    return parseUtcDate(s).toLocaleString("fa-IR", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    });
  } catch {
    return s;
  }
}

function formatRate(rate: number): string {
  // For high rates (IRR) → no decimals; for low rates → more decimals
  if (rate >= 1000) return rate.toLocaleString("en-US", { maximumFractionDigits: 0 });
  if (rate >= 1) return rate.toLocaleString("en-US", { maximumFractionDigits: 4 });
  return rate.toLocaleString("en-US", { maximumFractionDigits: 8 });
}

/* ─── Common Currencies ─── */
const COMMON_CURRENCIES = ["USD", "EUR", "GBP", "AED", "TRY", "CNY", "GOLD"];

/* ═══════════════════ Create Modal ═══════════════════ */

interface CreateModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (cmd: CreateCurrencyRateCommand) => Promise<void>;
  saving: boolean;
}

const CreateModal: React.FC<CreateModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  saving,
}) => {
  const [fromCurrency, setFromCurrency] = useState("USD");
  const [toCurrency, setToCurrency] = useState("IRR");
  const [rate, setRate] = useState<string>("");
  const [effectiveAt, setEffectiveAt] = useState<string>(
    new Date().toISOString().slice(0, 16),
  );
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!fromCurrency.trim() || !toCurrency.trim()) {
      setError("هر دو ارز الزامی است");
      return;
    }
    const rateNum = Number(rate);
    if (!rateNum || rateNum <= 0) {
      setError("نرخ باید بزرگ‌تر از صفر باشد");
      return;
    }
    if (fromCurrency.toUpperCase() === toCurrency.toUpperCase()) {
      setError("ارز مبدأ و مقصد نباید یکسان باشند");
      return;
    }

    try {
      await onSubmit({
        fromCurrency: fromCurrency.toUpperCase(),
        toCurrency: toCurrency.toUpperCase(),
        rate: rateNum,
        effectiveAt: new Date(effectiveAt).toISOString(),
        source: "Manual",
      });
      // Reset
      setRate("");
    } catch (e) {
      setError(e instanceof Error ? e.message : "خطا در ثبت");
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl dark:bg-gray-800">
        <h3 className="mb-4 text-lg font-bold text-gray-900 dark:text-white">
          افزودن نرخ ارز
        </h3>

        <form onSubmit={handleSubmit} className="space-y-3">
          {error && (
            <div className="rounded bg-red-50 p-2 text-xs text-red-700 dark:bg-red-900/30 dark:text-red-400">
              ⚠️ {error}
            </div>
          )}

          <div className="grid grid-cols-2 gap-2">
            <div>
              <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                ارز مبدأ
              </label>
              <input
                type="text"
                value={fromCurrency}
                onChange={(e) => setFromCurrency(e.target.value.toUpperCase())}
                placeholder="USD"
                dir="ltr"
                list="currencies"
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            </div>
            <div>
              <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
                ارز مقصد
              </label>
              <input
                type="text"
                value={toCurrency}
                onChange={(e) => setToCurrency(e.target.value.toUpperCase())}
                placeholder="IRR"
                dir="ltr"
                list="currencies"
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            </div>
          </div>

          <datalist id="currencies">
            {COMMON_CURRENCIES.map((c) => (
              <option key={c} value={c} />
            ))}
            <option value="IRR" />
          </datalist>

          <div>
            <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
              نرخ (۱ {fromCurrency} = ? {toCurrency})
            </label>
            <input
              type="number"
              value={rate}
              onChange={(e) => setRate(e.target.value)}
              placeholder="مثلاً: 650000"
              dir="ltr"
              min={0}
              step="any"
              className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            />
          </div>

          <div>
            <label className="text-xs font-medium text-gray-700 dark:text-gray-300">
              زمان اعتبار
            </label>
            <input
              type="datetime-local"
              value={effectiveAt}
              onChange={(e) => setEffectiveAt(e.target.value)}
              className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            />
            <p className="mt-1 text-[10px] text-gray-500">
              از این لحظه به بعد، این نرخ (یا نرخ جدیدتر) معتبر است
            </p>
          </div>

          <div className="flex justify-end gap-2 border-t border-gray-200 pt-3 dark:border-gray-700">
            <button
              type="button"
              onClick={onClose}
              className="rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300"
            >
              انصراف
            </button>
            <button
              type="submit"
              disabled={saving}
              className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {saving ? "در حال ثبت..." : "ثبت نرخ"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

/* ═══════════════════ Main Page ═══════════════════ */

export const CurrencyRatesManagementPage: React.FC = () => {
  const { rates, loading, error, refresh, handleCreate } =
    useCurrencyRateManagement();

  const [fromFilter, setFromFilter] = useState<string>("");
  const [toFilter, setToFilter] = useState<string>("");
  const [search, setSearch] = useState<string>("");
  const [modalOpen, setModalOpen] = useState(false);
  const [saving, setSaving] = useState(false);

  const filtered = useMemo(() => {
    return rates.filter((r) => {
      if (fromFilter && r.fromCurrency !== fromFilter.toUpperCase()) return false;
      if (toFilter && r.toCurrency !== toFilter.toUpperCase()) return false;
      if (search.trim()) {
        const q = search.toLowerCase();
        const match =
          r.fromCurrency.toLowerCase().includes(q) ||
          r.toCurrency.toLowerCase().includes(q) ||
          r.source.toLowerCase().includes(q);
        if (!match) return false;
      }
      return true;
    });
  }, [rates, fromFilter, toFilter, search]);

  const submitCreate = async (cmd: CreateCurrencyRateCommand) => {
    setSaving(true);
    try {
      await handleCreate(cmd);
      setModalOpen(false);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="space-y-4 p-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-900 dark:text-white">
            نرخ‌های ارز
          </h1>
          <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
            {rates.length} نرخ ثبت‌شده · {filtered.length} نمایش
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={() => refresh()}
            disabled={loading}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 disabled:opacity-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
          >
            🔄 بروزرسانی
          </button>
          <button
            type="button"
            onClick={() => setModalOpen(true)}
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
          >
            ➕ نرخ جدید
          </button>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-2">
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="جستجو..."
          className="flex-1 min-w-[200px] rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        />
        <input
          type="text"
          value={fromFilter}
          onChange={(e) => setFromFilter(e.target.value.toUpperCase())}
          placeholder="از ارز (USD)"
          dir="ltr"
          className="w-32 rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        />
        <input
          type="text"
          value={toFilter}
          onChange={(e) => setToFilter(e.target.value.toUpperCase())}
          placeholder="به ارز (IRR)"
          dir="ltr"
          className="w-32 rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        />
      </div>

      {/* Error */}
      {error && (
        <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
          ⚠️ {error}
        </div>
      )}

      {/* Table */}
      <div className="overflow-x-auto rounded-lg border border-gray-200 shadow-sm dark:border-gray-700">
        <table className="w-full text-right text-sm text-gray-700 dark:text-gray-300">
          <thead className="bg-gray-50 text-xs text-gray-700 dark:bg-gray-700 dark:text-gray-300">
            <tr>
              <th className="p-3">از</th>
              <th className="p-3">به</th>
              <th className="p-3">نرخ</th>
              <th className="p-3">زمان اعتبار (تهران)</th>
              <th className="p-3">منبع</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {loading ? (
              <tr>
                <td colSpan={5} className="p-6 text-center">
                  در حال بارگذاری...
                </td>
              </tr>
            ) : filtered.length === 0 ? (
              <tr>
                <td colSpan={5} className="p-6 text-center text-gray-500">
                  هیچ نرخی یافت نشد.
                </td>
              </tr>
            ) : (
              filtered.map((r) => (
                <tr
                  key={r.id}
                  className="hover:bg-gray-50 dark:hover:bg-gray-800/50"
                >
                  <td className="p-3 font-mono text-xs">{r.fromCurrency}</td>
                  <td className="p-3 font-mono text-xs">{r.toCurrency}</td>
                  <td className="p-3 font-mono text-xs" dir="ltr">
                    {formatRate(r.rate)}
                  </td>
                  <td className="p-3 text-xs">
                    {formatTehran(r.effectiveAt)}
                  </td>
                  <td className="p-3 text-xs text-gray-500">{r.source}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <div className="rounded-lg border border-blue-700 bg-blue-900/20 p-3 text-xs text-blue-300">
        ℹ️ نرخ‌ها Historical هستند — هر ثبت جدید، نرخ قبلی را حذف نمی‌کند.
        آخرین نرخ قبل از هر لحظه، در محاسبات استفاده می‌شود.
      </div>

      {/* Create Modal */}
      <CreateModal
        isOpen={modalOpen}
        onClose={() => setModalOpen(false)}
        onSubmit={submitCreate}
        saving={saving}
      />
    </div>
  );
};