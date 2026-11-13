import React from "react";
import { useDashboardSummary } from "../hooks/useDashboard";
import type { CriticalEventView } from "../models";
import { WidgetShell } from "./_shared/WidgetShell";

const TraderEventsWidget: React.FC = () => {
  const { data, isLoading, error } = useDashboardSummary();
  const events = data?.recentCriticalEvents;

  return (
    <WidgetShell
      title="رویدادهای مهم اخیر"
      subtitle={events ? `${events.length} رویداد (Warning/Critical)` : undefined}
      linkTo="/trader/events"
      tone={events && events.some((e) => e.severity >= 4) ? "danger" : "neutral"}
      isLoading={isLoading}
      error={error?.message ?? null}
    >
      {events && <EventsBody events={events} />}
    </WidgetShell>
  );
};

const EventsBody: React.FC<{ events: CriticalEventView[] }> = ({ events }) => {
  if (events.length === 0) {
    return <p className="py-3 text-center text-xs text-gray-500">هیچ رویداد مهمی نیست ✅</p>;
  }

  return (
    <div className="space-y-1">
      {events.slice(0, 6).map((e) => {
        const color =
          e.severity >= 4
            ? "border-red-500 bg-red-50 dark:bg-red-900/20"
            : e.severity >= 3
              ? "border-red-400 bg-red-50/50 dark:bg-red-900/10"
              : "border-amber-400 bg-amber-50 dark:bg-amber-900/20";

        return (
          <div key={e.id} className={`rounded border-r-2 px-2 py-1 text-[11px] ${color}`}>
            <div className="flex items-start justify-between gap-2">
              <div className="min-w-0 flex-1">
                <div className="truncate font-medium">{e.message}</div>
                <div className="mt-0.5 flex items-center gap-2 text-[10px] text-gray-500">
                  <span>{e.typeName}</span>
                  {e.isin && <span className="font-mono">{e.isin}</span>}
                </div>
              </div>
              <span className="whitespace-nowrap text-[10px] text-gray-500">
                {new Date(e.occurredAt).toLocaleTimeString("fa-IR", {
                  hour: "2-digit",
                  minute: "2-digit",
                })}
              </span>
            </div>
          </div>
        );
      })}
    </div>
  );
};

export default TraderEventsWidget;