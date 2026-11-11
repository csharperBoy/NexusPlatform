import React, { useState } from "react";
import { useOrderIntentsManagement } from "../../../hooks/useOrderIntentsManagement";
import { INTENT_STATE_OPTIONS, PAPER_OPTIONS } from "../../../models";
import { PaperBadge, SideBadge, StateBadge } from "./OrderBadges";
import { OrderIntentDrawer } from "./OrderIntentDrawer";

export const OrderIntentsTab: React.FC = () => {
  const {
    intents,
    totalCount,
    loading,
    loadingMore,
    error,
    filters,
    setFilters,
    resetFilters,
    refresh,
    loadMore,
    canLoadMore,
  } = useOrderIntentsManagement();

  const [openId, setOpenId] = useState<string | null>(null);

  const formatDate = (iso: string | null): string => {
    if (!iso) return "—";
    return new Date(iso).toLocaleString("fa-IR");
  };

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap items-center gap-2">
        <input
          type="text"
          value={filters.isin ?? ""}
          onChange={(e) => setFilters({ isin: e.target.value })}
          placeholder="ISIN..."
          className="w-40 rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        />
        <select
          value={filters.state ?? ""}
          onChange={(e) =>
            setFilters({ state: e.target.value === "" ? null : Number(e.target.value) })
          }
          className="rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        >
          <option value="">همه‌ی وضعیت‌ها</option>
          {INTENT_STATE_OPTIONS.map((s) => (
            <option key={s.value} value={s.value}>
              {s.label}
            </option>
          ))}
        </select>
        <select
          value={filters.isPaper == null ? "" : String(filters.isPaper)}
          onChange={(e) =>
            setFilters({ isPaper: e.target.value === "" ? null : e.target.value === "true" })
          }
          className="rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        >
          {PAPER_OPTIONS.map((o) => (
            <option key={o.value} value={o.value}>
              {o.label}
            </option>
          ))}
        </select>
        <input
          type="text"
          value={filters.search ?? ""}
          onChange={(e) => setFilters({ search: e.target.value })}
          placeholder="جستجو (دلیل / خطا)..."
          className="flex-1 min-w-[180px] rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        />
        <button
          type="button"
          onClick={resetFilters}
          className="rounded border border-gray-300 px-3 py-2 text-xs text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
        >
          پاک کردن
        </button>
        <button
          type="button"
          onClick={refresh}
          disabled={loading}
          className="rounded border border-gray-300 px-3 py-2 text-xs text-gray-700 hover:bg-gray-100 disabled:opacity-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
        >
          🔄 بروزرسانی
        </button>
      </div>

      {error && (
        <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
          ⚠️ {error}
        </div>
      )}

      <div className="text-xs text-gray-500 dark:text-gray-400">
        {intents.length} از {totalCount} تصمیم
      </div>

      <div className="overflow-x-auto rounded-lg border border-gray-200 dark:border-gray-700">
        <table className="w-full text-right text-sm text-gray-700 dark:text-gray-300">
          <thead className="bg-gray-50 text-xs dark:bg-gray-700">
            <tr>
              <th className="p-2">نماد</th>
              <th className="p-2">جهت</th>
              <th className="p-2">حساب</th>
              <th className="p-2">حالت</th>
              <th className="p-2">قیمت × تعداد</th>
              <th className="p-2">وضعیت</th>
              <th className="p-2">دلیل</th>
              <th className="p-2">زمان</th>
              <th className="p-2 text-center">عملیات</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {loading && intents.length === 0 ? (
              <tr>
                <td colSpan={9} className="p-6 text-center">
                  در حال بارگذاری...
                </td>
              </tr>
            ) : intents.length === 0 ? (
              <tr>
                <td colSpan={9} className="p-6 text-center">
                  هیچ تصمیمی یافت نشد
                </td>
              </tr>
            ) : (
              intents.map((i) => (
                <tr
                  key={i.id}
                  className="cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800/50"
                  onClick={() => setOpenId(i.id)}
                >
                  <td className="p-2">
                    <div className="font-medium">{i.symbolName ?? i.isin}</div>
                    <div className="font-mono text-[10px] text-gray-500">{i.isin}</div>
                  </td>
                  <td className="p-2">
                    <SideBadge side={i.side} sideName={i.sideName} />
                  </td>
                  <td className="p-2 text-xs">{i.accountName ?? "—"}</td>
                  <td className="p-2">
                    <PaperBadge isPaper={i.isPaper} />
                  </td>
                  <td className="p-2 font-mono text-xs">
                    {i.price.toLocaleString("en-US")} × {i.quantity.toLocaleString("en-US")}
                  </td>
                  <td className="p-2">
                    <StateBadge state={i.state} stateName={i.stateName} variant="intent" />
                  </td>
                  <td className="p-2 max-w-[200px] truncate text-xs" title={i.failureReason ?? i.reason ?? ""}>
                    {i.failureReason ?? i.reason ?? "—"}
                  </td>
                  <td className="p-2 text-xs">{formatDate(i.createdAt)}</td>
                  <td className="p-2 text-center">
                    <button
                      type="button"
                      onClick={(e) => {
                        e.stopPropagation();
                        setOpenId(i.id);
                      }}
                      className="rounded px-2 py-1 text-xs text-blue-600 hover:bg-blue-50 dark:text-blue-400 dark:hover:bg-blue-900/30"
                    >
                      جزئیات
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {canLoadMore && (
        <div className="flex justify-center">
          <button
            type="button"
            onClick={loadMore}
            disabled={loadingMore}
            className="rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 disabled:opacity-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
          >
            {loadingMore ? "در حال بارگذاری..." : `نمایش بیشتر (${totalCount - intents.length} باقی‌مانده)`}
          </button>
        </div>
      )}

      <OrderIntentDrawer
        isOpen={openId !== null}
        intentId={openId}
        onClose={() => setOpenId(null)}
      />
    </div>
  );
};