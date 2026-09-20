import { useState } from "react";
import {
  useAccountsStore,
  useSymbolsStore,
  useLoginStore,
  useLoginLogStore,
  getTokenStatus,
} from "../stores";
import { TokenStatusBadge } from "../components";
import { parseOrderJson } from "../utils";
import { logTimestamp } from "../utils";
import type { Symbol } from "../models";

type Tab = "accounts" | "symbols";

export function BaseInfoPage() {
  const [tab, setTab] = useState<Tab>("accounts");

  return (
    <div className="space-y-4">
      <h1 className="text-lg font-bold text-slate-200">اطلاعات پایه</h1>

      {/* Tabs */}
      <div className="flex gap-2 border-b border-slate-800">
        {(
          [
            { id: "accounts", label: "حساب‌ها" },
            { id: "symbols", label: "نمادها" },
          ] as const
        ).map((t) => (
          <button
            key={t.id}
            type="button"
            onClick={() => setTab(t.id)}
            className={[
              "px-4 py-2 text-sm font-semibold transition-colors",
              tab === t.id
                ? "border-b-2 border-green-500 text-green-400"
                : "text-slate-400 hover:text-slate-200",
            ].join(" ")}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === "accounts" && <AccountsSection />}
      {tab === "symbols" && <SymbolsSection />}
    </div>
  );
}

/* ══════════════════════════════════════════════
   بخش حساب‌ها
   ══════════════════════════════════════════════ */
function AccountsSection() {
  const accounts = useAccountsStore((s) => s.accounts);
  const addAccount = useAccountsStore((s) => s.addAccount);
  const updateAccount = useAccountsStore((s) => s.updateAccount);
  const removeAccount = useAccountsStore((s) => s.removeAccount);
  const clearAllTokens = useAccountsStore((s) => s.clearAllTokens);
  const clearAllCredentials = useAccountsStore((s) => s.clearAllCredentials);
  const autoLogin = useAccountsStore((s) => s.autoLogin);
  const setAutoLogin = useAccountsStore((s) => s.setAutoLogin);

  const loginAccount = useLoginStore((s) => s.loginAccount);
  const loginAll = useLoginStore((s) => s.loginAll);
  const busyIds = useLoginStore((s) => s.busyIds);

  const logEntries = useLoginLogStore((s) => s.entries);
  const clearLog = useLoginLogStore((s) => s.clear);

  const [visiblePwd, setVisiblePwd] = useState<Record<string, boolean>>({});
  const [visibleTok, setVisibleTok] = useState<Record<string, boolean>>({});

  return (
    <div className="space-y-4">
      {/* Actions */}
      <div className="flex flex-wrap gap-2">
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
          onClick={() => addAccount()}
        >
          + افزودن حساب
        </button>
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700 disabled:opacity-50"
          disabled={busyIds.size > 0}
          onClick={() => loginAll()}
        >
          🔐 لاگین همه
        </button>
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
          onClick={() => {
            if (confirm("همه توکن‌ها پاک بشن؟")) clearAllTokens();
          }}
        >
          🧹 پاک کردن توکن‌ها
        </button>
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
          onClick={() => {
            if (confirm("نام‌کاربری و رمزها پاک بشن؟")) clearAllCredentials();
          }}
        >
          🗑 پاک کردن رمزها
        </button>
      </div>

      {/* Auto-login switch */}
      <label className="flex cursor-pointer items-center gap-2 rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-xs text-blue-300">
        <input
          type="checkbox"
          checked={autoLogin}
          onChange={(e) => setAutoLogin(e.target.checked)}
        />
        لاگین خودکار هنگام باز شدن برنامه + تازه‌سازی قبل از انقضا
      </label>

      {/* Account cards */}
      {accounts.map((acc) => {
        const isBusy = busyIds.has(acc.id);
        return (
          <div
            key={acc.id}
            className="space-y-3 rounded-xl border border-blue-900/60 bg-slate-900 p-4"
          >
            {/* Name + Remove */}
            <div className="flex items-end gap-2">
              <div className="flex-1">
                <label className="mb-1 block text-xs text-slate-400">
                  نام حساب
                </label>
                <input
                  className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
                  value={acc.name}
                  onChange={(e) =>
                    updateAccount(acc.id, { name: e.target.value })
                  }
                />
              </div>
              <button
                type="button"
                className="rounded border border-red-900 px-2 py-1 text-xs text-red-400 hover:bg-red-950 disabled:opacity-30"
                disabled={accounts.length === 1}
                onClick={() => {
                  if (confirm(`حذف "${acc.name}"؟`)) removeAccount(acc.id);
                }}
              >
                ✕
              </button>
            </div>

            {/* Username + Password */}
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="mb-1 block text-xs text-slate-400">
                  نام کاربری (کد ملی)
                </label>
                <input
                  className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-sm"
                  value={acc.username}
                  onChange={(e) =>
                    updateAccount(acc.id, { username: e.target.value })
                  }
                  placeholder="12502831"
                />
              </div>
              <div>
                <label className="mb-1 block text-xs text-slate-400">
                  رمز عبور
                </label>
                <div className="flex gap-2">
                  <input
                    className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-sm"
                    type={visiblePwd[acc.id] ? "text" : "password"}
                    value={acc.password}
                    onChange={(e) =>
                      updateAccount(acc.id, { password: e.target.value })
                    }
                    autoComplete="new-password"
                  />
                  <button
                    type="button"
                    className="rounded-lg bg-slate-800 px-2 text-xs"
                    onClick={() =>
                      setVisiblePwd((v) => ({
                        ...v,
                        [acc.id]: !v[acc.id],
                      }))
                    }
                  >
                    {visiblePwd[acc.id] ? "🙈" : "👁"}
                  </button>
                </div>
              </div>
            </div>

            {/* Login button + status */}
            <div className="flex items-center gap-3">
              <button
                type="button"
                className="rounded-lg bg-blue-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-blue-500 disabled:opacity-50"
                disabled={
                  isBusy || !acc.username.trim() || !acc.password.trim()
                }
                onClick={() => loginAccount(acc.id)}
              >
                {isBusy ? "⏳ در حال لاگین..." : "🔐 لاگین"}
              </button>
              <TokenStatusBadge token={acc.token} />
            </div>

            {/* Token field */}
            <div>
              <label className="mb-1 block text-xs text-slate-400">
                توکن (بدون Bearer)
              </label>
              <div className="flex gap-2">
                <textarea
                  rows={2}
                  className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-xs"
                  value={acc.token}
                  onChange={(e) =>
                    updateAccount(acc.id, { token: e.target.value })
                  }
                  placeholder="خودکار پر میشه یا دستی پیست کن"
                  spellCheck={false}
                />
                <button
                  type="button"
                  className="rounded-lg bg-slate-800 px-2 text-xs"
                  onClick={() =>
                    setVisibleTok((v) => ({ ...v, [acc.id]: !v[acc.id] }))
                  }
                >
                  {visibleTok[acc.id] ? "🙈" : "👁"}
                </button>
              </div>
            </div>
          </div>
        );
      })}

      {/* Warning */}
      <div className="rounded-lg border border-yellow-900/50 bg-yellow-950/20 p-3 text-right text-xs text-yellow-400">
        ⚠️ نام‌کاربری و رمز عبور در localStorage ذخیره می‌شن (مبهم‌شده). برای
        راحتی، ولی از نظر امنیتی ایده‌آل نیست. اگه سیستم مشترک داری، auto-login
        رو خاموش کن و رمزها رو پاک کن.
      </div>

      {/* Login log */}
      {logEntries.length > 0 && (
        <div className="rounded-lg border border-slate-700 bg-slate-900 p-3">
          <div className="mb-2 flex items-center justify-between text-xs text-slate-400">
            <span>لاگ لاگین ({logEntries.length})</span>
            <button
              type="button"
              className="rounded bg-slate-800 px-2 py-0.5 text-blue-300"
              onClick={clearLog}
            >
              پاک کن
            </button>
          </div>
          <div className="max-h-48 overflow-auto font-mono text-[11px] leading-6">
            {logEntries.map((l, i) => (
              <div
                key={i}
                className={
                  l.type === "ok"
                    ? "text-green-400"
                    : l.type === "err"
                      ? "text-red-400"
                      : "text-blue-300"
                }
              >
                <span className="text-slate-500">{l.t}</span> {l.msg}
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

/* ══════════════════════════════════════════════
   بخش نمادها
   ══════════════════════════════════════════════ */
function SymbolsSection() {
  const symbols = useSymbolsStore((s) => s.symbols);
  const addSymbols = useSymbolsStore((s) => s.addSymbols);
  const updateSymbol = useSymbolsStore((s) => s.updateSymbol);
  const removeSymbol = useSymbolsStore((s) => s.removeSymbol);
  const clearAll = useSymbolsStore((s) => s.clearAll);
  const resetToDefaults = useSymbolsStore((s) => s.resetToDefaults);

  const [jsonInput, setJsonInput] = useState("");
  const [feedback, setFeedback] = useState<{
    type: "ok" | "err";
    msg: string;
  } | null>(null);

  const handleAddFromJson = () => {
    if (!jsonInput.trim()) {
      setFeedback({ type: "err", msg: "اول JSON رو پیست کن" });
      return;
    }
    try {
      const parsed = parseOrderJson(jsonInput);
      addSymbols(parsed);
      setFeedback({
        type: "ok",
        msg: `✅ ${parsed.length} نماد اضافه/آپدیت شد — ${logTimestamp()}`,
      });
      setJsonInput("");
    } catch (e) {
      setFeedback({
        type: "err",
        msg: `❌ ${e instanceof Error ? e.message : "خطا"}`,
      });
    }
  };

  const handleExport = () => {
    const text = JSON.stringify(symbols, null, 2);
    navigator.clipboard.writeText(text).then(
      () => setFeedback({ type: "ok", msg: "✅ کپی شد" }),
      () => setFeedback({ type: "err", msg: "❌ کپی نشد" }),
    );
  };

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap gap-2">
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
          onClick={handleExport}
          disabled={symbols.length === 0}
        >
          📋 خروجی JSON
        </button>
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
          onClick={() => {
            if (confirm("به مقادیر پیش‌فرض برگرده؟")) resetToDefaults();
          }}
        >
          ↺ پیش‌فرض
        </button>
        <button
          type="button"
          className="rounded-lg bg-slate-800 px-3 py-1.5 text-xs text-blue-300 hover:bg-slate-700"
          onClick={() => {
            if (confirm("همه نمادها پاک بشن؟")) clearAll();
          }}
        >
          🗑 پاک کردن همه
        </button>
      </div>

      {/* JSON paste */}
      <div className="rounded-xl border border-slate-700 bg-slate-900 p-3">
        <label className="mb-2 block text-xs text-slate-400">
          پیست JSON از کارگزاری (تک یا آرایه)
        </label>
        <textarea
          rows={4}
          className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-xs"
          value={jsonInput}
          onChange={(e) => setJsonInput(e.target.value)}
          placeholder='{"price":11380,"quantity":50000,"symbolIsin":"IRO3LABN0001","symbolName":"لبن",...}'
        />
        <button
          type="button"
          className="mt-2 w-full rounded-lg bg-green-600 py-2 text-sm font-semibold text-slate-950 hover:bg-green-500"
          onClick={handleAddFromJson}
        >
          ➕ افزودن به لیست
        </button>
        {feedback && (
          <div
            className={`mt-2 text-xs ${
              feedback.type === "ok" ? "text-green-400" : "text-red-400"
            }`}
          >
            {feedback.msg}
          </div>
        )}
      </div>

      {/* Symbols list */}
      {symbols.map((s) => (
        <SymbolCard
          key={s.symbolIsin}
          sym={s}
          onUpdate={(patch) => updateSymbol(s.symbolIsin, patch)}
          onRemove={() => {
            if (confirm(`حذف ${s.symbolName}؟`)) removeSymbol(s.symbolIsin);
          }}
        />
      ))}
    </div>
  );
}

interface SymbolCardProps {
  sym: Symbol;
  onUpdate: (patch: Partial<Symbol>) => void;
  onRemove: () => void;
}

function SymbolCard({ sym, onUpdate, onRemove }: SymbolCardProps) {
  return (
    <div className="space-y-2 rounded-xl border border-slate-700 bg-slate-900 p-3">
      <div className="flex items-end gap-2">
        <div className="flex-1">
          <label className="mb-1 block text-xs text-slate-400">نام نماد</label>
          <input
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-1.5 text-sm"
            value={sym.symbolName}
            onChange={(e) => onUpdate({ symbolName: e.target.value })}
          />
        </div>
        <div className="flex-[2]">
          <label className="mb-1 block text-xs text-slate-400">ISIN</label>
          <input
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-1.5 font-mono text-xs"
            value={sym.symbolIsin}
            onChange={(e) => onUpdate({ symbolIsin: e.target.value })}
          />
        </div>
        <button
          type="button"
          className="rounded border border-red-900 px-2 py-1 text-xs text-red-400 hover:bg-red-950"
          onClick={onRemove}
        >
          ✕
        </button>
      </div>

      <div className="grid grid-cols-3 gap-2">
        <div>
          <label className="mb-1 block text-xs text-slate-400">قیمت</label>
          <input
            type="number"
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-1.5 text-sm"
            value={sym.price}
            onChange={(e) => onUpdate({ price: Number(e.target.value) })}
          />
        </div>
        <div>
          <label className="mb-1 block text-xs text-slate-400">حجم</label>
          <input
            type="number"
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-1.5 text-sm"
            value={sym.quantity}
            onChange={(e) => onUpdate({ quantity: Number(e.target.value) })}
          />
        </div>
        <div>
          <label className="mb-1 block text-xs text-slate-400">کارمزد</label>
          <input
            type="number"
            step="0.000001"
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-1.5 text-sm"
            value={sym.commission}
            onChange={(e) => onUpdate({ commission: Number(e.target.value) })}
          />
        </div>
      </div>
    </div>
  );
}