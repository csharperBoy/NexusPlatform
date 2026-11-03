import React, { useEffect, useMemo, useState } from "react";
import { backtestApi } from "../../../api/backtestApi";
import type { BacktestEventView, BacktestEventQuery } from "../../../models";
import {
  BACKTEST_EVENT_TYPE_LABELS,
  BACKTEST_EVENT_TYPE_COLORS,
  BACKTEST_EVENT_SEVERITY_LABELS,
  BacktestEventType,
} from "../../../models";

interface Props {
  backtestRunId: string;
}

const parseUtcDate = (s: string): Date => {
  const hasTz = /[Zz]|[+-]\d{2}:?\d{2}$/.test(s);
  return new Date(hasTz ? s : s + "Z");
};

const fmtDateTime = (s: string): string =>
  parseUtcDate(s).toLocaleString("fa-IR", {
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  });

const prettyJson = (s: string | null): string => {
  if (!s) return "";
  try {
    return JSON.stringify(JSON.parse(s), null, 2);
  } catch {
    return s;
  }
};

export const BacktestLogsTab: React.FC<Props> = ({ backtestRunId }) => {
  const [events, setEvents] = useState<BacktestEventView[]>([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [typeFilter, setTypeFilter] = useState<string>("");
  const [severityFilter, setSeverityFilter] = useState<string>("");
  const [search, setSearch] = useState<string>("");

  const [selectedEvent, setSelectedEvent] = useState<BacktestEventView | null>(
    null,
  );

  const query: BacktestEventQuery = useMemo(
    () => ({
      backtestRunId,
      type: typeFilter !== "" ? Number(typeFilter) : null,
      minSeverity: severityFilter !== "" ? Number(severityFilter) : null,
      search: search.trim() || null,
      skip: 0,
      take: 5000,
    }),
    [backtestRunId, typeFilter, severityFilter, search],
  );

  useEffect(() => {
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const result = await backtestApi.QueryEvents(query);
        setEvents(result.items);
        setTotalRecords(result.totalRecords);
      } catch (e) {
        setError(e instanceof Error ? e.message : "خطا در دریافت رویدادها");
        setEvents([]);
      } finally {
        setLoading(false);
      }
    })();
  }, [query]);

  const typeOptions = useMemo(() => {
    const present = new Set(events.map((e) => e.type));
    return Array.from(present).sort((a, b) => a - b);
  }, [events]);

  return (
    <div className="space-y-3">
      {/* Filters */}
      <div className="grid grid-cols-1 gap-2 md:grid-cols-3">
        <select
          value={typeFilter}
          onChange={(e) => setTypeFilter(e.target.value)}
          className="rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        >
          <option value="">همه‌ی نوع‌ها</option>
          {typeOptions.map((t) => (
            <option key={t} value={t}>
              {BACKTEST_EVENT_TYPE_LABELS[t] ?? t}
            </option>
          ))}
        </select>

        <select
          value={severityFilter}
          onChange={(e) => setSeverityFilter(e.target.value)}
          className="rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        >
          <option value="">همه‌ی سطوح</option>
          <option value="0">Debug و بالاتر</option>
          <option value="1">Info و بالاتر</option>
          <option value="2">Warning و بالاتر</option>
          <option value="3">فقط Error و Critical</option>
        </select>

        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="جستجو در پیام..."
          className="rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        />
      </div>

      <p className="text-xs text-gray-500 dark:text-gray-400">
        {totalRecords.toLocaleString("fa-IR")} رویداد
      </p>

      {error && (
        <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
          ⚠️ {error}
        </div>
      )}

      {/* Table */}
      <div className="max-h-[600px] overflow-y-auto overflow-x-auto rounded-lg border border-gray-200 dark:border-gray-700">
        <table className="w-full text-right text-xs text-gray-700 dark:text-gray-300">
          <thead className="sticky top-0 z-10 bg-gray-100 text-xs text-gray-700 dark:bg-gray-700 dark:text-gray-300">
            <tr>
              <th className="p-2">زمان</th>
              <th className="p-2">نوع</th>
              <th className="p-2">سطح</th>
              <th className="p-2">پیام</th>
              <th className="p-2">ISIN</th>
              <th className="p-2">کندل</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {loading ? (
              <tr>
                <td colSpan={6} className="p-6 text-center">
                  در حال بارگذاری...
                </td>
              </tr>
            ) : events.length === 0 ? (
              <tr>
                <td colSpan={6} className="p-6 text-center text-gray-500">
                  رویدادی یافت نشد.
                </td>
              </tr>
            ) : (
              events.map((e) => (
                <tr
                  key={e.id}
                  onClick={() => setSelectedEvent(e)}
                  className="cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700/30"
                >
                  <td className="whitespace-nowrap p-2 font-mono text-[10px]">
                    {fmtDateTime(e.occurredAt)}
                  </td>
                  <td className="p-2">
                    <span
                      className={`inline-flex rounded px-1.5 py-0.5 text-[10px] font-medium whitespace-nowrap ${
                        BACKTEST_EVENT_TYPE_COLORS[e.type] ?? ""
                      }`}
                    >
                      {BACKTEST_EVENT_TYPE_LABELS[e.type] ?? e.typeName}
                    </span>
                  </td>
                  <td className="p-2 text-[10px]">
                    {BACKTEST_EVENT_SEVERITY_LABELS[e.severity] ?? e.severityName}
                  </td>
                  <td className="p-2">{e.message}</td>
                  <td className="p-2 font-mono text-[10px]">{e.isin ?? "—"}</td>
                  <td className="whitespace-nowrap p-2 text-[10px] text-gray-500">
                    {e.candleTime ? fmtDateTime(e.candleTime) : "—"}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Detail Drawer */}
      {selectedEvent && (
        <div
          className="fixed inset-0 z-50 flex justify-end bg-black/50"
          onClick={() => setSelectedEvent(null)}
        >
          <div
            className="w-full max-w-2xl overflow-y-auto bg-white shadow-xl dark:bg-gray-800"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="sticky top-0 flex items-center justify-between border-b border-gray-200 bg-white px-5 py-3 dark:border-gray-700 dark:bg-gray-800">
              <h3 className="text-lg font-bold text-gray-900 dark:text-white">
                جزئیات رویداد
              </h3>
              <button
                onClick={() => setSelectedEvent(null)}
                className="text-2xl text-gray-500 hover:text-gray-700"
              >
                ×
              </button>
            </div>
            <div className="space-y-4 p-5">
              <div className="grid grid-cols-2 gap-3 text-sm">
                <div>
                  <p className="text-xs text-gray-500">زمان</p>
                  <p className="font-mono text-xs">
                    {fmtDateTime(selectedEvent.occurredAt)}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-gray-500">نوع</p>
                  <p>{BACKTEST_EVENT_TYPE_LABELS[selectedEvent.type] ?? selectedEvent.typeName}</p>
                </div>
                <div>
                  <p className="text-xs text-gray-500">سطح</p>
                  <p>{BACKTEST_EVENT_SEVERITY_LABELS[selectedEvent.severity] ?? selectedEvent.severityName}</p>
                </div>
                {selectedEvent.isin && (
                  <div>
                    <p className="text-xs text-gray-500">ISIN</p>
                    <p className="font-mono text-xs">{selectedEvent.isin}</p>
                  </div>
                )}
                {selectedEvent.candleTime && (
                  <div>
                    <p className="text-xs text-gray-500">زمان کندل</p>
                    <p className="font-mono text-xs">
                      {fmtDateTime(selectedEvent.candleTime)}
                    </p>
                  </div>
                )}
              </div>

              <div className="rounded-lg bg-gray-50 p-3 dark:bg-gray-900/40">
                <p className="text-xs text-gray-500">پیام</p>
                <p className="mt-1 text-sm text-gray-900 dark:text-gray-100">
                  {selectedEvent.message}
                </p>
              </div>

              {selectedEvent.payloadJson && (
                <div>
                  <p className="mb-1 text-xs font-semibold text-gray-700 dark:text-gray-300">
                    Payload (JSON)
                  </p>
                  <pre
                    dir="ltr"
                    className="max-h-96 overflow-auto rounded-lg bg-gray-900 p-3 text-xs text-gray-100"
                  >
                    {prettyJson(selectedEvent.payloadJson)}
                  </pre>
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};