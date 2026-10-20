import React, { useEffect, useMemo, useState } from "react";
import { positionApi } from "../../api/positionApi";
import { tradingRunApi } from "../../api/tradingRunApi";
import { accountApi } from "../../api/accountApi";
import type {
  PositionView,
  TradingRunInfoView,
} from "../../models";
import {
  PositionState,
  POSITION_STATE_LABELS,
  POSITION_STATE_COLORS,
} from "../../models";
import type { SelectionListDto } from "@/core/models/SelectionListDto";

type FilterMode = "byRun" | "byAccount";

const StateBadge: React.FC<{ state: number }> = ({ state }) => {
  const s = state as PositionState;
  return (
    <span
      className={`inline-flex rounded-full px-2 py-1 text-xs font-medium whitespace-nowrap ${
        POSITION_STATE_COLORS[s] ?? "bg-slate-700 text-slate-300"
      }`}
    >
      {POSITION_STATE_LABELS[s] ?? "نامشخص"}
    </span>
  );
};

const fmtMoney = (v: number | null): string => {
  if (v == null) return "—";
  const formatted = Math.abs(v).toLocaleString("fa-IR");
  if (v < 0) return `(${formatted})`;
  return formatted;
};

const fmtPnLColor = (v: number | null): string => {
  if (v == null || v === 0) return "text-gray-600 dark:text-gray-400";
  return v > 0
    ? "text-emerald-600 dark:text-emerald-400"
    : "text-red-600 dark:text-red-400";
};

