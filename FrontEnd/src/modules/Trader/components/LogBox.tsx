import { useLogStore } from "../stores";
import type { LogType } from "../stores/useLogStore";

const colorOf = (type: LogType) =>
  ({
    ok: "text-green-400",
    err: "text-red-400",
    info: "text-blue-300",
    send: "text-yellow-400",
  })[type] || "text-slate-300";

export function LogBox() {
  const entries = useLogStore((s) => s.entries);
  const clear = useLogStore((s) => s.clear);

  return (
    <div className="mt-4 rounded-lg border border-slate-700 bg-slate-950 p-3">
      <div className="mb-2 flex items-center justify-between text-xs text-slate-400">
        <span>لاگ ({entries.length})</span>
        <button
          type="button"
          className="rounded bg-slate-800 px-2 py-0.5 text-blue-300 hover:bg-slate-700"
          onClick={clear}
        >
          پاک کن
        </button>
      </div>
      <div className="max-h-64 overflow-auto font-mono text-[11px] leading-6">
        {entries.length === 0 && (
          <div className="text-slate-600">لاگی نیست.</div>
        )}
        {entries.map((l, i) => (
          <div key={i} className={colorOf(l.type)}>
            <span className="text-slate-500">{l.t}</span> {l.msg}
          </div>
        ))}
      </div>
    </div>
  );
}