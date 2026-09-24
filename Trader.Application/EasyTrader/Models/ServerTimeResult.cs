namespace Trader.Application.EasyTrader.Models
{
    public record ServerTimeResult(
        long ClientTimestamp,
        long ServerTimestamp,
        long Diff,
        long Rtt);
}