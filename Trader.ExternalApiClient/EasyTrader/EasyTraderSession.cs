//using Trader.Application.Brokers;

namespace Trader.ExternalApiClient.EasyTrader
{
    /// <summary>Session مخصوص EasyTrader.</summary>
    public record EasyTraderSession(
        string AccessToken,
        DateTimeOffset TokenExp) : ExternalSystemSession
    {
        public override DateTimeOffset? ExpiresAt => TokenExp;
    }
}