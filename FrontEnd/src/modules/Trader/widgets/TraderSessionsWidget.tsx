import React from "react";
import { useDashboardSummary } from "../hooks/useDashboard";
import type { AccountsSessionSummary } from "../models";
import { Stat, WidgetShell } from "./_shared/WidgetShell";

const TraderSessionsWidget: React.FC = () => {
  const { data, isLoading, error } = useDashboardSummary();
  const sessions = data?.accountsSessions;

  const tone =
    sessions && (sessions.expired > 0 || sessions.empty > 0)
      ? "warning"
      : "neutral";

  return (
    <WidgetShell
      title="حساب‌های کارگزاری"
      subtitle={sessions ? `${sessions.total} حساب` : undefined}
      linkTo="/trader/accounts"
      tone={tone}
      isLoading={isLoading}
      error={error?.message ?? null}
    >
      {sessions && <SessionsBody data={sessions} />}
    </WidgetShell>
  );
};

const SessionsBody: React.FC<{ data: AccountsSessionSummary }> = ({ data }) => (
  <div className="grid grid-cols-2 gap-2">
    <Stat label="Session معتبر" value={data.valid} tone="success" />
    <Stat label="منقضی" value={data.expired} tone={data.expired > 0 ? "warning" : "neutral"} />
    <Stat label="خالی" value={data.empty} tone={data.empty > 0 ? "warning" : "neutral"} />
    <Stat label="نامعتبر" value={data.invalid} tone={data.invalid > 0 ? "danger" : "neutral"} />
  </div>
);

export default TraderSessionsWidget;