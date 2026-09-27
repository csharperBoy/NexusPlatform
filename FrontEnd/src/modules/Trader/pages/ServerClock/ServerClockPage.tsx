import { useEffect, useState } from "react";
import { serverClockApi } from "../../api";
import type { ServerClockInfoView } from "../../models";

function Card({ title, value }: { title: string; value: string | number }) {
  return (
    <div className="rounded-lg border border-slate-700 bg-slate-950 p-3">
      <div className="mb-1 text-[11px] text-slate-400">{title}</div>
      <div className="font-mono text-sm font-bold text-slate-100">{value}</div>
    </div>
  );
}

export function ServerClockPage() {
  const [info, setInfo] = useState<ServerClockInfoView | null>(null);
  const [busy, setBusy] = useState(false);
  const [tick, setTick] = useState(0);

  const reload = () => {
    void serverClockApi.GetStatus().then(setInfo).catch(() => {});
  };

  useEffect(() => {
    reload();
    const id = setInterval(() => setTick((x) => x + 1), 1000);
    return () => clearInterval(id);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  /* هر ۱۰ ثانیه یه بار refresh */
  useEffect(() => {
    const id = setInterval(reload, 10000);
    return () => clearInterval(id);
  }, []);

  const handleSync = async (samples: number) => {
    setBusy(true);
    try {
      const fresh = await serverClockApi.Sync(samples);
      setInfo(fresh);
    } finally {
      setBusy(false);
    }
  };

  if (!info) {
    return <div className="text-xs text-slate-500">در حال بارگذاری...</div>;
  }

  const sinceLast = info.lastUpdatedAt
    ? Math.round((Date.now() - info.lastUpdatedAt) / 1000)
    : null;

  return (
    <div className="space-y-4 rounded-xl border border-slate-800 bg-slate-900 p-4">
      <h1 className="text-lg font-bold text-slate-200">همگام‌سازی ساعت سرور</h1>

      <div className="grid grid-cols-2 gap-3 md:grid-cols-3">
        <Card title="diff نهایی" value={`${info.diff}ms`} />
        <Card title="offset" value={`${info.offset}ms`} />
        <Card title="تاخیر یک‌طرفه" value={`${info.oneWayLatency}ms`} />
        <Card title="نمونه‌ها" value={info.samplesCount} />
        <Card
          title="آخرین sync"
          value={sinceLast !== null ? `${sinceLast} ثانیه پیش` : "—"}
        />
        <Card title="وضعیت" value={info.lastError ? "⚠️ خطا" : "✅ OK"} />
      </div>

      {info.lastError && (
        <div className="rounded-lg bg-red-950/30 p-2 text-xs text-red-400">
          {info.lastError}
        </div>
      )}

      <div className="flex gap-2">
        <button
          type="button"
          className="flex-1 rounded-lg bg-emerald-600 py-3 font-bold text-white hover:bg-emerald-500 disabled:opacity-50"
          disabled={busy}
          onClick={() => handleSync(5)}
        >
          {busy ? "در حال sync..." : "⚡ sync (۵ نمونه)"}
        </button>
        <button
          type="button"
          className="flex-1 rounded-lg bg-slate-800 py-3 font-bold text-blue-300 hover:bg-slate-700 disabled:opacity-50"
          disabled={busy}
          onClick={() => handleSync(10)}
        >
          {busy ? "در حال sync..." : "⚡ sync قوی (۱۰ نمونه)"}
        </button>
      </div>

      <div className="rounded-lg border border-blue-900/50 bg-blue-950/20 p-3 text-xs leading-relaxed text-slate-300">
        💡 sync ساعت و محاسبه‌ی diff در بک‌اند انجام می‌شه. این صفحه فقط وضعیت
        جاری رو نشون می‌ده. برای اینکه diff تازه بمونه، بک‌اند باید هر ۳۰ ثانیه
        یه بار خودش sync کنه.
      </div>
    </div>
  );
}