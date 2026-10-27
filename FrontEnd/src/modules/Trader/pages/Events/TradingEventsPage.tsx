import React, { useEffect, useMemo, useState } from "react";
import { tradingEventApi } from "../../api/tradingEventApi";
import { tradingRunApi } from "../../api/tradingRunApi";
import type {
  TradingRunInfoView,
  TradingEventView,
  TradingEventQuery,
} from "../../models";
import {
  STAGE_LABELS,
  SEVERITY_LABELS,
  SEVERITY_COLORS,
} from "../../models";

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
      second: "2-digit",
    });
  } catch {
    return s;
  }
}

function prettyJson(json: string | null): string {
  if (!json) return "";
  try {
    return JSON.stringify(JSON.parse(json), null, 2);
  } catch {
    return json;
  }
}

/* ═══════════════════ Detail Drawer ═══════════════════ */

interface DrawerProps {
  event: TradingEventView | null;
  onClose: () => void;
}

const DetailDrawer: React.FC<DrawerProps> = ({ event, onClose }) => {
  if (!event) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex justify-end bg-black/50"
      onClick={onClose}
    >
      <div
        className="w-full max-w-2xl overflow-y-auto bg-white shadow-xl dark:bg-gray-800"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="sticky top-0 flex items-center justify-between border-b border-gray-200 bg-white px-5 py-3 dark:border-gray-700 dark:bg-gray-800">
          <h3 className="text-lg font-bold text-gray-900 dark:text-white">
            جزئیات رویداد
          </h3>
          <button
            onClick={onClose}
            className="text-2xl leading-none text-gray-500 hover:text-gray-700 dark:text-gray-400"
          >
            ×
          </button>
        </div>

        {/* Body */}
        <div className="space-y-4 p-5">
          {/* Meta */}
          <div className="grid grid-cols-2 gap-3 text-sm">
            <div>
              <p className="text-xs text-gray-500">زمان</p>
              <p className="font-mono text-xs">{formatTehran(event.occurredAt)}</p>
            </div>
            <div>
              <p className="text-xs text-gray-500">مرحله</p>
              <p>{STAGE_LABELS[event.stage] ?? event.stageName}</p>
            </div>
            <div>
              <p className="text-xs text-gray-500">سطح</p>
              <span
                className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                  SEVERITY_COLORS[event.severity] ?? ""
                }`}
              >
                {SEVERITY_LABELS[event.severity] ?? event.severityName}
              </span>
            </div>
            <div>
              <p className="text-xs text-gray-500">نوع</p>
              <p className="font-mono text-xs">{event.typeName}</p>
            </div>
          </div>

          {/* Message */}
          <div className="rounded-lg bg-gray-50 p-3 dark:bg-gray-900/40">
            <p className="text-xs text-gray-500">پیام</p>
            <p className="mt-1 text-sm text-gray-900 dark:text-gray-100">
              {event.message}
            </p>
          </div>

          {/* Correlation */}
          <div className="space-y-1 text-xs">
            <p className="font-semibold text-gray-700 dark:text-gray-300">
              شناسه‌های مرتبط
            </p>
            <div className="grid grid-cols-2 gap-x-3 gap-y-1 font-mono">
              {event.runId && (
                <Row label="Run" value={event.runId} />
              )}
              {event.isin && <Row label="ISIN" value={event.isin} />}
              {event.accountId && (
                <Row label="Account" value={event.accountId} />
              )}
              {event.signalId && (
                <Row label="Signal" value={event.signalId} />
              )}
              {event.orderIntentId && (
                <Row label="OrderIntent" value={event.orderIntentId} />
              )}
              {event.brokerOrderId && (
                <Row label="BrokerOrder" value={event.brokerOrderId} />
              )}
              {event.tradeId && <Row label="Trade" value={event.tradeId} />}
              {event.positionId && (
                <Row label="Position" value={event.positionId} />
              )}
            </div>
          </div>

          {/* Payload */}
          {event.payloadJson && (
            <div>
              <p className="mb-1 text-xs font-semibold text-gray-700 dark:text-gray-300">
                Payload (JSON)
              </p>
              <pre
                dir="ltr"
                className="max-h-96 overflow-auto rounded-lg bg-gray-900 p-3 text-xs text-gray-100"
              >
                {prettyJson(event.payloadJson)}
              </pre>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

const Row: React.FC<{ label: string; value: string }> = ({ label, value }) => (
  <>
    <span className="text-gray-500">{label}:</span>
    <span className="truncate text-gray-700 dark:text-gray-300" title={value}>
      {value}
    </span>
  </>
);

/* ═══════════════════ Main Page ═══════════════════ */

export const TradingEventsPage: React.FC = () => {
  const [runs, setRuns] = useState<TradingRunInfoView[]>([]);

  // Filters
  const [selectedRunId, setSelectedRunId] = useState<string>("");
  const [selectedStage, setSelectedStage] = useState<string>("");
  const [selectedMinSeverity, setSelectedMinSeverity] = useState<string>("");
  const [search, setSearch] = useState<string>("");
  const [from, setFrom] = useState<string>("");
  const [to, setTo] = useState<string>("");

  // Pagination
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);

  // Data
  const [events, setEvents] = useState<TradingEventView[]>([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Detail
  const [selectedEvent, setSelectedEvent] = useState<TradingEventView | null>(
    null,
  );

  // Load runs
  useEffect(() => {
    (async () => {
      try {
        const list = await tradingRunApi.GetList();
        setRuns(list);
      } catch {
        setRuns([]);
      }
    })();
  }, []);

  // Build query
  const query: TradingEventQuery = useMemo(
    () => ({
      runId: selectedRunId || null,
      stage: selectedStage !== "" ? Number(selectedStage) : null,
      minSeverity:
        selectedMinSeverity !== "" ? Number(selectedMinSeverity) : null,
      search: search.trim() || null,
      from: from ? new Date(from).toISOString() : null,
      to: to ? new Date(to).toISOString() : null,
      skip: (page - 1) * pageSize,
      take: pageSize,
    }),
    [selectedRunId, selectedStage, selectedMinSeverity, search, from, to, page, pageSize],
  );

  // Fetch
  useEffect(() => {
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const result = await tradingEventApi.Query(query);
        setEvents(result.items);
        setTotalRecords(result.totalRecords);
      } catch (e) {
        setError(e instanceof Error ? e.message : "خطا در دریافت رویدادها");
        setEvents([]);
        setTotalRecords(0);
      } finally {
        setLoading(false);
      }
    })();
  }, [query]);

  // Reset page on filter change
  useEffect(() => {
    setPage(1);
  }, [selectedRunId, selectedStage, selectedMinSeverity, search, from, to]);

  const totalPages = Math.max(1, Math.ceil(totalRecords / pageSize));

  return (
    <div className="space-y-4 p-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-900 dark:text-white">
            لاگ رویدادهای Trading
          </h1>
          <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
            {totalRecords.toLocaleString("fa-IR")} رویداد
          </p>
        </div>
        <button
          type="button"
          onClick={() => {
            // Trigger refetch by resetting query
            setSearch((s) => s);
          }}
          disabled={loading}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 disabled:opacity-50 dark:border-gray-600 dark:text-gray-300"
        >
          🔄 بروزرسانی
        </button>
      </div>

      {/* Filters */}
      <div className="grid grid-cols-1 gap-2 md:grid-cols-3 lg:grid-cols-6">
        <select
          value={selectedRunId}
          onChange={(e) => setSelectedRunId(e.target.value)}
          className="rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        >
          <option value="">همه‌ی پلن‌ها</option>
          {runs.map((r) => (
            <option key={r.id} value={r.id}>
              {r.name}
            </option>
          ))}
        </select>

        <select
          value={selectedStage}
          onChange={(e) => setSelectedStage(e.target.value)}
          className="rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        >
          <option value="">همه‌ی مراحل</option>
          {Object.entries(STAGE_LABELS).map(([k, v]) => (
            <option key={k} value={k}>
              {v}
            </option>
          ))}
        </select>

        <select
          value={selectedMinSeverity}
          onChange={(e) => setSelectedMinSeverity(e.target.value)}
          className="rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        >
          <option value="">همه‌ی سطوح</option>
          <option value="0">اشکال‌زدایی و بالاتر</option>
          <option value="1">اطلاع و بالاتر</option>
          <option value="2">هشدار و بالاتر</option>
          <option value="3">خطا و بالاتر</option>
          <option value="4">فقط بحرانی</option>
        </select>

        <input
          type="datetime-local"
          value={from}
          onChange={(e) => setFrom(e.target.value)}
          className="rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
          placeholder="از تاریخ"
        />

        <input
          type="datetime-local"
          value={to}
          onChange={(e) => setTo(e.target.value)}
          className="rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
          placeholder="تا تاریخ"
        />

        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="جستجو در پیام..."
          className="rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
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
              <th className="p-3">زمان</th>
              <th className="p-3">مرحله</th>
              <th className="p-3">سطح</th>
              <th className="p-3">پیام</th>
              <th className="p-3">ISIN</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {loading ? (
              <tr>
                <td colSpan={5} className="p-6 text-center">
                  در حال بارگذاری...
                </td>
              </tr>
            ) : events.length === 0 ? (
              <tr>
                <td colSpan={5} className="p-6 text-center text-gray-500">
                  رویدادی یافت نشد.
                </td>
              </tr>
            ) : (
              events.map((e) => (
                <tr
                  key={e.id}
                  onClick={() => setSelectedEvent(e)}
                  className="cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800/50"
                >
                  <td className="whitespace-nowrap p-3 font-mono text-xs">
                    {formatTehran(e.occurredAt)}
                  </td>
                  <td className="p-3 text-xs">
                    {STAGE_LABELS[e.stage] ?? e.stageName}
                  </td>
                  <td className="p-3">
                    <span
                      className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                        SEVERITY_COLORS[e.severity] ?? ""
                      }`}
                    >
                      {SEVERITY_LABELS[e.severity] ?? e.severityName}
                    </span>
                  </td>
                  <td className="p-3 text-xs">{e.message}</td>
                  <td className="p-3 font-mono text-xs">{e.isin ?? "—"}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      <div className="flex items-center justify-between text-sm">
        <span className="text-xs text-gray-500">
          صفحه {page} از {totalPages}
        </span>
        <div className="flex gap-2">
          <button
            type="button"
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page <= 1 || loading}
            className="rounded border border-gray-300 px-3 py-1 text-xs hover:bg-gray-100 disabled:opacity-50 dark:border-gray-600"
          >
            قبلی
          </button>
          <button
            type="button"
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page >= totalPages || loading}
            className="rounded border border-gray-300 px-3 py-1 text-xs hover:bg-gray-100 disabled:opacity-50 dark:border-gray-600"
          >
            بعدی
          </button>
        </div>
      </div>

      {/* Detail Drawer */}
      <DetailDrawer
        event={selectedEvent}
        onClose={() => setSelectedEvent(null)}
      />
    </div>
  );
};