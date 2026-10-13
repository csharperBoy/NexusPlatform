namespace Trader.Infrastructure.Brokers.EasyTrader.Internal
{
    internal static class EasyTraderDateParsers
    {
        public static DateTimeOffset ParseOrDefault(string? s)
            => DateTimeOffset.TryParse(s, out var d) ? d : default;

        public static DateTimeOffset? ParseOrNull(string? s)
            => DateTimeOffset.TryParse(s, out var d) ? d : null;
    }
}