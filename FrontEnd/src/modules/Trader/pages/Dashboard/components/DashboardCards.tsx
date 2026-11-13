import React from "react";
import { Link } from "react-router-dom";
import type {
  AccountsSessionSummary,
  CriticalEventView,
  KillSwitchSummary,
  OrdersTodaySummary,
  PositionsSummary,
  RunsSummary,
} from "../../../models";

/* ═══════════════ Card shell ═══════════════ */

const Card: React.FC<{
  title: string;
  subtitle?: string;
  linkTo?: string;
  linkLabel?: string;
  tone?: "neutral" | "warning" | "danger";
  children: React.ReactNode;
}> = ({ title, subtitle, linkTo, linkLabel, tone = "neutral", children }) => {
  const borderTone =
    tone === "danger"
      ? "border-red-700 dark:border-red-600"
      : tone === "warning"
        ? "border-amber-700 dark:border-amber-600"
        : "border-gray-200 dark:border-gray-700";

  return (
    <div
      className={`flex flex-col rounded-xl border bg-white p-4 shadow-sm dark:bg-gray-800 ${borderTone}`}
    >
      <div className="mb-3 flex items-start justify-between gap-2">
        <div>
          <h3 className="text-sm font-bold text-gray-900 dark:text-white">{title}</h3>
          {subtitle && (
            <p className="mt-0.5 text-[11px] text-gray-500 dark:text-gray-400">{subtitle}</p>
          )}
        </div>
        {linkTo && (
          <Link
            to={linkTo}
            className="text-[11px] text-blue-600 hover:underline dark:text-blue-400"
          >
            {linkLabel ?? "مشاهده"} →
          </Link>
        )}
      </div>
      {children}
    </div>
  );
};

/* ═══════════════ Stat helper ═══════════════ */

const Stat: React.FC<{ label: string; value: React.ReactNode; tone?: "neutral" | "success" | "warning" | "danger" }> = ({
  label,
  value,
  tone = "neutral",
}) => {
  const color =
    tone === "success"
      ? "text-emerald-600 dark:text-emerald-400"
      : tone === "warning"
        ? "text-amber-600 dark:text-amber-400"
        : tone === "danger"
          ? "text-red-600 dark:text-red-400"
          : "text-gray-900 dark:text-gray-100";

  return (
    <div className="flex flex-col">
      <span className="text-[10px] text-gray-500 dark:text-gray-400">{label}</span>
      <span className={`mt-0.5 font-mono text-lg font-bold ${color}`}>{value}</span>
    </div>
  );
};

/* ═══════════════ Kill Switch ═══════════════ */

export const KillSwitchCard: React.FC<{ data: KillSwitchSummary }> = ({ data }) => {
  const tone =
    data.globalActive || data.activeAccounts.length > 0 ? "danger" : "neutral";

  return (
    <Card
      title="Kill Switch"
      subtitle={
        data.globalActive
          ? "🚨 فعال (Global)"
          : data.activeAccounts.length > 0
            ? `⚠️ ${data.activeAccounts.length} حساب فعال`
            : "غیرفعال"
      }
      linkTo="/trader/risk"
      linkLabel="مدیریت"
      tone={tone}
    >
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
    </Card>
  );
};

/* ═══════════════ Runs ═══════════════ */

export const RunsCard: React.FC<{ data: RunsSummary }> = ({ data }) => (
  <Card title="Trading Runs" subtitle={`${data.total} پلن`} linkTo="/trader/trading-runs">
    <div className="grid grid-cols-3 gap-2">
      <Stat
        label="در حال اجرا"
        value={data.running}
        tone={data.running > 0 ? "success" : "neutral"}
      />
      <Stat label="آماده" value={data.ready} />
      <Stat label="متوقف" value={data.stopped} />
      <Stat label="خطا" value={data.faulted} tone={data.faulted > 0 ? "danger" : "neutral"} />
      <Stat label="ایجاد شده" value={data.created} />
    </div>
  </Card>
);

/* ═══════════════ Orders Today ═══════════════ */

