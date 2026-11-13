import React from "react";
import { useDashboardSummary } from "../hooks/useDashboard";
import type { OrdersTodaySummary } from "../models";
import { Stat, WidgetShell } from "./_shared/WidgetShell";

const TraderOrdersTodayWidget: React.FC = () => {
  const { data, isLoading, error } = useDashboardSummary();
  const orders = data?.ordersToday;

  return (
    <WidgetShell
      title="سفارش‌های امروز"
      subtitle={
        orders ? `${orders.total} سفارش — ${orders.paper} Paper / ${orders.live} Live` : undefined
      }
      linkTo="/trader/orders"
      tone={orders && orders.rejected > 0 ? "warning" : "neutral"}
      isLoading={isLoading}
      error={error?.message ?? null}
    >
      {orders && <OrdersBody data={orders} />}
    </WidgetShell>
  );
};

const OrdersBody: React.FC<{ data: OrdersTodaySummary }> = ({ data }) => (
  <div className="grid grid-cols-3 gap-2">
    <Stat label="کامل اجرا" value={data.fullyExecuted} tone="success" />
    <Stat label="نیمه اجرا" value={data.partiallyExecuted} tone="warning" />
    <Stat label="در انتظار / باز" value={data.pending + data.open} />
    <Stat label="رد شده" value={data.rejected} tone={data.rejected > 0 ? "danger" : "neutral"} />
    <Stat label="لغو شده" value={data.canceled} />
    <Stat label="منقضی" value={data.expired} />
  </div>
);

export default TraderOrdersTodayWidget;