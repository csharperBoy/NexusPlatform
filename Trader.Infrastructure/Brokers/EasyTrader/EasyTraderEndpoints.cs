namespace Trader.Infrastructure.Brokers.EasyTrader
{
    internal static class EasyTraderEndpoints
    {
        public const string OidcAuthorize = "/connect/authorize";
        public const string OidcToken = "/connect/token";

        public const string ServerTime = "/easy/api/account/server-time/{0}";
        public const string SameLogin = "/easy/api/account/same-login";
        public const string Order = "/core/api/v2/order";
        public const string SymbolInfo = "/symbols/api/MarketData/symbol-info-data";
    }
}