import React, { useMemo, useState } from "react";
import type {
  AccountInfoView,
  TradingRunInfoView,
  LossStreakState,
} from "../../../models";
import { formatCooldown, isCircuitActive } from "../../../models";
import { ConfirmActionModal } from "./ConfirmActionModal";

interface Props {
  activeCircuits: LossStreakState[];
  accounts: AccountInfoView[];
  runs: TradingRunInfoView[];
  busyKey: string | null;
  onReset: (runId: string, accountId: string, reason: string | null) => Promise<void>;
  onResetAll: () => Promise<void>;
}

type ModalState =
  | { kind: "reset-one"; runId: string; accountId: string; runName: string; accountName: string }
  | { kind: "reset-all" }
  | null;

export const CircuitBreakerCard: React.FC<Props> = ({
  activeCircuits,
  accounts,
  runs,
  busyKey,
  onReset,
  onResetAll,
}) => {
  const [modal, setModal] = useState<ModalState>(null);

  const accountById = useMemo(
    () => new Map(accounts.map((a) => [a.id, a])),
    [accounts],
  );
  const runById = useMemo(
    () => new Map(runs.map((r) => [r.id, r])),
    [runs],
  );

  // فقط Circuitهای واقعاً فعال (cooldown منقضی نشده)
  const liveCircuits = activeCircuits.filter(isCircuitActive);

  const closeModal = () => setModal(null);

  const confirmAction = async (reason: string | null) => {
    if (!modal) return;
    try {
      if (modal.kind === "reset-one") {
        await onReset(modal.runId, modal.accountId, reason);
      } else if (modal.kind === "reset-all") {
        await onResetAll();
      }
      closeModal();
    } catch (e) {
      alert(e instanceof Error ? e.message : "خطا");
    }
  };

  return (
    <>
      <div
        className={`rounded-lg border p-4 ${
          liveCircuits.length > 0
            ? "border-amber-500 bg-amber-50/30 dark:border-amber-600 dark:bg-amber-950/20"
            : "border-gray-200 dark:border-gray-700"
        }`}
      >
        <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
          <div>
            <h3 className="text-base font-bold text-gray-900 dark:text-white">
              ⚡ Circuit Breaker
            </h3>
            <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
              پس از N ضرر متوالی، Orderهای جدید برای (Run, Account) متوقف
              می‌شوند تا Cooldown تمام شود.
            </p>
          </div>
          <div className="flex items-center gap-2">
            <span
              className={`rounded-full px-3 py-1 text-xs font-medium ${
                liveCircuits.length > 0
                  ? "bg-amber-600 text-white"
                  : "bg-emerald-700 text-white"
              }`}
            >
              {liveCircuits.length > 0
                ? `⚡ ${liveCircuits.length} فعال`
                : "✅ بدون Circuit"}
            </span>
            {liveCircuits.length > 0 && (
              <button
                type="button"
                onClick={() => setModal({ kind: "reset-all" })}
                disabled={busyKey === "reset-all"}
                className="rounded-lg bg-blue-600 px-3 py-1 text-xs font-medium text-white hover:bg-blue-700 disabled:opacity-50"
              >
                🔄 Reset همه
              </button>
            )}
          </div>
        </div>

        {liveCircuits.length === 0 ? (
          <p className="rounded border border-dashed border-gray-300 p-4 text-center text-xs text-gray-500 dark:border-gray-600">
            هیچ Circuit فعالی وجود ندارد — همه‌ی Runها به‌طور عادی اجرا می‌شوند
          </p>
        ) : (
          <div className="overflow-x-auto rounded border border-gray-200 dark:border-gray-700">
            <table className="w-full text-right text-sm">
              <thead className="bg-gray-50 text-xs dark:bg-gray-700">
                <tr>
                  <th className="p-2">پلن</th>
                  <th className="p-2">حساب</th>
                  <th className="p-2 text-center">ضرر متوالی</th>
                  <th className="p-2 text-center">Cooldown باقی‌مانده</th>
                  <th className="p-2 text-center">عملیات</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                {liveCircuits.map((c) => {
                  const runName =
                    runById.get(c.runId)?.name ?? c.runId.substring(0, 8);
                  const accountName =
                    accountById.get(c.accountId)?.name ??
                    c.accountId.substring(0, 8);
                  const resetKey = `reset:${c.runId}:${c.accountId}`;

                  return (
                    <tr
                      key={`${c.runId}:${c.accountId}`}
                      className="hover:bg-gray-50 dark:hover:bg-gray-800/50"
                    >
                      <td className="p-2 font-medium">{runName}</td>
                      <td className="p-2">{accountName}</td>
                      <td className="p-2 text-center font-mono">
                        {c.consecutiveLosses}
                      </td>
                      <td className="p-2 text-center text-xs text-amber-600 dark:text-amber-400">
                        ⏱ {formatCooldown(c.cooldownExpiresAt)}
                      </td>
                      <td className="p-2 text-center">
                        <button
                          type="button"
                          onClick={() =>
                            setModal({
                              kind: "reset-one",
                              runId: c.runId,
                              accountId: c.accountId,
                              runName,
                              accountName,
                            })
                          }
                          disabled={busyKey === resetKey}
                          className="rounded bg-blue-600 px-3 py-1 text-xs font-medium text-white hover:bg-blue-700 disabled:opacity-50"
                        >
                          🔄 Reset
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* ─── Modals ─── */}
      <ConfirmActionModal
        isOpen={modal?.kind === "reset-one"}
        title={`Reset Circuit Breaker`}
        message={
          modal?.kind === "reset-one"
            ? `Circuit فعال برای پلن «${modal.runName}» و حساب «${modal.accountName}» ریست می‌شود. Orderهای جدید از سر گرفته می‌شوند.`
            : ""
        }
        warning="مطمئن شو که دلیل ضررها را بررسی کرده‌ای. Reset بدون بررسی می‌تواند به ضرر بیشتر منجر شود."
        showReasonInput
        reasonPlaceholder="مثلاً: بررسی شد، استراتژی اصلاح شد"
        confirmLabel="Reset"
        confirmColor="blue"
        saving={
          modal?.kind === "reset-one" &&
          busyKey === `reset:${modal.runId}:${modal.accountId}`
        }
        onCancel={closeModal}
        onConfirm={confirmAction}
      />

      <ConfirmActionModal
        isOpen={modal?.kind === "reset-all"}
        title="Reset همه‌ی Circuit Breakerها"
        message={`${liveCircuits.length} Circuit فعال ریست می‌شوند. Orderهای جدید برای همه‌ی (Run, Account) از سر گرفته می‌شوند.`}
        warning="این یک اقدام گسترده است. مطمئن شو که علت ضررها را بررسی کرده‌ای."
        confirmLabel="Reset همه"
        confirmColor="blue"
        saving={busyKey === "reset-all"}
        onCancel={closeModal}
        onConfirm={confirmAction}
      />
    </>
  );
};