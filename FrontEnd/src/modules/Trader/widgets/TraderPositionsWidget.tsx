import React from "react";
import { useDashboardSummary } from "../hooks/useDashboard";
import type { PositionsSummary } from "../models";
import { WidgetShell } from "./_shared/WidgetShell";

const TraderPositionsWidget: React.FC = () => {
  const { data, isLoading, error } = useDashboardSummary();
  const positions = data?.positions;

  return (
    <WidgetShell
      title="موقعیت‌های باز"
      subtitle={
        positions ? `${positions.totalCount} موقعیت (${positions.paperCount} Paper)` : undefined
      }
      linkTo="/trader/positions"
      isLoading={isLoading}
      error={error?.message ?? null}
    >
      {positions && <PositionsBody data={positions} />}
    </WidgetShell>
  );
};

const PositionsBody: React.FC<{ data: PositionsSummary }> = ({ data }) => {
  const pnlTone =
    data.totalUnrealizedPnL > 0
      ? "text-emerald-600 dark:text-emerald-400"
      : data.totalUnrealizedPnL < 0
        ? "text-red-600 dark:text-red-400"
        : "text-gray-900 dark:text-gray-100";

  return (
    <>
      <div className="mb-2 flex items-center justify-between">
        <span className="text-[10px] text-gray-500">Unrealized PnL</span>
        <span className={`font-mono text-sm font-bold ${pnlTone}`}>
          {data.totalUnrealizedPnL.toLocaleString("en-US")}
        </span>
      </div>

      {data.items.length === 0 ? (
        <p className="py-3 text-center text-xs text-gray-500">هیچ موقعیت بازی نیست</p>
      ) : (
        <div className="max-h-40 space-y-1 overflow-y-auto">
          {data.items.slice(0, 8).map((p) => (
            <div
              key={p.id}
              className="flex items-center justify-between rounded bg-slate-50 px-2 py-1 text-[11px] dark:bg-slate-700"
            >
              <div className="flex items-center gap-2">
                <span className="font-medium">{p.symbolName}</span>
                {p.isPaper && (
                  <span className="rounded bg-cyan-900/40 px-1 text-cyan-300">📄</span>
                )}
              </div>
              <div className="flex items-center gap-2 font-mono">
                <span className="text-gray-500">
                  {p.quantity.toLocaleString("en-US")}
                </span>
                <span
                  className={
                    p.unrealizedPnL != null
                      ? p.unrealizedPnL > 0
                        ? "text-emerald-600 dark:text-emerald-400"
                        : p.unrealizedPnL < 0
                          ? "text-red-600 dark:text-red-400"
                          : "text-gray-600"
                      : "text-gray-500"
                  }
                >
                  {p.unrealizedPnL != null
                    ? p.unrealizedPnL.toLocaleString("en-US")
                    : "—"}
                </span>
              </div>
            </div>
          ))}
        </div>
      )}
    </>
  );
};

export default TraderPositionsWidget;