export const OrdersTodayCard: React.FC<{ data: OrdersTodaySummary }> = ({ data }) => (
  <Card
    title="سفارش‌های امروز"
    subtitle={`${data.total} سفارش — ${data.paper} Paper / ${data.live} Live`}
    linkTo="/trader/orders"
  >
    <div className="grid grid-cols-3 gap-2">
      <Stat label="کامل اجرا" value={data.fullyExecuted} tone="success" />
      <Stat label="نیمه اجرا" value={data.partiallyExecuted} tone="warning" />
      <Stat label="در انتظار / باز" value={data.pending + data.open} />
      <Stat label="رد شده" value={data.rejected} tone={data.rejected > 0 ? "danger" : "neutral"} />
      <Stat label="لغو شده" value={data.canceled} />
      <Stat label="منقضی" value={data.expired} />
    </div>
  </Card>
);

/* ═══════════════ Sessions ═══════════════ */

export const SessionsCard: React.FC<{ data: AccountsSessionSummary }> = ({ data }) => {
  const tone =
    data.expired > 0 || data.empty > 0
      ? "warning"
      : "neutral";

  return (
    <Card
      title="حساب‌های کارگزاری"
      subtitle={`${data.total} حساب`}
      linkTo="/trader/accounts"
      tone={tone}
    >
      <div className="grid grid-cols-2 gap-2">
        <Stat label="Session معتبر" value={data.valid} tone="success" />
        <Stat label="منقضی" value={data.expired} tone={data.expired > 0 ? "warning" : "neutral"} />
        <Stat label="خالی" value={data.empty} tone={data.empty > 0 ? "warning" : "neutral"} />
        <Stat label="نامعتبر" value={data.invalid} tone={data.invalid > 0 ? "danger" : "neutral"} />
      </div>
    </Card>
  );
};

/* ═══════════════ Positions ═══════════════ */

export const PositionsCard: React.FC<{ data: PositionsSummary }> = ({ data }) => {
  const pnlTone = data.totalUnrealizedPnL > 0
    ? "text-emerald-600 dark:text-emerald-400"
    : data.totalUnrealizedPnL < 0
      ? "text-red-600 dark:text-red-400"
      : "text-gray-900 dark:text-gray-100";

  return (
    <Card
      title="موقعیت‌های باز"
      subtitle={`${data.totalCount} موقعیت (${data.paperCount} Paper)`}
      linkTo="/trader/positions"
    >
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
            <div key={p.id} className="flex items-center justify-between rounded bg-slate-50 px-2 py-1 text-[11px] dark:bg-slate-700">
              <div className="flex items-center gap-2">
                <span className="font-medium">{p.symbolName}</span>
                {p.isPaper && <span className="rounded bg-cyan-900/40 px-1 text-cyan-300">📄</span>}
              </div>
              <div className="flex items-center gap-2 font-mono">
                <span className="text-gray-500">{p.quantity.toLocaleString("en-US")}</span>
                <span className={p.unrealizedPnL != null
                  ? (p.unrealizedPnL > 0
                    ? "text-emerald-600 dark:text-emerald-400"
                    : p.unrealizedPnL < 0
                      ? "text-red-600 dark:text-red-400"
                      : "text-gray-600")
                  : "text-gray-500"}>
                  {p.unrealizedPnL != null
                    ? p.unrealizedPnL.toLocaleString("en-US")
                    : "—"}
                </span>
              </div>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
};

/* ═══════════════ Recent Critical Events ═══════════════ */

export const EventsCard: React.FC<{ data: CriticalEventView[] }> = ({ data }) => (
  <Card
    title="رویدادهای مهم اخیر"
    subtitle={`${data.length} رویداد (Warning/Critical)`}
    linkTo="/trader/events"
  >
    {data.length === 0 ? (
      <p className="py-3 text-center text-xs text-gray-500">هیچ رویداد مهمی نیست ✅</p>
    ) : (
      <div className="space-y-1">
        {data.slice(0, 6).map((e) => {
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
                  {new Date(e.occurredAt).toLocaleTimeString("fa-IR", { hour: "2-digit", minute: "2-digit" })}
                </span>
              </div>
            </div>
          );
        })}
      </div>
    )}
  </Card>
);