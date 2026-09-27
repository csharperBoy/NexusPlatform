namespace Trader.Infrastructure.Brokers.EasyTrader.Internal
{
    /// <summary>
    /// استثنای سفارش که timings رو حمل می‌کنه تا در PlanExecutor قابل استفاده باشه.
    /// </summary>
    internal class EasyTraderOrderException : EasyTraderException
    {
        public long FireAtUnixMs { get; }
        public long ReceivedAtUnixMs { get; }

        public EasyTraderOrderException(
            string message,
            long fireAtUnixMs,
            long receivedAtUnixMs,
            int? httpStatus = null,
            string? responseBody = null)
            : base(message, httpStatus, responseBody)
        {
            FireAtUnixMs = fireAtUnixMs;
            ReceivedAtUnixMs = receivedAtUnixMs;
        }
    }
}