// Trader.Application/Strategies/Tuning/TuningWindowBuilder.cs
using Trader.Domain.Enums;

namespace Trader.Application.Strategies.Tuning;

public sealed record TuningWindow(
    TuningScale Scale, DateTime From, DateTime To);

public static class TuningWindowBuilder
{
    private const int YearlyDays = 365;
    private const int SemiDays = 182;
    private const int QuarterDays = 91;
    private const int MonthlyDays = 30;
    private const int MonthlySampleSize = 24;

    public static IReadOnlyDictionary<TuningScale, IReadOnlyList<TuningWindow>> BuildAll(
        DateTime dateFrom, DateTime dateTo, TuningScale selected, Guid seed)
    {
        var result = new Dictionary<TuningScale, IReadOnlyList<TuningWindow>>();

        if (selected.HasFlag(TuningScale.Yearly))
            result[TuningScale.Yearly] = BuildScale(dateFrom, dateTo, TuningScale.Yearly, YearlyDays);

        if (selected.HasFlag(TuningScale.SemiAnnual))
            result[TuningScale.SemiAnnual] = BuildScale(dateFrom, dateTo, TuningScale.SemiAnnual, SemiDays);

        if (selected.HasFlag(TuningScale.Quarterly))
            result[TuningScale.Quarterly] = BuildScale(dateFrom, dateTo, TuningScale.Quarterly, QuarterDays);

        if (selected.HasFlag(TuningScale.Monthly))
        {
            var all = BuildScale(dateFrom, dateTo, TuningScale.Monthly, MonthlyDays);
            if (all.Count > MonthlySampleSize)
            {
                var rnd = new Random(seed.GetHashCode());
                all = all.OrderBy(_ => rnd.Next()).Take(MonthlySampleSize)
                         .OrderBy(w => w.From).ToList();
            }
            result[TuningScale.Monthly] = all;
        }

        return result;
    }

    private static IReadOnlyList<TuningWindow> BuildScale(
        DateTime dateFrom, DateTime dateTo, TuningScale scale, int days)
    {
        var list = new List<TuningWindow>();
        var cur = dateFrom;
        while (true)
        {
            var end = cur.AddDays(days);
            if (end > dateTo)
            {
                // Only include partial last window if it's >= 70% of scale
                var remaining = (dateTo - cur).TotalDays;
                if (remaining >= days * 0.7) list.Add(new TuningWindow(scale, cur, dateTo));
                break;
            }
            list.Add(new TuningWindow(scale, cur, end));
            cur = end;
        }
        return list;
    }
}