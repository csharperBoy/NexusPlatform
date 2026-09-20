interface Props {
  remaining: number | null;
  nextLabel: string;
}

export function Countdown({ remaining, nextLabel }: Props) {
  if (remaining === null && !nextLabel) return null;

  return (
    <div className="mt-4 rounded-lg bg-slate-800 p-3 text-center">
      {nextLabel && (
        <div className="mb-1 text-xs text-blue-300">{nextLabel}</div>
      )}
      {remaining !== null && (
        <div className="text-2xl font-bold tabular-nums text-yellow-400">
          {remaining > 0
            ? `⏱ ${(remaining / 1000).toFixed(3)} ثانیه`
            : "🏁 ارسال..."}
        </div>
      )}
    </div>
  );
}