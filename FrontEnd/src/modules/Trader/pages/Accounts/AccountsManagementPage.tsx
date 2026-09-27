import React, { useState } from "react";
import { GenericCrudPage } from "@/core/components/crud/components/GenericCrudPage";
import { GenericColumnDef } from "@/core/components/crud/types";
import { accountCrudApi } from "../../hooks/useAccountManagement";
import { accountApi } from "../../api/accountApi";
import type {
  AccountInfoView,
  CreateAccountCommand,
  UpdateAccountCommand,
} from "../../models";
import { TokenStatusBadge } from "../SchedulePlans/components/TokenStatusBadge";

const columns: GenericColumnDef<AccountInfoView>[] = [
  { key: "name", label: "نام حساب", type: "text", required: true },
  {
    key: "username",
    label: "نام کاربری (کد ملی)",
    type: "text",
    dir: "ltr",
    required: true,
  },
  {
    key: "password",
    label: "رمز عبور",
    type: "text",
    required: true,
    render: () => "••••••",
  },
  {
    key: "sessionStatus",
    label: "وضعیت نشست",
    editable: false,
    render: (_v, item) => <TokenStatusBadge status={item.sessionStatus} />,
  },
];

/* ═══════════════════════════════════════════════════════
   دکمه‌ی لاگین همه
   ═══════════════════════════════════════════════════════ */
interface LoginAllResult {
  accountId: string;
  accountName: string;
  ok: boolean;
  error?: string;
}

const LoginAllButton: React.FC<{
  accounts: AccountInfoView[];
  onComplete: () => void;
}> = ({ accounts, onComplete }) => {
  const [busy, setBusy] = useState(false);
  const [results, setResults] = useState<LoginAllResult[] | null>(null);
  const [progress, setProgress] = useState<{ current: number; total: number } | null>(null);

  const handleLoginAll = async () => {
    if (accounts.length === 0) {
      alert("هیچ حسابی وجود ندارد");
      return;
    }

    if (!confirm(`لاگین همه‌ی ${accounts.length} حساب؟`)) return;

    setBusy(true);
    setResults(null);
    setProgress({ current: 0, total: accounts.length });

    const list: LoginAllResult[] = [];

    /* ─── لوپ سریال — چون هر login سنگین است (OIDC flow) ─── */
    for (let i = 0; i < accounts.length; i++) {
      const acc = accounts[i];
      setProgress({ current: i + 1, total: accounts.length });

      try {
        await accountApi.login(acc.id);
        list.push({
          accountId: acc.id,
          accountName: acc.name,
          ok: true,
        });
      } catch (e) {
        list.push({
          accountId: acc.id,
          accountName: acc.name,
          ok: false,
          error: e instanceof Error ? e.message : "خطای نامشخص",
        });
      }
    }

    setResults(list);
    setBusy(false);
    setProgress(null);
    onComplete();
  };

  const successCount = results?.filter((r) => r.ok).length ?? 0;
  const failCount = results?.filter((r) => !r.ok).length ?? 0;

  return (
    <div className="mb-4 rounded-xl border border-slate-700 bg-slate-900 p-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={handleLoginAll}
            disabled={busy || accounts.length === 0}
            className="rounded-lg bg-emerald-600 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-500 disabled:opacity-50"
          >
            {busy
              ? `⏳ در حال لاگین... (${progress?.current}/${progress?.total})`
              : "🔐 لاگین همه"}
          </button>

          <span className="text-xs text-slate-400">
            {accounts.length} حساب
          </span>
        </div>

        {results && (
          <div className="flex flex-wrap gap-2 text-xs">
            <span className="rounded bg-emerald-900/40 px-2 py-1 text-emerald-300">
              ✅ {successCount} موفق
            </span>
            {failCount > 0 && (
              <span className="rounded bg-red-900/40 px-2 py-1 text-red-300">
                ❌ {failCount} ناموفق
              </span>
            )}
          </div>
        )}
      </div>

      {/* ─── گزارش نتیجه ─── */}
      {results && results.length > 0 && (
        <div className="mt-3 space-y-1 border-t border-slate-700 pt-3">
          {results.map((r) => (
            <div
              key={r.accountId}
              className={`flex items-center justify-between rounded px-2 py-1 text-xs ${
                r.ok
                  ? "bg-emerald-950/20 text-emerald-300"
                  : "bg-red-950/20 text-red-300"
              }`}
            >
              <span className="font-medium">{r.accountName}</span>
              <span className="font-mono text-[11px]">
                {r.ok ? "✅ موفق" : `❌ ${r.error}`}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

/* ═══════════════════════════════════════════════════════
   صفحه‌ی اصلی
   ═══════════════════════════════════════════════════════ */
export const AccountsManagementPage: React.FC = () => {
  /* برای گرفتن لیست حساب‌ها قبل از رندر دکمه */
  const [accounts, setAccounts] = useState<AccountInfoView[]>([]);
  const [reloadKey, setReloadKey] = useState(0);

  const loadAccounts = React.useCallback(async () => {
    try {
      const list = await accountApi.GetList();
      setAccounts(list);
    } catch {
      setAccounts([]);
    }
  }, []);

  React.useEffect(() => {
    void loadAccounts();
  }, [loadAccounts, reloadKey]);

  const handleLoginComplete = () => {
    /* بعد از لاگین همه، لیست رو refresh کن (sessionStatus عوض شده) */
    void loadAccounts();
    /* و GenericCrudPage رو هم مجبور به refresh کن */
    setReloadKey((k) => k + 1);
  };

  return (
    <div>
      <LoginAllButton accounts={accounts} onComplete={handleLoginComplete} />

      <GenericCrudPage<
        AccountInfoView,
        CreateAccountCommand,
        UpdateAccountCommand
      >
        key={reloadKey}
        title="مدیریت حساب‌ها"
        columns={columns}
        crudOptions={{
          api: accountCrudApi,
          columns,
          mapToUpdateCommand: (item) => ({
            id: item.id,
            name: item.name,
            username: item.username,
            password: undefined,
          }),
          mapToCreateCommand: (formData) => ({
            broker: 1,
            name: formData.name,
            username: formData.username,
            password: formData.password ?? "",
          }),
          tableFeatures: {
            enableSearch: true,
            enableDelete: true,
            enableColumnFilter: false,
          },
          pageFeatures: {
            enableAdd: true,
          },
        }}
      />
    </div>
  );
};