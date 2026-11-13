import React from "react";
import { Link } from "react-router-dom";

interface Props {
  title: string;
  subtitle?: string;
  linkTo?: string;
  linkLabel?: string;
  tone?: "neutral" | "warning" | "danger" | "success";
  isLoading?: boolean;
  error?: string | null;
  children: React.ReactNode;
}

export const WidgetShell: React.FC<Props> = ({
  title,
  subtitle,
  linkTo,
  linkLabel = "مشاهده",
  tone = "neutral",
  isLoading = false,
  error = null,
  children,
}) => {
  const borderTone =
    tone === "danger"
      ? "border-red-700 dark:border-red-600"
      : tone === "warning"
        ? "border-amber-700 dark:border-amber-600"
        : tone === "success"
          ? "border-emerald-700 dark:border-emerald-600"
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
            {linkLabel} →
          </Link>
        )}
      </div>

      {isLoading && <Skeleton />}
      {!isLoading && error && <ErrorState message={error} />}
      {!isLoading && !error && children}
    </div>
  );
};

/* ═══════════════ Stat tile ═══════════════ */

export const Stat: React.FC<{
  label: string;
  value: React.ReactNode;
  tone?: "neutral" | "success" | "warning" | "danger";
}> = ({ label, value, tone = "neutral" }) => {
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

/* ═══════════════ Skeleton / Error ═══════════════ */

const Skeleton: React.FC = () => (
  <div className="space-y-2 animate-pulse">
    <div className="h-4 w-3/4 rounded bg-gray-200 dark:bg-gray-700" />
    <div className="h-4 w-1/2 rounded bg-gray-200 dark:bg-gray-700" />
    <div className="h-4 w-2/3 rounded bg-gray-200 dark:bg-gray-700" />
  </div>
);

const ErrorState: React.FC<{ message: string }> = ({ message }) => (
  <div className="rounded bg-red-50 p-2 text-[11px] text-red-700 dark:bg-red-900/30 dark:text-red-400">
    ⚠️ {message}
  </div>
);