export const PositionsPage: React.FC = () => {
  const [mode, setMode] = useState<FilterMode>("byRun");
  const [runs, setRuns] = useState<TradingRunInfoView[]>([]);
  const [accounts, setAccounts] = useState<SelectionListDto[]>([]);

  const [selectedRunId, setSelectedRunId] = useState<string>("");
  const [selectedAccountId, setSelectedAccountId] = useState<string>("");

  const [positions, setPositions] = useState<PositionView[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /* ─── Load reference data ─── */
  useEffect(() => {
    (async () => {
      try {
        const [runList, accountList] = await Promise.all([
          tradingRunApi.GetList(),
          accountApi.GetSelectionList(),
        ]);
        setRuns(runList);
        setAccounts(accountList);
      } catch {
        // Ignore
      }
    })();
  }, []);

  /* ─── Load positions on filter change ─── */
  useEffect(() => {
    const id = mode === "byRun" ? selectedRunId : selectedAccountId;
    if (!id) {
      setPositions([]);
      return;
    }
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const data =
          mode === "byRun"
            ? await positionApi.GetByRun(id)
            : await positionApi.GetByAccount(id);
        setPositions(data);
      } catch (e) {
        setError(
          e instanceof Error ? e.message : "خطا در دریافت موقعیت‌ها",
        );
        setPositions([]);
      } finally {
        setLoading(false);
      }
    })();
  }, [mode, selectedRunId, selectedAccountId]);

  /* ─── Summary ─── */
  const summary = useMemo(() => {
    const open = positions.filter((p) => p.state !== PositionState.Closed);
    const totalRealized = positions.reduce(
      (s, p) => s + p.realizedPnL,
      0,
    );
    const totalUnrealized = positions.reduce(
      (s, p) => s + (p.unrealizedPnL ?? 0),
      0,
    );
    return { count: positions.length, open: open.length, totalRealized, totalUnrealized };
  }, [positions]);

  return (
    <div className="space-y-4 p-6">
      {/* Header */}
      <div>
        <h1 className="text-xl font-bold text-gray-900 dark:text-white">
          موقعیت‌های باز
        </h1>
        <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
          {summary.count} موقعیت · {summary.open} باز
        </p>
      </div>

      {/* Tabs */}
      <div className="flex gap-1 border-b border-gray-200 dark:border-gray-700">
        <button
          type="button"
          onClick={() => setMode("byRun")}
          className={`border-b-2 px-4 py-2 text-sm font-medium transition-colors ${
            mode === "byRun"
              ? "border-blue-500 text-blue-600 dark:text-blue-400"
              : "border-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400"
          }`}
        >
          بر اساس پلن
        </button>
        <button
          type="button"
          onClick={() => setMode("byAccount")}
          className={`border-b-2 px-4 py-2 text-sm font-medium transition-colors ${
            mode === "byAccount"
              ? "border-blue-500 text-blue-600 dark:text-blue-400"
              : "border-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400"
          }`}
        >
          بر اساس حساب
        </button>
      </div>

      {/* Filter Select */}
      <div className="max-w-md">
        {mode === "byRun" ? (
          <select
            value={selectedRunId}
            onChange={(e) => setSelectedRunId(e.target.value)}
            className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
          >
            <option value="">انتخاب پلن...</option>
            {runs.map((r) => (
              <option key={r.id} value={r.id}>
                {r.name}
              </option>
            ))}
          </select>
        ) : (
          <select
            value={selectedAccountId}
            onChange={(e) => setSelectedAccountId(e.target.value)}
            className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
          >
            <option value="">انتخاب حساب...</option>
            {accounts.map((a) => (
              <option key={a.value} value={a.value}>
                {a.display || a.label}
              </option>
            ))}
          </select>
        )}
      </div>

      {/* Summary Cards */}
      {positions.length > 0 && (
        <div className="grid grid-cols-1 gap-3 md:grid-cols-3">
          <div className="rounded-lg border border-gray-200 bg-white p-3 dark:border-gray-700 dark:bg-gray-800">
            <p className="text-xs text-gray-500">سود/زیان محقق‌شده</p>
            <p className={`mt-1 text-lg font-bold ${fmtPnLColor(summary.totalRealized)}`}>
              {fmtMoney(summary.totalRealized)}
            </p>
          </div>
          <div className="rounded-lg border border-gray-200 bg-white p-3 dark:border-gray-700 dark:bg-gray-800">
            <p className="text-xs text-gray-500">سود/زیان جاری</p>
            <p className={`mt-1 text-lg font-bold ${fmtPnLColor(summary.totalUnrealized)}`}>
              {fmtMoney(summary.totalUnrealized)}
            </p>
          </div>
          <div className="rounded-lg border border-gray-200 bg-white p-3 dark:border-gray-700 dark:bg-gray-800">
            <p className="text-xs text-gray-500">مجموع</p>
            <p className={`mt-1 text-lg font-bold ${fmtPnLColor(summary.totalRealized + summary.totalUnrealized)}`}>
              {fmtMoney(summary.totalRealized + summary.totalUnrealized)}
            </p>
          </div>
        </div>
      )}

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
              <th className="p-3">نماد</th>
              <th className="p-3">وضعیت</th>
              <th className="p-3">تعداد</th>
              <th className="p-3">میانگین ورود</th>
              <th className="p-3">SL</th>
              <th className="p-3">TP</th>
              <th className="p-3">سود محقق</th>
              <th className="p-3">سود جاری</th>
              <th className="p-3">تاریخ باز شدن</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {!selectedRunId && !selectedAccountId ? (
              <tr>
                <td colSpan={9} className="p-6 text-center text-sm text-gray-500">
                  برای دیدن موقعیت‌ها، یک {mode === "byRun" ? "پلن" : "حساب"} انتخاب کنید.
                </td>
              </tr>
            ) : loading ? (
              <tr>
                <td colSpan={9} className="p-6 text-center">
                  در حال بارگذاری...
                </td>
              </tr>
            ) : positions.length === 0 ? (
              <tr>
                <td colSpan={9} className="p-6 text-center text-sm text-gray-500">
                  موقعیتی یافت نشد.
                </td>
              </tr>
            ) : (
              positions.map((p) => (
                <tr
                  key={p.id}
                  className="hover:bg-gray-50 dark:hover:bg-gray-800/50"
                >
                  <td className="p-3 font-mono text-xs">{p.isin}</td>
                  <td className="p-3">
                    <StateBadge state={p.state} />
                  </td>
                  <td className="p-3">{p.quantity.toLocaleString("fa-IR")}</td>
                  <td className="p-3">
                    {p.averageEntryPrice.toLocaleString("fa-IR")}
                  </td>
                  <td className="p-3 text-xs">
                    {p.stopLossPrice != null
                      ? p.stopLossPrice.toLocaleString("fa-IR")
                      : "—"}
                  </td>
                  <td className="p-3 text-xs">
                    {p.takeProfitPrice != null
                      ? p.takeProfitPrice.toLocaleString("fa-IR")
                      : "—"}
                  </td>
                  <td className={`p-3 text-xs ${fmtPnLColor(p.realizedPnL)}`}>
                    {fmtMoney(p.realizedPnL)}
                  </td>
                  <td className={`p-3 text-xs ${fmtPnLColor(p.unrealizedPnL)}`}>
                    {fmtMoney(p.unrealizedPnL)}
                  </td>
                  <td className="p-3 text-xs">
                    {new Date(p.openedAt).toLocaleString("fa-IR")}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};