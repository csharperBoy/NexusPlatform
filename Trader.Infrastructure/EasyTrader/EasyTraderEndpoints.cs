namespace Trader.Infrastructure.EasyTrader
{
    /// <summary>
    /// مسیرهای ثابت کارگزاری. Pathها relative به BaseUrl هستن.
    /// </summary>
    internal static class EasyTraderEndpoints
    {
        /* ─── OIDC (relative به OidcBaseUrl) ─── */
        public const string OidcAuthorize = "/connect/authorize";
        public const string OidcToken = "/connect/token";

        /* ─── API (relative به BaseUrl) ─── */
        public const string ServerTime = "/easy/api/account/server-time/{0}";
        public const string SameLogin = "/easy/api/account/same-login";
        public const string Order = "/core/api/v2/order";
        public const string SymbolInfo = "/symbols/api/MarketData/symbol-info-data";
    }
}