namespace Trader.Infrastructure.Brokers.EasyTrader
{
    internal static class EasyTraderEndpoints
    {    
        /* ═══ OIDC ═══ */
        public const string OidcAuthorize = "/connect/authorize";
        public const string OidcToken = "/connect/token";

        /* ═══ Account ═══ */
        public const string SameLogin = "/easy/api/account/same-login";
        public const string ServerTime = "/easy/api/account/server-time/{0}";

        /* ═══ Order ═══ */
        public const string Order = "/core/api/v2/order";

        /* ═══ Symbol Info ═══ */
        public const string SymbolInfo = "/symbols/api/MarketData/symbol-info-data";
        public const string ReturnChartData = "/symbols/api/MarketData/return-chart-data";
        public const string IndInstTrade = "/symbols/api/MarketData/indInstTrade";

        /* ═══ Chart ═══ */
        public const string ChartHistory = "/chart/api/v2/datafeed/miniChart/history";

        /* ═══ Symbol Analysis ═══ */
        public const string IndInstAnalysis = "/easy/api/symbol-analysis/ind-inst";
        public const string TechnicalAnalysis = "/easy/api/symbol-analysis/technical-analysis";
        public const string IndTradingTrend = "/easy/api/symbol-analysis/ind-trading-trend";

        /* ═══ Market Sheet ═══ */
        public const string MarketSheetSum = "/ms/api/MarketSheet/sum/{0}";

    }
}