import React, { useState } from "react";
import type {
  AccountInfoView,
  KillSwitchStatusView,
} from "../../../models";
import { ConfirmActionModal } from "./ConfirmActionModal";

interface Props {
  status: KillSwitchStatusView | null;
  accounts: AccountInfoView[];
  busyKey: string | null;
  onActivateGlobal: (reason: string | null) => Promise<void>;
  onDeactivateGlobal: () => Promise<void>;
  onActivateAccount: (accountId: string, reason: string | null) => Promise<void>;
  onDeactivateAccount: (accountId: string) => Promise<void>;
}

type ModalState =
  | { kind: "activate-global" }
  | { kind: "deactivate-global" }
  | { kind: "activate-account"; accountId: string; accountName: string }
  | { kind: "deactivate-account"; accountId: string; accountName: string }
  | null;

export const KillSwitchCard: React.FC<Props> = ({
  status,
  accounts,
  busyKey,
  onActivateGlobal,
  onDeactivateGlobal,
  onActivateAccount,
  onDeactivateAccount,
}) => {
  const [modal, setModal] = useState<ModalState>(null);

  if (status === null) {
    return (
      <div className="rounded-lg border border-gray-200 p-4 dark:border-gray-700">
        <p className="text-sm text-gray-500">در حال بارگذاری...</p>
      </div>
    );
  }

  const globalActive = status.globalActive;
  const activeAccountSet = new Set(status.activeAccountIds);

  const sortedAccounts = [...accounts].sort((a, b) => {
    const aActive = activeAccountSet.has(a.id);
    const bActive = activeAccountSet.has(b.id);
    if (aActive !== bActive) return aActive ? -1 : 1;
    return a.name.localeCompare(b.name, "fa");
  });

  const closeModal = () => setModal(null);

  const confirmAction = async (reason: string | null) => {
    if (!modal) return;
    try {
      if (modal.kind === "activate-global") {
        await onActivateGlobal(reason);
      } else if (modal.kind === "deactivate-global") {
        await onDeactivateGlobal();
      } else if (modal.kind === "activate-account") {
        await onActivateAccount(modal.accountId, reason);
      } else if (modal.kind === "deactivate-account") {
        await onDeactivateAccount(modal.accountId);
      }
      closeModal();
    } catch (e) {
      // Error از hook بالا می‌آید — Modal باز بماند تا user ببیند
      alert(e instanceof Error ? e.message : "خطا");
    }
  };

  return (
    <>
      <div
        className={`rounded-lg border p-4 ${
          globalActive
            ? "border-red-500 bg-red-50/30 dark:border-red-500 dark:bg-red-950/20"
            : "border-gray-200 dark:border-gray-700"
        }`}
      >
        <div className="mb-3 flex items-center justify-between">
          <div>
            <h3 className="text-base font-bold text-gray-900 dark:text-white">
              🚨 Kill Switch (Emergency Stop)
            </h3>
            <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
              توقف فوری Orderهای جدید — Positionهای باز با SL/TP محافظت
              می‌شوند.
            </p>
          </div>
          <span
            className={`rounded-full px-3 py-1 text-xs font-medium ${
              globalActive
                ? "bg-red-600 text-white"
                : "bg-slate-700 text-slate-300"
            }`}
          >
            {globalActive ? "🚨 فعال" : "غیرفعال"}
          </span>
        </div>

        {/* ─── Global Section ─── */}
        <div className="mb-4 rounded-lg border border-gray-200 bg-white p-3 dark:border-gray-700 dark:bg-gray-800">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div>
              <p className="text-sm font-medium text-gray-900 dark:text-white">
                Global Kill Switch
              </p>
              <p className="text-xs text-gray-500">
                همه‌ی Runها و Accountها را متوقف می‌کند
              </p>
            </div>
            {globalActive ? (
              <button
                type="button"
                onClick={() => setModal({ kind: "deactivate-global" })}
                disabled={busyKey === "deactivate-global"}
                className="rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
              >
                ✅ غیرفعال‌سازی
              </button>
            ) : (
              <button
                type="button"
                onClick={() => setModal({ kind: "activate-global" })}
                disabled={busyKey === "activate-global"}
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
              >
                🚨 فعال‌سازی
              </button>
            )}
          </div>
        </div>

        {/* ─── Per-Account ─── */}
        <div>
          <p className="mb-2 text-xs font-semibold text-gray-600 dark:text-gray-400">
            Per-Account ({status.activeAccountIds.length} فعال از {accounts.length})
          </p>

          {accounts.length === 0 ? (
            <p className="rounded border border-dashed border-gray-300 p-3 text-center text-xs text-gray-500 dark:border-gray-600">
              هیچ Account ثبت نشده است
            </p>
          ) : (
            <div className="overflow-hidden rounded border border-gray-200 dark:border-gray-700">
              <table className="w-full text-right text-sm">
                <thead className="bg-gray-50 text-xs dark:bg-gray-700">
                  <tr>
                    <th className="p-2">حساب</th>
                    <th className="p-2">وضعیت</th>
                    <th className="p-2 text-center">عملیات</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                  {sortedAccounts.map((acc) => {
                    const isActive = activeAccountSet.has(acc.id);
                    const keyActivate = `activate:${acc.id}`;
                    const keyDeactivate = `deactivate:${acc.id}`;

                    return (
                      <tr
                        key={acc.id}
                        className={
                          isActive
                            ? "bg-red-50/40 dark:bg-red-950/10"
                            : "hover:bg-gray-50 dark:hover:bg-gray-800/50"
                        }
                      >
                        <td className="p-2 font-medium">{acc.name}</td>
                        <td className="p-2">
                          {isActive ? (
                            <span className="rounded-full bg-red-600 px-2 py-0.5 text-[10px] font-medium text-white">
                              🚨 فعال
                            </span>
                          ) : (
                            <span className="rounded-full bg-slate-200 px-2 py-0.5 text-[10px] font-medium text-slate-700 dark:bg-slate-700 dark:text-slate-300">
                              ○ غیرفعال
                            </span>
                          )}
                        </td>
                        <td className="p-2 text-center">
                          {isActive ? (
                            <button
                              type="button"
                              onClick={() =>
                                setModal({
                                  kind: "deactivate-account",
                                  accountId: acc.id,
                                  accountName: acc.name,
                                })
                              }
                              disabled={busyKey === keyDeactivate}
                              className="rounded bg-emerald-600 px-3 py-1 text-xs font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
                            >
                              غیرفعال
                            </button>
                          ) : (
                            <button
                              type="button"
                              onClick={() =>
                                setModal({
                                  kind: "activate-account",
                                  accountId: acc.id,
                                  accountName: acc.name,
                                })
                              }
                              disabled={busyKey === keyActivate}
                              className="rounded bg-red-600 px-3 py-1 text-xs font-medium text-white hover:bg-red-700 disabled:opacity-50"
                            >
                              فعال
                            </button>
                          )}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {/* ─── Modals ─── */}
      <ConfirmActionModal
        isOpen={modal?.kind === "activate-global"}
        title="فعال‌سازی Global Kill Switch"
        message="تمام Orderهای جدید در همه‌ی Runها و Accountها مسدود می‌شوند. Positionهای باز دست‌نخورده باقی می‌مانند و SL/TP آن‌ها کار می‌کند."
        warning="این یک اقدام بحرانی است. اگر مطمئنی، تأیید کن."
        showReasonInput
        reasonPlaceholder="مثلاً: نوسان شدید بازار"
        confirmLabel="فعال‌سازی Global"
        confirmColor="red"
        saving={busyKey === "activate-global"}
        onCancel={closeModal}
        onConfirm={confirmAction}
      />

      <ConfirmActionModal
        isOpen={modal?.kind === "deactivate-global"}
        title="غیرفعال‌سازی Global Kill Switch"
        message="Orderهای جدید در همه‌ی Runها از سر گرفته می‌شوند. مطمئن شو که ریسک بازار را بررسی کرده‌ای."
        confirmLabel="غیرفعال‌سازی"
        confirmColor="emerald"
        saving={busyKey === "deactivate-global"}
        onCancel={closeModal}
        onConfirm={confirmAction}
      />

      <ConfirmActionModal
        isOpen={modal?.kind === "activate-account"}
        title={`فعال‌سازی Kill Switch برای «${
          modal?.kind === "activate-account" ? modal.accountName : ""
        }»`}
        message="Orderهای جدید روی این Account مسدود می‌شوند. Runهای دیگر و Accountهای دیگر ادامه می‌دهند."
        showReasonInput
        reasonPlaceholder="مثلاً: بازبینی حساب"
        confirmLabel="فعال‌سازی"
        confirmColor="red"
        saving={
          modal?.kind === "activate-account" &&
          busyKey === `activate:${modal.accountId}`
        }
        onCancel={closeModal}
        onConfirm={confirmAction}
      />

      <ConfirmActionModal
        isOpen={modal?.kind === "deactivate-account"}
        title={`غیرفعال‌سازی Kill Switch برای «${
          modal?.kind === "deactivate-account" ? modal.accountName : ""
        }»`}
        message="Orderهای جدید روی این Account از سر گرفته می‌شوند."
        confirmLabel="غیرفعال‌سازی"
        confirmColor="emerald"
        saving={
          modal?.kind === "deactivate-account" &&
          busyKey === `deactivate:${modal.accountId}`
        }
        onCancel={closeModal}
        onConfirm={confirmAction}
      />
    </>
  );
};