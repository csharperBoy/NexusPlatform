import React from "react";
import { useDashboardSummary } from "../hooks/useDashboard";
import type { RunsSummary } from "../models";
import { Stat, WidgetShell } from "./_shared/WidgetShell";

const TraderRunsWidget: React.FC = () => {
  const { data, isLoading, error } = useDashboardSummary();
  const runs = data?.runs;

  return (
    <WidgetShell
      title="Trading Runs"
      subtitle={runs ? `${runs.total} پلن` : undefined}
      linkTo="/trader/trading-runs"
      tone={runs && runs.faulted > 0 ? "danger" : "neutral"}
      isLoading={isLoading}
      error={error?.message ?? null}
    >
      {runs && <RunsBody data={runs} />}
    </WidgetShell>
  );
};

const RunsBody: React.FC<{ data: RunsSummary }> = ({ data }) => (
  <div className="grid grid-cols-3 gap-2">
    <Stat label="در حال اجرا" value={data.running} tone={data.running > 0 ? "success" : "neutral"} />
    <Stat label="آماده" value={data.ready} />
    <Stat label="متوقف" value={data.stopped} />
    <Stat label="خطا" value={data.faulted} tone={data.faulted > 0 ? "danger" : "neutral"} />
    <Stat label="ایجاد شده" value={data.created} />
  </div>
);

export default TraderRunsWidget;