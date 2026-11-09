import React, { useEffect, useState } from "react";

type ConfirmColor = "red" | "amber" | "emerald" | "blue";

interface Props {
  isOpen: boolean;
  title: string;
  message: string;
  warning?: string;
  showReasonInput?: boolean;
  reasonPlaceholder?: string;
  confirmLabel: string;
  confirmColor: ConfirmColor;
  saving: boolean;
  onCancel: () => void;
  onConfirm: (reason: string | null) => void;
}

const colorClasses: Record<ConfirmColor, string> = {
  red: "bg-red-600 hover:bg-red-700",
  amber: "bg-amber-600 hover:bg-amber-700",
  emerald: "bg-emerald-600 hover:bg-emerald-700",
  blue: "bg-blue-600 hover:bg-blue-700",
};

export const ConfirmActionModal: React.FC<Props> = ({
  isOpen,
  title,
  message,
  warning,
  showReasonInput,
  reasonPlaceholder,
  confirmLabel,
  confirmColor,
  saving,
  onCancel,
  onConfirm,
}) => {
  const [reason, setReason] = useState("");

  useEffect(() => {
    if (isOpen) setReason("");
  }, [isOpen]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl dark:bg-gray-800">
        <h3 className="mb-2 text-lg font-bold text-gray-900 dark:text-white">
          {title}
        </h3>
        <p className="mb-3 text-sm text-gray-600 dark:text-gray-300">
          {message}
        </p>

        {warning && (
          <div className="mb-3 rounded-lg border border-amber-500 bg-amber-50 p-3 text-xs text-amber-700 dark:bg-amber-900/20 dark:text-amber-300">
            ⚠️ {warning}
          </div>
        )}

        {showReasonInput && (
          <div className="mb-4">
            <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
              دلیل (اختیاری)
            </label>
            <textarea
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder={reasonPlaceholder}
              rows={2}
              className="w-full rounded border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            />
          </div>
        )}

        <div className="flex justify-end gap-2 border-t border-gray-200 pt-4 dark:border-gray-700">
          <button
            type="button"
            onClick={onCancel}
            disabled={saving}
            className="rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 disabled:opacity-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
          >
            انصراف
          </button>
          <button
            type="button"
            onClick={() => onConfirm(reason.trim() || null)}
            disabled={saving}
            className={`rounded-lg px-4 py-2 text-sm font-medium text-white disabled:opacity-50 ${colorClasses[confirmColor]}`}
          >
            {saving ? "در حال اجرا..." : confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
};