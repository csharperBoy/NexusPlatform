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
        public const string Money = "/easy/api/money";
        public const string ClientAppSetting = "/easy/api/clientAppSetting";

        /* ═══ Portfolio ═══ */
        public const string Performance = "/assetmodule/api/performance";

        /* ═══ Order ═══ */
        public const string Order = "/core/api/v2/order";

        /* ═══ Symbol ═══ */
        public const string SymbolInfo = "/symbols/api/MarketData/symbol-info-data";
        public const string ReturnChartData = "/symbols/api/MarketData/return-chart-data";
        public const string IndInstTrade = "/symbols/api/MarketData/indInstTrade";
        public const string TradingTime = "/symbols/api/TradingTime";

        /* ═══ Chart ═══ */
        public const string ChartHistory = "/chart/api/v2/datafeed/miniChart/history";

        /* ═══ Analysis ═══ */
        public const string IndInstAnalysis = "/easy/api/symbol-analysis/ind-inst";
        public const string TechnicalAnalysis = "/easy/api/symbol-analysis/technical-analysis";
        public const string IndTradingTrend = "/easy/api/symbol-analysis/ind-trading-trend";
        public const string FundamentalAnalysis = "/easy/api/symbol-analysis/fundamental-analysis";

        /* ═══ Market Sheet ═══ */
        public const string MarketSheetSum = "/ms/api/MarketSheet/sum/{0}";

        /* ═══ Credit ═══ */
        public const string IsCreditCustomer = "/credit/api/isCreditCustomer";

        /* ═══ Market Live ═══ */
        public const string PositiveNegativeSymbols = "/easy/api/market-live/industries/positive-negative-symbols";
        public const string TseIndex = "/symbols/api/MarketData/tse-index";
        public const string MarketWatch = "/easy/api/MarketWatch";


        /* ═══ MarketData (GraphQL batch) ═══ */
        public const string MarketDataGraphQL = "/symbols/api/marketdata";

        /* ═══ Order History ═══ */
        public const string OrderReport = "/easy/api/orderHistory/orderReport";

        /* ═══ Order Trades ═══ */
        public const string OrderTrades = "/easy/api/orderHistory/trades/";

        /* ═══ Finance ═══ */
        public const string Payments = "/finance/api/payments";
        public const string PaymentAccountBalances = "/finance/api/payment-account-balances";
        public const string PaymentCancel = "/finance/api/payments/{0}/cancel";
    }
}