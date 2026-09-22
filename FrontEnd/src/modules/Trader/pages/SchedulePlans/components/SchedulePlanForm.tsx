import { useState } from "react";
import type {
  CreateSchedulePlanCommand,
  OrderMode,
} from "../../../models";
import { todayDateKey, tomorrowDateKey } from "../../../utils/dateKeys";

interface Props {
  onCancel: () => void;
  onSubmit: (cmd: CreateSchedulePlanCommand) => void;
  saving: boolean;
}

export function SchedulePlanForm({ onCancel, onSubmit, saving }: Props) {
  const [name, setName] = useState("برنامه جدید");
  const [date, setDate] = useState(todayDateKey());
  const [autoLoginAt, setAutoLoginAt] = useState("08:30:00");
  const [autoRefreshAt, setAutoRefreshAt] = useState("08:44:00");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit({
      name: name.trim(),
      date,
      enabled: false,
      autoLoginAt,
      autoRefreshAt,
      orders: [],
    });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label className="mb-1 block text-xs text-slate-400">نام برنامه</label>
        <input
          className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
        />
      </div>

      <div>
        <label className="mb-1 block text-xs text-slate-400">تاریخ</label>
        <input
          type="date"
          className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 font-mono text-sm text-slate-100"
          value={date}
          onChange={(e) => setDate(e.target.value)}
          required
        />
        <div className="mt-1 flex gap-1">
          <button
            type="button"
            className="rounded border border-slate-700 bg-slate-800 px-2 py-0.5 text-[10px] text-blue-300 hover:bg-slate-700"
            onClick={() => setDate(todayDateKey())}
          >
            امروز
          </button>
          <button
            type="button"
            className="rounded border border-slate-700 bg-slate-800 px-2 py-0.5 text-[10px] text-blue-300 hover:bg-slate-700"
            onClick={() => setDate(tomorrowDateKey())}
          >
            فردا
          </button>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="mb-1 block text-xs text-slate-400">
            ساعت لاگین
          </label>
          <input
            type="text"
            className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 font-mono text-sm text-slate-100"
            value={autoLoginAt}
            onChange={(e) => setAutoLoginAt(e.target.value)}
            placeholder="08:30:00"
          />
        </div>
        <div>
          <label className="mb-1 block text-xs text-slate-400">
            ساعت رفرش قیمت
          </label>
          <input
            type="text"
            className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 font-mono text-sm text-slate-100"
            value={autoRefreshAt}
            onChange={(e) => setAutoRefreshAt(e.target.value)}
            placeholder="08:44:00"
          />
        </div>
      </div>

      <div className="flex justify-end gap-2 border-t border-slate-700 pt-3">
        <button
          type="button"
          className="rounded-lg border border-slate-600 px-4 py-2 text-sm text-slate-300 hover:bg-slate-800"
          onClick={onCancel}
        >
          انصراف
        </button>
        <button
          type="submit"
          disabled={saving}
          className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
        >
          {saving ? "در حال ثبت..." : "ثبت پلن"}
        </button>
      </div>
    </form>
  );
}