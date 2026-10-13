using System.Globalization;

namespace Trader.Infrastructure.Brokers.EasyTrader.Internal
{
    internal static class EasyTraderNumericParsers
    {
        public static long ParseLong(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            if (long.TryParse(value, out var l)) return l;
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                return (long)Math.Round(d);
            return 0;
        }

        public static decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0m;
            if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                return d;
            return 0m;
        }
    }
}