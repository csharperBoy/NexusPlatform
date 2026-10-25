import React, { useMemo, useState } from "react";
import { useCurrencyRateManagement } from "../../hooks/useCurrencyRateManagement";
import type { CreateCurrencyRateCommand } from "../../models";

/* ─── Helpers ─── */
function parseUtcDate(s: string): Date {
  if (!s) return new Date();
  const hasTz = /[Zz]|[+-]\d{2}:?\d{2}$/.test(s);
  return new Date(hasTz ? s : s + "Z");
}

/**
 * تبدیل Date به مقدار قابل قبول برای <input type="datetime-local">
 * در timezone محلی مرورگر (نه UTC).
 */
function toLocalDateTimeInputValue(d: Date): string {
  const pad = (n: number) => String(n).padStart(2, "0");
  return (
    `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}` +
    `T${pad(d.getHours())}:${pad(d.getMinutes())}`
  );
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
const STANDARD_CURRENCIES = [
  { value: "USD", label: "دلار آمریکا (USD)" },
  { value: "EUR", label: "یورو (EUR)" },
  { value: "GBP", label: "پوند انگلیس (GBP)" },
  { value: "AED", label: "درهم امارات (AED)" },
  { value: "TRY", label: "لیر ترکیه (TRY)" },
  { value: "CNY", label: "یوان چین (CNY)" },
  { value: "JPY", label: "ین ژاپن (JPY)" },
  { value: "CHF", label: "فرانک سوئیس (CHF)" },
  { value: "GOLD", label: "طلا (GOLD)" },
  { value: "IRR", label: "ریال ایران (IRR)" },
];

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
  const [customFrom, setCustomFrom] = useState("");
  const [customTo, setCustomTo] = useState("");
  const [useCustomFrom, setUseCustomFrom] = useState(false);
  const [useCustomTo, setUseCustomTo] = useState(false);
  const [rate, setRate] = useState<string>("");
  const [effectiveAt, setEffectiveAt] = useState<string>(() =>
    toLocalDateTimeInputValue(new Date()),
  );
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const resolvedFrom = useCustomFrom
    ? customFrom.trim().toUpperCase()
    : fromCurrency;
  const resolvedTo = useCustomTo ? customTo.trim().toUpperCase() : toCurrency;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!resolvedFrom || !resolvedTo) {
      setError("هر دو ارز الزامی است");
      return;
    }
    if (resolvedFrom === resolvedTo) {
      setError("ارز مبدأ و مقصد نباید یکسان باشند");
      return;
    }
    const rateNum = Number(rate);
    if (!rateNum || rateNum <= 0) {
      setError("نرخ باید بزرگ‌تر از صفر باشد");
      return;
    }

    try {
      await onSubmit({
        fromCurrency: resolvedFrom,
        toCurrency: resolvedTo,
        rate: rateNum,
        effectiveAt: new Date(effectiveAt).toISOString(),
        source: "Manual",
      });
      setRate("");
      setEffectiveAt(toLocalDateTimeInputValue(new Date()));
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

          {/* ─── From ─── */}
          <div>
            <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
              ارز مبدأ
            </label>
            <div className="flex gap-2">
              <select
                value={useCustomFrom ? "__custom__" : fromCurrency}
                onChange={(e) => {
                  if (e.target.value === "__custom__") {
                    setUseCustomFrom(true);
                  } else {
                    setUseCustomFrom(false);
                    setFromCurrency(e.target.value);
                  }
                }}
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              >
                {STANDARD_CURRENCIES.map((c) => (
                  <option key={c.value} value={c.value}>
                    {c.label}
                  </option>
                ))}
                <option value="__custom__">سایر (دلخواه)...</option>
              </select>
              {useCustomFrom && (
                <input
                  type="text"
                  value={customFrom}
                  onChange={(e) => setCustomFrom(e.target.value.toUpperCase())}
                  placeholder="کد ارز"
                  dir="ltr"
                  maxLength={10}
                  className="w-32 rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                />
              )}
            </div>
          </div>

          {/* ─── To ─── */}
          <div>
            <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
              ارز مقصد
            </label>
            <div className="flex gap-2">
              <select
                value={useCustomTo ? "__custom__" : toCurrency}
                onChange={(e) => {
                  if (e.target.value === "__custom__") {
                    setUseCustomTo(true);
                  } else {
                    setUseCustomTo(false);
                    setToCurrency(e.target.value);
                  }
                }}
                className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              >
                {STANDARD_CURRENCIES.map((c) => (
                  <option key={c.value} value={c.value}>
                    {c.label}
                  </option>
                ))}
                <option value="__custom__">سایر (دلخواه)...</option>
              </select>
              {useCustomTo && (
                <input
                  type="text"
                  value={customTo}
                  onChange={(e) => setCustomTo(e.target.value.toUpperCase())}
                  placeholder="کد ارز"
                  dir="ltr"
                  maxLength={10}
                  className="w-32 rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                />
              )}
            </div>
          </div>

          {/* ─── Rate ─── */}
          <div>
            <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
              نرخ (۱ {resolvedFrom || "?"} = ? {resolvedTo || "?"})
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

          {/* ─── Effective At ─── */}
          <div>
            <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
              زمان اعتبار (وقت محلی)
            </label>
            <input
              type="datetime-local"
              value={effectiveAt}
              onChange={(e) => setEffectiveAt(e.target.value)}
              className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            />
            <p className="mt-1 text-[10px] text-gray-500">
              از این لحظه به بعد، این نرخ (یا نرخ جدیدتر) معتبر است. 
              اگر خالی بماند، الآن ثبت می‌شود.
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