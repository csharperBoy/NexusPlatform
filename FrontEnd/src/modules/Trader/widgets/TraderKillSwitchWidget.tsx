import React from "react";
import { useDashboardSummary } from "../hooks/useDashboard";
import type { KillSwitchSummary } from "../models";
import { WidgetShell } from "./_shared/WidgetShell";

const TraderKillSwitchWidget: React.FC = () => {
  const { data, isLoading, error } = useDashboardSummary();

  const ks = data?.killSwitch;
  const active = !!ks && (ks.globalActive || ks.activeAccounts.length > 0);

  return (
    <WidgetShell
      title="Kill Switch"
      subtitle={
        !ks
          ? undefined
          : ks.globalActive
            ? "🚨 فعال (Global)"
            : ks.activeAccounts.length > 0
              ? `⚠️ ${ks.activeAccounts.length} حساب فعال`
              : "غیرفعال"
      }
      linkTo="/trader/risk"
      linkLabel="مدیریت"
      tone={active ? "danger" : "neutral"}
      isLoading={isLoading}
      error={error?.message ?? null}
    >
      {ks && <KillSwitchBody data={ks} />}
    </WidgetShell>
  );
};

const KillSwitchBody: React.FC<{ data: KillSwitchSummary }> = ({ data }) => (
  <div className="space-y-2">
    <div
      className={`rounded-lg p-2 text-xs ${
        data.globalActive
          ? "bg-red-900/20 text-red-300"
          : "bg-slate-100 text-slate-600 dark:bg-slate-700 dark:text-slate-300"
      }`}
    >
      <div className="flex items-center justify-between">
        <span className="font-medium">Global</span>
        <span className="font-mono">
          {data.globalActive ? "ACTIVE" : "inactive"}
        </span>
      </div>
      {data.globalActive && data.globalReason && (
        <div className="mt-1 text-[10px] text-red-200">{data.globalReason}</div>
      )}
    </div>

    {data.activeAccounts.length > 0 && (
      <div className="space-y-1">
        {data.activeAccounts.slice(0, 4).map((a) => (
          <div
            key={a.accountId}
            className="rounded bg-amber-50 px-2 py-1 text-[11px] dark:bg-amber-900/20"
          >
            <div className="flex items-center justify-between">
              <span className="font-medium">{a.accountName}</span>
              <span className="text-[10px] text-gray-500">
                {new Date(a.activatedAt).toLocaleDateString("fa-IR")}
              </span>
            </div>
            {a.reason && (
              <div className="text-[10px] text-amber-700 dark:text-amber-300">
                {a.reason}
              </div>
            )}
          </div>
        ))}
        {data.activeAccounts.length > 4 && (
          <div className="text-[10px] text-gray-500">
            + {data.activeAccounts.length - 4} دیگر
          </div>
        )}
      </div>
    )}
  </div>
);

export default TraderKillSwitchWidget;