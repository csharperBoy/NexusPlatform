// src/modules/Trader/pages/StrategyTuning/StrategyTuningListPage.tsx
import React, { useState } from "react";
import { Link } from "react-router-dom";
import { useStrategyTuningList } from "../../hooks/useStrategyTuning";
import { TUNING_STATUS_LABELS } from "../../models";
import { StrategyTuningCreateModal } from "./StrategyTuningCreateModal";

export const StrategyTuningListPage: React.FC = () => {
  const { runs, loading, error, refresh } = useStrategyTuningList();
  const [createOpen, setCreateOpen] = useState(false);

  return (
    <div className="space-y-4 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-900 dark:text-white">بهینه‌سازی استراتژی</h1>
          <p className="mt-1 text-xs text-gray-500">{runs.length} اجرا</p>
        </div>
        <div className="flex gap-2">
          <button onClick={refresh} disabled={loading}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm dark:border-gray-600 dark:text-gray-300">
            🔄 بروزرسانی
          </button>
          <button onClick={() => setCreateOpen(true)}
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700">
            ➕ اجرای جدید
          </button>
        </div>
      </div>

      {error && (
        <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
          ⚠️ {error}
        </div>
      )}

      <div className="overflow-x-auto rounded-lg border border-gray-200 dark:border-gray-700">
        <table className="w-full text-right text-sm">
          <thead className="bg-gray-50 text-xs dark:bg-gray-700">
            <tr>
              <th className="p-3">نام</th>
              <th className="p-3">استراتژی</th>
              <th className="p-3">نماد</th>
              <th className="p-3">بازه</th>
              <th className="p-3">وضعیت</th>
              <th className="p-3">پیشرفت</th>
              <th className="p-3">عملیات</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {loading && runs.length === 0 ? (
              <tr><td colSpan={7} className="p-6 text-center">در حال بارگذاری...</td></tr>
            ) : runs.length === 0 ? (
              <tr><td colSpan={7} className="p-6 text-center text-gray-500">هیچ اجرایی نیست.</td></tr>
            ) : runs.map(r => (
              <tr key={r.id} className="hover:bg-gray-50 dark:hover:bg-gray-700/30">
                <td className="p-3 font-medium">{r.name}</td>
                <td className="p-3 font-mono text-xs">{r.strategyKey}</td>
                <td className="p-3 font-mono text-xs">{r.isin}</td>
                <td className="p-3 text-xs">
                  {r.dateFrom.slice(0,10)} → {r.dateTo.slice(0,10)}
                </td>
                <td className="p-3 text-xs">{TUNING_STATUS_LABELS[r.status as keyof typeof TUNING_STATUS_LABELS] ?? String(r.status)}</td>
                <td className="p-3 text-xs">
                  {r.completedEvaluations}/{r.totalEvaluations}
                </td>
                <td className="p-3">
                  <Link to={`/trader/tuning/${r.id}`}
                    className="rounded bg-blue-600 px-2 py-1 text-xs text-white">
                    جزئیات
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <StrategyTuningCreateModal
        isOpen={createOpen}
        onClose={() => setCreateOpen(false)}
        onCreated={() => { setCreateOpen(false); refresh(); }}
      />
    </div>
  );
};