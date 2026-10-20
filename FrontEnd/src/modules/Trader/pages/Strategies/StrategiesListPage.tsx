import React, { useEffect, useState } from "react";
import { strategyApi } from "../../api/strategyApi";
import type { StrategyInfoView } from "../../models";

export const StrategiesListPage: React.FC = () => {
  const [strategies, setStrategies] = useState<StrategyInfoView[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    (async () => {
      try {
        setLoading(true);
        const data = await strategyApi.GetList();
        setStrategies(data);
      } catch (e) {
        setError(e instanceof Error ? e.message : "خطا در دریافت استراتژی‌ها");
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  return (
    <div className="space-y-4 p-6">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-900 dark:text-white">
          استراتژی‌های معاملاتی
        </h1>
        <span className="text-xs text-gray-500 dark:text-gray-400">
          {strategies.length} استراتژی ثبت‌شده
        </span>
      </div>

      <div className="rounded-lg border border-slate-200 bg-slate-50 p-3 text-xs text-slate-600 dark:border-slate-700 dark:bg-slate-800/50 dark:text-slate-300">
        ℹ️ استراتژی‌ها در کد ثبت می‌شوند و از این صفحه قابل ایجاد یا حذف نیستند.
      </div>

      {loading ? (
        <div className="p-6 text-center text-sm text-gray-500">
          در حال بارگذاری...
        </div>
      ) : error ? (
        <div className="rounded-lg bg-red-900/20 p-4 text-sm text-red-400">
          {error}
        </div>
      ) : strategies.length === 0 ? (
        <div className="p-6 text-center text-sm text-gray-500">
          هیچ استراتژی‌ای ثبت نشده است.
        </div>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {strategies.map((s) => (
            <div
              key={s.key}
              className="rounded-xl border border-gray-200 bg-white p-4 shadow-sm dark:border-gray-700 dark:bg-gray-800"
            >
              <div className="mb-3 flex items-start justify-between">
                <div>
                  <h2 className="text-lg font-bold text-gray-900 dark:text-white">
                    {s.displayName}
                  </h2>
                  <p className="mt-0.5 font-mono text-xs text-gray-500 dark:text-gray-400">
                    {s.key}
                  </p>
                </div>
                <span className="rounded-full bg-blue-900/40 px-2 py-1 text-xs text-blue-300">
                  {s.configTypeName}
                </span>
              </div>

              <div className="space-y-1.5 border-t border-gray-200 pt-3 dark:border-gray-700">
                <p className="mb-1 text-xs font-semibold text-gray-600 dark:text-gray-400">
                  پارامترهای قابل تنظیم:
                </p>
                {s.configFields.length === 0 ? (
                  <p className="text-xs text-gray-500">بدون پارامتر</p>
                ) : (
                  s.configFields.map((f) => (
                    <div
                      key={f.name}
                      className="flex items-center justify-between text-xs"
                    >
                      <span className="font-mono text-gray-700 dark:text-gray-300">
                        {f.name}
                      </span>
                      <span className="text-gray-500 dark:text-gray-400">
                        {f.typeName}
                        {f.defaultValue != null && (
                          <span className="mr-1 text-gray-400">
                            ({f.defaultValue})
                          </span>
                        )}
                      </span>
                    </div>
                  ))
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};