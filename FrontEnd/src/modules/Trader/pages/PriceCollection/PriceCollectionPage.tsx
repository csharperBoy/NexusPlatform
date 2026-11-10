import React, { useState } from "react";
import { PriceSourcesManagementPage } from "../PriceSources/PriceSourcesManagementPage";
import { PriceCollectionJobsPage } from "../PriceCollectionJobs/PriceCollectionJobsPage";

type TabKey = "jobs" | "sources";

const TABS: { key: TabKey; label: string; hint: string }[] = [
  {
    key: "jobs",
    label: "Jobهای جمع‌آوری",
    hint: "اجرای Backfill / Recurring روی منابع",
  },
  {
    key: "sources",
    label: "منابع قیمت",
    hint: "پیکربندی منبع (TGJU، ...) و دارایی‌ها",
  },
];

export const PriceCollectionPage: React.FC = () => {
  const [tab, setTab] = useState<TabKey>("jobs");

  return (
    <div className="p-6">
      {/* Header */}
      <div className="mb-4">
        <h1 className="text-xl font-bold text-gray-900 dark:text-white">
          جمع‌آوری قیمت
        </h1>
        <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
          مدیریت منابع نرخ و Jobهای جمع‌آوری از منابع خارجی
        </p>
      </div>

      {/* Tabs */}
      <div className="mb-4 flex gap-1 border-b border-gray-200 dark:border-gray-700">
        {TABS.map((t) => {
          const active = tab === t.key;
          return (
            <button
              key={t.key}
              type="button"
              onClick={() => setTab(t.key)}
              title={t.hint}
              className={`relative -mb-px border-b-2 px-4 py-2 text-sm font-medium transition-colors ${
                active
                  ? "border-blue-500 text-blue-600 dark:text-blue-400"
                  : "border-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
              }`}
            >
              {t.label}
            </button>
          );
        })}
      </div>

      {/* Content */}
      <div>
        {tab === "jobs" && <PriceCollectionJobsPage />}
        {tab === "sources" && <PriceSourcesManagementPage />}
      </div>
    </div>
  );
};