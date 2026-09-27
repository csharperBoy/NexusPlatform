namespace Trader.Application.Brokers
{
    /// <summary>
    /// Session خروجی از لاگین هر broker.
    /// abstract — هر broker شکل خودش رو داره.
    /// </summary>
    public abstract record BrokerSession
    {
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
        public abstract DateTimeOffset? ExpiresAt { get; }
    }
}