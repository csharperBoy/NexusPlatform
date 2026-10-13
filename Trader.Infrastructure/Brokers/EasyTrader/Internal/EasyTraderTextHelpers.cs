namespace Trader.Infrastructure.Brokers.EasyTrader.Internal
{
    internal static class EasyTraderTextHelpers
    {
        public static string Truncate(string? s, int max)
        {
            if (string.IsNullOrEmpty(s)) return s ?? string.Empty;
            return s.Length <= max ? s : s[..max] + "...";
        }
    }
}