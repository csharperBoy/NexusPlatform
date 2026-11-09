import React, { useEffect, useState } from "react";
import { accountApi } from "../../../api/accountApi";
import type { TradingRunAccountModel } from "../../../models";
import type { SelectionListDto } from "@/core/models/SelectionListDto";

interface Props {
  value: TradingRunAccountModel[];
  onChange: (accounts: TradingRunAccountModel[]) => void;
}

const CAPITAL_MODE_FIXED = 0;
const CAPITAL_MODE_DYNAMIC = 1;

/**
 * چک‌لیست Accountها + تخصیص سرمایه per-account.
 *
 * هر Account مستقل:
 *   • Fixed: مقدار ثابت (ریال)
 *   • Dynamic: درصد از BuyPower لحظه‌ی Order (0, 100]
 *
 * یک Account می‌تواند در چند Run باشد — بدون رزرو.
 * % Dynamic نسبت به BuyPower لحظه‌ی Order محاسبه می‌شود.
 */
export const TradingRunAccountEditor: React.FC<Props> = ({
  value,
  onChange,
}) => {
  const [options, setOptions] = useState<SelectionListDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        const list = await accountApi.GetSelectionList();
        setOptions(list);
      } catch {
        setOptions([]);
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  if (loading) {
    return (
      <p className="text-xs text-gray-500">در حال بارگذاری حساب‌ها...</p>
    );
  }

  if (options.length === 0) {
    return (
      <p className="rounded border border-dashed border-amber-500 p-3 text-xs text-amber-600 dark:border-amber-600">
        هیچ حساب معاملاتی ثبت نشده است. ابتدا از صفحه‌ی حساب‌ها یک حساب
        اضافه کنید.
      </p>
    );
  }

  const getSelected = (accountId: string) =>
    value.find((v) => v.accountId === accountId);

  const toggle = (accountId: string) => {
    if (getSelected(accountId)) {
      onChange(value.filter((v) => v.accountId !== accountId));
    } else {
      onChange([
        ...value,
        {
          accountId,
          capitalMode: CAPITAL_MODE_DYNAMIC,
          fixedCapitalAmount: null,
          percentOfBuyPower: 100,
        },
      ]);
    }
  };

  const update = (
    accountId: string,
    patch: Partial<TradingRunAccountModel>,
  ) => {
    onChange(
      value.map((v) =>
        v.accountId === accountId ? { ...v, ...patch } : v,
      ),
    );
  };

  const setMode = (accountId: string, mode: number) => {
    if (mode === CAPITAL_MODE_FIXED) {
      update(accountId, {
        capitalMode: CAPITAL_MODE_FIXED,
        percentOfBuyPower: null,
        fixedCapitalAmount: null,
      });
    } else {
      update(accountId, {
        capitalMode: CAPITAL_MODE_DYNAMIC,
        fixedCapitalAmount: null,
        percentOfBuyPower: 100,
      });
    }
  };

  return (
    <div className="space-y-2">
      <div className="overflow-hidden rounded border border-gray-200 dark:border-gray-700">
        <div className="divide-y divide-gray-200 dark:divide-gray-700">
          {options.map((o) => {
            const selected = getSelected(o.value);
            const isFixed = selected?.capitalMode === CAPITAL_MODE_FIXED;
            const isDynamic = selected?.capitalMode === CAPITAL_MODE_DYNAMIC;

            const fixedInvalid =
              isFixed &&
              (!selected?.fixedCapitalAmount ||
                selected.fixedCapitalAmount <= 0);

            const percentInvalid =
              isDynamic &&
              (selected?.percentOfBuyPower == null ||
                selected.percentOfBuyPower <= 0 ||
                selected.percentOfBuyPower > 100);

            return (
              <div
                key={o.value}
                className={`space-y-2 p-3 ${
                  selected
                    ? "bg-blue-50 dark:bg-blue-900/20"
                    : "hover:bg-gray-50 dark:hover:bg-gray-800/50"
                }`}
              >
                {/* Row 1: checkbox + name */}
                <div className="flex items-center gap-3">
                  <input
                    type="checkbox"
                    checked={!!selected}
                    onChange={() => toggle(o.value)}
                    className="h-4 w-4 rounded border-gray-300 text-blue-600"
                  />
                  <div className="flex-1">
                    <p className="text-sm font-medium text-gray-900 dark:text-white">
                      {o.display || o.label}
                    </p>
                    <p className="font-mono text-[10px] text-gray-400">
                      {o.value}
                    </p>
                  </div>
                </div>

                {/* Row 2: capital config (only when selected) */}
                {selected && (
                  <div className="flex flex-wrap items-center gap-2 pr-7">
                    {/* Mode toggle */}
                    <div className="inline-flex overflow-hidden rounded border border-gray-300 text-xs dark:border-gray-600">
                      <button
                        type="button"
                        onClick={() =>
                          setMode(o.value, CAPITAL_MODE_FIXED)
                        }
                        className={`px-3 py-1 ${
                          isFixed
                            ? "bg-blue-600 text-white"
                            : "bg-white text-gray-700 hover:bg-gray-100 dark:bg-gray-700 dark:text-gray-300"
                        }`}
                      >
                        ثابت
                      </button>
                      <button
                        type="button"
                        onClick={() =>
                          setMode(o.value, CAPITAL_MODE_DYNAMIC)
                        }
                        className={`px-3 py-1 ${
                          isDynamic
                            ? "bg-blue-600 text-white"
                            : "bg-white text-gray-700 hover:bg-gray-100 dark:bg-gray-700 dark:text-gray-300"
                        }`}
                      >
                        داینامیک
                      </button>
                    </div>

                    {/* Amount / Percent input */}
                    {isFixed && (
                      <>
                        <input
                          type="number"
                          value={selected.fixedCapitalAmount ?? ""}
                          onChange={(e) =>
                            update(o.value, {
                              fixedCapitalAmount:
                                e.target.value === ""
                                  ? null
                                  : Number(e.target.value),
                            })
                          }
                          min={0}
                          step={1000}
                          dir="ltr"
                          placeholder="مبلغ (ریال)"
                          className={`w-40 rounded border p-1 text-sm ${
                            fixedInvalid
                              ? "border-red-500 bg-red-50 dark:bg-red-900/20"
                              : "border-gray-300 dark:border-gray-600 dark:bg-gray-700"
                          } dark:text-white`}
                        />
                        <span className="text-xs text-gray-500">ریال</span>
                      </>
                    )}

                    {isDynamic && (
                      <>
                        <input
                          type="number"
                          value={selected.percentOfBuyPower ?? ""}
                          onChange={(e) =>
                            update(o.value, {
                              percentOfBuyPower:
                                e.target.value === ""
                                  ? null
                                  : Number(e.target.value),
                            })
                          }
                          min={0.01}
                          max={100}
                          step={0.01}
                          dir="ltr"
                          placeholder="درصد"
                          className={`w-24 rounded border p-1 text-sm ${
                            percentInvalid
                              ? "border-red-500 bg-red-50 dark:bg-red-900/20"
                              : "border-gray-300 dark:border-gray-600 dark:bg-gray-700"
                          } dark:text-white`}
                        />
                        <span className="text-xs text-gray-500">
                          % از BuyPower
                        </span>
                      </>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>

      {value.length > 0 && (
        <div className="rounded bg-slate-100 p-2 text-[10px] text-slate-600 dark:bg-slate-700 dark:text-slate-300">
          ℹ️ <strong>ثابت:</strong> مقدار مشخصی از سرمایه (ریال).{" "}
          <strong>داینامیک:</strong> درصدی از BuyPower لحظه‌ی Order آن
          حساب. جمع درصدها لازم نیست ۱۰۰ باشد — هر Account مستقل است. اگر
          چند Run روی یک Account باشند، BuyPower بین آن‌ها مشترک است و هر
          Run در لحظه‌ی Order سهم خودش را می‌بیند.
        </div>
      )}
    </div>
  );
};