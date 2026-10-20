import React, { useEffect, useState } from "react";
import { StopRunBehavior, STOP_BEHAVIOR_LABELS } from "../../../models";

interface Props {
  isOpen: boolean;
  runName: string;
  onCancel: () => void;
  onConfirm: (behavior: StopRunBehavior) => void;
  saving: boolean;
}

export const StopRunConfirmModal: React.FC<Props> = ({
  isOpen,
  runName,
  onCancel,
  onConfirm,
  saving,
}) => {
  const [behavior, setBehavior] = useState<StopRunBehavior>(
    StopRunBehavior.KeepPositions,
  );

  useEffect(() => {
    if (isOpen) setBehavior(StopRunBehavior.KeepPositions);
  }, [isOpen]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl dark:bg-gray-800">
        <h3 className="mb-3 text-lg font-bold text-gray-900 dark:text-white">
          توقف پلن معاملاتی
        </h3>
        <p className="mb-4 text-sm text-gray-600 dark:text-gray-300">
          پلن <strong className="text-gray-900 dark:text-white">{runName}</strong>{" "}
          متوقف خواهد شد. تکلیف موقعیت‌های باز چیست؟
        </p>

        <div className="mb-4 space-y-2">
          {Object.entries(STOP_BEHAVIOR_LABELS).map(([k, v]) => {
            const val = Number(k) as StopRunBehavior;
            return (
              <label
                key={k}
                className={`flex cursor-pointer items-start gap-3 rounded-lg border p-3 ${
                  behavior === val
                    ? "border-blue-500 bg-blue-50 dark:bg-blue-900/20"
                    : "border-gray-200 dark:border-gray-700"
                }`}
              >
                <input
                  type="radio"
                  checked={behavior === val}
                  onChange={() => setBehavior(val)}
                  className="mt-1"
                />
                <div>
                  <p className="text-sm font-medium text-gray-900 dark:text-white">
                    {v}
                  </p>
                  <p className="mt-0.5 text-xs text-gray-500">
                    {val === StopRunBehavior.KeepPositions
                      ? "موقعیت‌های باز باقی می‌مانند و می‌توانید بعداً مدیریت کنید."
                      : "سفارش فروش فوری برای همه‌ی موقعیت‌های باز ارسال می‌شود."}
                  </p>
                </div>
              </label>
            );
          })}
        </div>

        <div className="flex justify-end gap-2 border-t border-gray-200 pt-4 dark:border-gray-700">
          <button
            type="button"
            onClick={onCancel}
            className="rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
          >
            انصراف
          </button>
          <button
            type="button"
            onClick={() => onConfirm(behavior)}
            disabled={saving}
            className="rounded-lg bg-amber-600 px-4 py-2 text-sm font-medium text-white hover:bg-amber-700 disabled:opacity-50"
          >
            {saving ? "در حال توقف..." : "توقف پلن"}
          </button>
        </div>
      </div>
    </div>
  );
};