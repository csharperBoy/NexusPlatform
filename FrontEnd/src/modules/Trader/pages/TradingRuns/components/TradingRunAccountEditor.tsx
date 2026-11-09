import React, { useEffect, useState } from "react";
import { accountApi } from "../../../api/accountApi";
import type { TradingRunAccountModel } from "../../../models";
import type { SelectionListDto } from "@/core/models/SelectionListDto";

interface Props {
  value: TradingRunAccountModel[];
  onChange: (accounts: TradingRunAccountModel[]) => void;
}

/**
 * چک‌لیست Accountها + درصد تخصیص کنار هرکدام.
 *
 * نکته: جمع درصدها لازم نیست ۱۰۰ باشد. هر Account مستقل است
 * (چون چند Run می‌توانند همان Account را با درصدهای مختلف استفاده کنند).
 *
 * Percent معنایش: «حداکثر این درصد از BuyPower لحظه‌ی Order».
 * اگر Account دست‌نخورده باشد، Percent=100 یعنی همه‌ی موجودی.
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
      onChange([...value, { accountId, allocationPercent: 100 }]);
    }
  };

  const updatePercent = (accountId: string, percent: number) => {
    onChange(
      value.map((v) =>
        v.accountId === accountId
          ? { ...v, allocationPercent: percent }
          : v,
      ),
    );
  };

  return (
    <div className="space-y-2">
      <div className="overflow-hidden rounded border border-gray-200 dark:border-gray-700">
        <div className="divide-y divide-gray-200 dark:divide-gray-700">
          {options.map((o) => {
            const selected = getSelected(o.value);
            const invalid =
              selected &&
              (selected.allocationPercent <= 0 ||
                selected.allocationPercent > 100);

            return (
              <div
                key={o.value}
                className={`flex items-center gap-3 p-3 ${
                  selected
                    ? "bg-blue-50 dark:bg-blue-900/20"
                    : "hover:bg-gray-50 dark:hover:bg-gray-800/50"
                }`}
              >
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
                {selected && (
                  <>
                    <input
                      type="number"
                      value={selected.allocationPercent}
                      onChange={(e) =>
                        updatePercent(o.value, Number(e.target.value))
                      }
                      min={0.01}
                      max={100}
                      step={0.01}
                      dir="ltr"
                      className={`w-24 rounded border p-1 text-sm ${
                        invalid
                          ? "border-red-500 bg-red-50 dark:bg-red-900/20"
                          : "border-gray-300 dark:border-gray-600 dark:bg-gray-700"
                      } dark:text-white`}
                    />
                    <span className="text-xs text-gray-500">%</span>
                  </>
                )}
              </div>
            );
          })}
        </div>
      </div>

      {value.length > 0 && (
        <div className="rounded bg-slate-100 p-2 text-[10px] text-slate-600 dark:bg-slate-700 dark:text-slate-300">
          ℹ️ درصد هر حساب نسبت به <strong>BuyPower لحظه‌ی Order</strong> آن
          حساب محاسبه می‌شود. جمع درصدها لازم نیست ۱۰۰ باشد — هر Account
          مستقل است. اگر چند Run روی یک Account باشند، BuyPower بین آن‌ها
          مشترک است و هر Run در لحظه‌ی Order سهم خودش را می‌بیند.
        </div>
      )}
    </div>
  );
};