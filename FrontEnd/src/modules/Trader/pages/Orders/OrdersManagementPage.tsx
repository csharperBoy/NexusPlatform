import React, { useState } from "react";
import { BrokerOrdersTab } from "./components/BrokerOrdersTab";
import { OrderIntentsTab } from "./components/OrderIntentsTab";

type TabKey = "broker-orders" | "order-intents";

const TABS: { key: TabKey; label: string; hint: string }[] = [
  {
    key: "broker-orders",
    label: "سفارش‌های Broker",
    hint: "سفارش‌های ارسال‌شده به کارگزاری + معاملات",
  },
  {
    key: "order-intents",
    label: "تصمیم‌های اجرا",
    hint: "شامل OrderIntentهای RiskBlocked و Failed",
  },
];

export const OrdersManagementPage: React.FC = () => {
  const [tab, setTab] = useState<TabKey>("broker-orders");

  return (
    <div className="p-6">
      <div className="mb-4">
        <h1 className="text-xl font-bold text-gray-900 dark:text-white">
          مدیریت سفارش‌ها
        </h1>
        <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
          مشاهده‌ی سفارش‌های ارسال‌شده به Broker و تصمیم‌های اجرا (شامل موارد مسدودشده توسط Risk)
        </p>
      </div>

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

      <div>
        {tab === "broker-orders" && <BrokerOrdersTab />}
        {tab === "order-intents" && <OrderIntentsTab />}
      </div>
    </div>
  );
};