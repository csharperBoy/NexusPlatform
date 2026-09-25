const COLORS: Record<string, string> = {
  empty: "bg-slate-700 text-slate-300",
  invalid: "bg-amber-900/50 text-amber-300",
  expired: "bg-red-900/50 text-red-300",
  valid: "bg-emerald-900/50 text-emerald-300",
};

const LABELS: Record<string, string> = {
  empty: "خالی",
  invalid: "نامعتبر",
  expired: "منقضی",
  valid: "معتبر",
};

export function TokenStatusBadge({ status }: { status: string }) {
  return (
    <span className={`rounded-full px-2 py-0.5 text-[11px] font-medium ${COLORS[status] ?? COLORS.empty}`}>
      {LABELS[status] ?? status}
    </span>
  );
}