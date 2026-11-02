import React, { useMemo } from "react";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from "recharts";
import type { BacktestDailyEquityView } from "../../../models";

interface Series {
  name: string;
  data: BacktestDailyEquityView[];
  color: string;
}

interface Props {
  series: Series[];
}

const COLORS = [
  "#10b981", // emerald
  "#3b82f6", // blue
  "#f59e0b", // amber
  "#ef4444", // red
  "#8b5cf6", // violet
];

export const EquityCurveChart: React.FC<Props> = ({ series }) => {
  // Merge all series into a single array keyed by date
  const mergedData = useMemo(() => {
    const dateMap = new Map<string, Record<string, number | string>>();

    series.forEach((s, idx) => {
      s.data.forEach((point) => {
        const key = point.date.slice(0, 10);
        if (!dateMap.has(key)) {
          dateMap.set(key, { date: key });
        }
        dateMap.get(key)![`series${idx}`] = point.equity;
      });
    });

    return Array.from(dateMap.values()).sort((a, b) =>
      String(a.date).localeCompare(String(b.date)),
    );
  }, [series]);

  if (mergedData.length === 0) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-8 text-center text-sm text-gray-500 dark:border-gray-700 dark:bg-gray-800">
        داده‌ای برای نمودار وجود ندارد.
      </div>
    );
  }

  return (
    <div className="rounded-lg border border-gray-200 bg-white p-4 dark:border-gray-700 dark:bg-gray-800">
      <h3 className="mb-4 text-sm font-bold text-gray-900 dark:text-white">
        منحنی سرمایه (Equity Curve)
      </h3>
      <div className="h-80 w-full" dir="ltr">
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={mergedData}>
            <CartesianGrid strokeDasharray="3 3" opacity={0.2} />
            <XAxis
              dataKey="date"
              tick={{ fontSize: 10 }}
              tickFormatter={(v) => {
                const d = new Date(v);
                return `${d.getMonth() + 1}/${d.getDate()}`;
              }}
            />
            <YAxis
              tick={{ fontSize: 10 }}
              tickFormatter={(v) => {
                if (Math.abs(v) >= 1_000_000) return `${(v / 1_000_000).toFixed(1)}M`;
                if (Math.abs(v) >= 1_000) return `${(v / 1_000).toFixed(0)}K`;
                return String(v);
              }}
            />
            <Tooltip
              formatter={(value: any) =>
                typeof value === "number"
                  ? value.toLocaleString("fa-IR", { maximumFractionDigits: 0 })
                  : value
              }
              labelFormatter={(label) => `تاریخ: ${label}`}
              contentStyle={{
                direction: "rtl",
                fontFamily: "inherit",
                fontSize: 12,
              }}
            />
            <Legend
              formatter={(value) => {
                const idx = parseInt(value.replace("series", ""), 10);
                return series[idx]?.name ?? value;
              }}
            />
            {series.map((s, idx) => (
              <Line
                key={idx}
                type="monotone"
                dataKey={`series${idx}`}
                stroke={s.color ?? COLORS[idx % COLORS.length]}
                strokeWidth={2}
                dot={false}
                connectNulls
              />
            ))}
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};