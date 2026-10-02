using System.Text.Json.Serialization;
using Trader.Domain.Enums;

namespace Trader.Infrastructure.Brokers.EasyTrader.Internal
{
    /* ═══ OIDC Token Response (snake_case طبق استاندارد OAuth2) ═══ */
    internal class OidcTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = default!;

        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }

    /* ═══ Server Time Response (camelCase) ═══ */
    internal class ServerTimeResponse
    {
        [JsonPropertyName("diff")]
        public long Diff { get; set; }

        [JsonPropertyName("serverTimestamp")]
        public long ServerTimestamp { get; set; }
    }

    /* ═══ Symbol Info Response (camelCase) ═══ */
    internal class SymbolInfoResponse
    {
        [JsonPropertyName("symbolISIN")]
        public string? SymbolISIN { get; set; }

        [JsonPropertyName("highAllowedPrice")]
        public long? HighAllowedPrice { get; set; }

        [JsonPropertyName("lowAllowedPrice")]
        public long? LowAllowedPrice { get; set; }

        [JsonPropertyName("lastTradedPrice")]
        public long? LastTradedPrice { get; set; }

        [JsonPropertyName("closingPrice")]
        public long? ClosingPrice { get; set; }

        [JsonPropertyName("firstTradedPrice")]
        public long? FirstTradedPrice { get; set; }

        [JsonPropertyName("tradeDate")]
        public string? TradeDate { get; set; }
    }

    /* ═══ Order Request (camelCase — این رو ما می‌فرستیم) ═══ */
    internal class EasyTraderOrderRequest
    {
        [JsonPropertyName("order")]
        public EasyTraderOrder Order { get; set; } = new();
    }

    internal class EasyTraderOrder
    {
        [JsonPropertyName("price")]
        public long Price { get; set; }

        [JsonPropertyName("quantity")]
        public long Quantity { get; set; }

        [JsonPropertyName("side")]
        public int Side { get; set; }

        [JsonPropertyName("validityType")]
        public int ValidityType { get; set; }

        [JsonPropertyName("createDateTime")]
        public string CreateDateTime { get; set; } = default!;

        [JsonPropertyName("commission")]
        public decimal Commission { get; set; }

        [JsonPropertyName("symbolIsin")]
        public string SymbolIsin { get; set; } = default!;

        [JsonPropertyName("symbolName")]
        public string SymbolName { get; set; } = default!;

        [JsonPropertyName("orderModelType")]
        public int OrderModelType { get; set; }

        [JsonPropertyName("totalValue")]
        public long TotalValue { get; set; }

        [JsonPropertyName("orderFrom")]
        public int OrderFrom { get; set; }
    }

    /* ═══ Order Response (camelCase) ═══ */
    internal class OrderResponse
    {
        [JsonPropertyName("isSuccessful")]
        public bool IsSuccessful { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("omsError")]
        public List<OmsErrorItem>? OmsError { get; set; }
    }

    internal class OmsErrorItem
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
    /* ═══ Chart ═══ */
    internal class CandleHistoryResponse
    {
        [JsonPropertyName("data")]
        public List<CandleItem> Data { get; set; } = new();

        [JsonPropertyName("s")]
        public string? S { get; set; }
    }

    internal class CandleItem
    {
        [JsonPropertyName("t")]
        public long T { get; set; }

        [JsonPropertyName("o")]
        public decimal O { get; set; }

        [JsonPropertyName("h")]
        public decimal H { get; set; }

        [JsonPropertyName("l")]
        public decimal L { get; set; }

        [JsonPropertyName("c")]
        public decimal C { get; set; }

        [JsonPropertyName("v")]
        public long V { get; set; }
    }

    /* ═══ Return Chart ═══ */
    internal class ReturnChartResponse
    {
        [JsonPropertyName("lastTradedPrice")]
        public long LastTradedPrice { get; set; }

        [JsonPropertyName("e30")]
        public decimal E30 { get; set; }

        [JsonPropertyName("e90")]
        public decimal E90 { get; set; }

        [JsonPropertyName("e360")]
        public decimal E360 { get; set; }

        [JsonPropertyName("maturityDay")]
        public string? MaturityDay { get; set; }

        [JsonPropertyName("daysToMaturity")]
        public int? DaysToMaturity { get; set; }

        [JsonPropertyName("returnToMaturity")]
        public decimal? ReturnToMaturity { get; set; }
    }

    /* ═══ Ind/Inst Trade ═══ */
    internal class IndInstTradeResponse
    {
        [JsonPropertyName("symbolISIN")]
        public string? SymbolISIN { get; set; }

        [JsonPropertyName("indBuyVolume")]
        public string? IndBuyVolume { get; set; }

        [JsonPropertyName("indBuyNumber")]
        public string? IndBuyNumber { get; set; }

        [JsonPropertyName("indSellVolume")]
        public string? IndSellVolume { get; set; }

        [JsonPropertyName("indSellNumber")]
        public string? IndSellNumber { get; set; }

        [JsonPropertyName("insBuyVolume")]
        public string? InsBuyVolume { get; set; }

        [JsonPropertyName("insBuyNumber")]
        public string? InsBuyNumber { get; set; }

        [JsonPropertyName("insSellVolume")]
        public string? InsSellVolume { get; set; }

        [JsonPropertyName("insSellNumber")]
        public string? InsSellNumber { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }
    }

    /* ═══ Ind/Inst Analysis ═══ */
    internal class IndInstAnalysisResponse
    {
        [JsonPropertyName("indBuyVol")]
        public long IndBuyVol { get; set; }

        [JsonPropertyName("indBuyPow")]
        public decimal IndBuyPow { get; set; }

        [JsonPropertyName("indSellVol")]
        public long IndSellVol { get; set; }

        [JsonPropertyName("insSellVol")]
        public long InsSellVol { get; set; }

        [JsonPropertyName("insBuyVol")]
        public long InsBuyVol { get; set; }

        [JsonPropertyName("bidPres")]
        public decimal BidPres { get; set; }

        [JsonPropertyName("netInd")]
        public decimal NetInd { get; set; }

        [JsonPropertyName("buyPerInd")]
        public decimal BuyPerInd { get; set; }

        [JsonPropertyName("sellPerInd")]
        public decimal SellPerInd { get; set; }

        [JsonPropertyName("diffValInd")]
        public decimal DiffValInd { get; set; }
    }

    /* ═══ Technical Analysis ═══ */
    internal class TechnicalAnalysisResponse
    {
        [JsonPropertyName("totalScore")]
        public TechnicalScoreItem? TotalScore { get; set; }

        [JsonPropertyName("categoryScore")]
        public List<TechnicalCategoryScoreItem>? CategoryScore { get; set; }
    }

    internal class TechnicalScoreItem
    {
        [JsonPropertyName("cat")]
        public string? Cat { get; set; }

        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }
    }

    internal class TechnicalCategoryScoreItem
    {
        [JsonPropertyName("cat")]
        public string? Cat { get; set; }

        [JsonPropertyName("catFa")]
        public string? CatFa { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }
    }

    /* ═══ Ind Trading Trend ═══ */
    internal class IndTradingTrendItem
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("val")]
        public decimal Val { get; set; }
    }

    /* ═══ Market Sheet Sum ═══ */
    internal class MarketSheetSumResponse
    {
        [JsonPropertyName("buyVolume")]
        public long BuyVolume { get; set; }

        [JsonPropertyName("buyCount")]
        public long BuyCount { get; set; }

        [JsonPropertyName("sellVolume")]
        public long SellVolume { get; set; }

        [JsonPropertyName("sellCount")]
        public long SellCount { get; set; }
    }
    /* ═══ Money ═══ */
    internal class MoneyResponse
    {
        [JsonPropertyName("t0")] public long T0 { get; set; }
        [JsonPropertyName("t1")] public long T1 { get; set; }
        [JsonPropertyName("t2")] public long T2 { get; set; }
        [JsonPropertyName("buyPowerT0")] public long BuyPowerT0 { get; set; }
        [JsonPropertyName("buyPowerT1")] public long BuyPowerT1 { get; set; }
        [JsonPropertyName("buyPowerT2")] public long BuyPowerT2 { get; set; }
        [JsonPropertyName("blockT2")] public long BlockT2 { get; set; }
        [JsonPropertyName("withdrawBlockT2")] public long WithdrawBlockT2 { get; set; }
        [JsonPropertyName("marginBlock")] public long MarginBlock { get; set; }
        [JsonPropertyName("credit")] public long Credit { get; set; }
        [JsonPropertyName("avandCredit")] public long AvandCredit { get; set; }
        [JsonPropertyName("walletWithdrawBalanceT0")] public long WalletWithdrawBalanceT0 { get; set; }
        [JsonPropertyName("warrantValueCredit")] public long WarrantValueCredit { get; set; }
        [JsonPropertyName("hamiBalance")] public long HamiBalance { get; set; }
        [JsonPropertyName("block")] public long Block { get; set; }
    }

    /* ═══ Portfolio Performance ═══ */
    internal class PerformanceResponse
    {
        [JsonPropertyName("items")]
        public List<PerformanceItem> Items { get; set; } = new();
    }

    internal class PerformanceItem
    {
        [JsonPropertyName("symbolIsin")] public string? SymbolIsin { get; set; }
        [JsonPropertyName("symbolName")] public string? SymbolName { get; set; }
        [JsonPropertyName("date")] public string? Date { get; set; }
        [JsonPropertyName("persianDate")] public long PersianDate { get; set; }
        [JsonPropertyName("asset")] public long Asset { get; set; }
        [JsonPropertyName("bonusShareVolume")] public long BonusShareVolume { get; set; }
        [JsonPropertyName("stockRightVolume")] public long StockRightVolume { get; set; }
        [JsonPropertyName("periodicDPSAmount")] public long PeriodicDPSAmount { get; set; }
        [JsonPropertyName("lastDPSAmount")] public long LastDPSAmount { get; set; }
        [JsonPropertyName("totalDPSAmount")] public long TotalDPSAmount { get; set; }
        [JsonPropertyName("breakevenPoint")] public decimal BreakevenPoint { get; set; }
        [JsonPropertyName("breakevenWithoutDps")] public decimal BreakevenWithoutDps { get; set; }
        [JsonPropertyName("periodicBuyAveragePrice")] public decimal PeriodicBuyAveragePrice { get; set; }
        [JsonPropertyName("periodicBuyAveragePriceWithoutDps")] public decimal PeriodicBuyAveragePriceWithoutDps { get; set; }
        [JsonPropertyName("periodicBuyAmount")] public decimal PeriodicBuyAmount { get; set; }
        [JsonPropertyName("periodicSellAveragePrice")] public decimal PeriodicSellAveragePrice { get; set; }
        [JsonPropertyName("periodicSellAmount")] public decimal PeriodicSellAmount { get; set; }
        [JsonPropertyName("periodicBoughtVolume")] public long PeriodicBoughtVolume { get; set; }
        [JsonPropertyName("periodicSoldVolume")] public long PeriodicSoldVolume { get; set; }
        [JsonPropertyName("periodicRealizedProfitAndLoss")] public decimal PeriodicRealizedProfitAndLoss { get; set; }
        [JsonPropertyName("periodicRealizedProfitAndLossWithoutDps")] public decimal PeriodicRealizedProfitAndLossWithoutDps { get; set; }
        [JsonPropertyName("totalBuyAveragePrice")] public decimal TotalBuyAveragePrice { get; set; }
        [JsonPropertyName("totalBuyAveragePriceWithoutDps")] public decimal TotalBuyAveragePriceWithoutDps { get; set; }
        [JsonPropertyName("totalBuyAmount")] public decimal TotalBuyAmount { get; set; }
        [JsonPropertyName("totalSellAveragePrice")] public decimal TotalSellAveragePrice { get; set; }
        [JsonPropertyName("totalSellAmount")] public decimal TotalSellAmount { get; set; }
        [JsonPropertyName("totalBoughtVolume")] public long TotalBoughtVolume { get; set; }
        [JsonPropertyName("totalSoldVolume")] public long TotalSoldVolume { get; set; }
        [JsonPropertyName("totalRealizedProfitAndLoss")] public decimal TotalRealizedProfitAndLoss { get; set; }
        [JsonPropertyName("totalRealizedProfitAndLossWithoutDps")] public decimal TotalRealizedProfitAndLossWithoutDps { get; set; }
        [JsonPropertyName("dailyBuyAveragePrice")] public decimal DailyBuyAveragePrice { get; set; }
        [JsonPropertyName("dailySellAveragePrice")] public decimal DailySellAveragePrice { get; set; }
        [JsonPropertyName("dailyBoughtVolume")] public long DailyBoughtVolume { get; set; }
        [JsonPropertyName("dailySoldVolume")] public long DailySoldVolume { get; set; }
        [JsonPropertyName("periodicBuyNetPrice")] public decimal PeriodicBuyNetPrice { get; set; }
        [JsonPropertyName("periodicSellNetPrice")] public decimal PeriodicSellNetPrice { get; set; }
        [JsonPropertyName("totalBuyNetPrice")] public decimal TotalBuyNetPrice { get; set; }
        [JsonPropertyName("totalSellNetPrice")] public decimal TotalSellNetPrice { get; set; }
    }

    /* ═══ Trading Time ═══ */
    internal class TradingTimeResponse
    {
        [JsonPropertyName("items")]
        public List<TradingTimeItem> Items { get; set; } = new();
    }

    internal class TradingTimeItem
    {
        [JsonPropertyName("title")] public List<string>? Title { get; set; }
        [JsonPropertyName("days")] public List<int>? Days { get; set; }
        [JsonPropertyName("tradingTimeType")] public int TradingTimeType { get; set; }
        [JsonPropertyName("startPreTradingTime")] public string? StartPreTradingTime { get; set; }
        [JsonPropertyName("endPreTradingTime")] public string? EndPreTradingTime { get; set; }
        [JsonPropertyName("startTradingTime")] public string? StartTradingTime { get; set; }
        [JsonPropertyName("endTradingTime")] public string? EndTradingTime { get; set; }
        [JsonPropertyName("startTalTime")] public string? StartTalTime { get; set; }
        [JsonPropertyName("endTalTime")] public string? EndTalTime { get; set; }
    }

    /* ═══ Client App Setting ═══ */
    internal class ClientAppSettingResponse
    {
        [JsonPropertyName("lightTheme")] public bool LightTheme { get; set; }
        [JsonPropertyName("buyQuantity")] public long BuyQuantity { get; set; }
        [JsonPropertyName("sellQuantity")] public long SellQuantity { get; set; }
        [JsonPropertyName("tick")] public decimal Tick { get; set; }
        [JsonPropertyName("tickType")] public string? TickType { get; set; }
        [JsonPropertyName("priceFromHeadline")] public bool PriceFromHeadline { get; set; }
        [JsonPropertyName("orderConfirmation")] public bool OrderConfirmation { get; set; }
        [JsonPropertyName("divideOrderToMultiple")] public bool DivideOrderToMultiple { get; set; }
        [JsonPropertyName("notchUp")] public bool NotchUp { get; set; }
        [JsonPropertyName("notchDown")] public bool NotchDown { get; set; }
        [JsonPropertyName("pageSize")] public int PageSize { get; set; }
        [JsonPropertyName("applyCommissionInPortfolio")] public bool ApplyCommissionInPortfolio { get; set; }
        [JsonPropertyName("useClosingPriceInPortfolioTotalValue")] public bool UseClosingPriceInPortfolioTotalValue { get; set; }
        [JsonPropertyName("showNotifications")] public bool ShowNotifications { get; set; }
        [JsonPropertyName("dataTracker")] public bool DataTracker { get; set; }
        [JsonPropertyName("usePersianNumber")] public bool UsePersianNumber { get; set; }
        [JsonPropertyName("noSleep")] public bool NoSleep { get; set; }
        [JsonPropertyName("noBalance")] public bool NoBalance { get; set; }
        [JsonPropertyName("userStatusBarToUp")] public bool UserStatusBarToUp { get; set; }
        [JsonPropertyName("portfolioBasedOnLastPositivePeriod")] public bool PortfolioBasedOnLastPositivePeriod { get; set; }
        [JsonPropertyName("portfolioTotalValueCalculateType")] public int PortfolioTotalValueCalculateType { get; set; }
        [JsonPropertyName("keepOrderFormAfterSubmit")] public bool KeepOrderFormAfterSubmit { get; set; }
        [JsonPropertyName("priceFromHeadlineSide")] public int PriceFromHeadlineSide { get; set; }
        [JsonPropertyName("blinkOnDataChange")] public bool BlinkOnDataChange { get; set; }
        [JsonPropertyName("portfolioBuyAveragePriceCalculationType")] public int PortfolioBuyAveragePriceCalculationType { get; set; }
        [JsonPropertyName("portfolioCalculationType")] public int PortfolioCalculationType { get; set; }
        [JsonPropertyName("soldPortfolioBuyAveragePriceCalculationType")] public int SoldPortfolioBuyAveragePriceCalculationType { get; set; }
        [JsonPropertyName("soldPortfolioCalculationType")] public int SoldPortfolioCalculationType { get; set; }
        [JsonPropertyName("volatility")] public decimal Volatility { get; set; }
        [JsonPropertyName("interestRate")] public decimal InterestRate { get; set; }
        [JsonPropertyName("showAsLastPeriodAsset")] public bool ShowAsLastPeriodAsset { get; set; }
    }

    internal class IndustryPositiveNegativeResponse
    {
        [JsonPropertyName("industryCode")] public string? IndustryCode { get; set; }
        [JsonPropertyName("industryName")] public string? IndustryName { get; set; }
        [JsonPropertyName("negativeCount")] public long NegativeCount { get; set; }
        [JsonPropertyName("zeroCount")] public long ZeroCount { get; set; }
        [JsonPropertyName("positiveCount")] public long PositiveCount { get; set; }
    }
    internal class TseIndexResponse
    {
        [JsonPropertyName("dayOfEvent")] public string? DayOfEvent { get; set; }
        [JsonPropertyName("indexChanges")] public long IndexChanges { get; set; }
        [JsonPropertyName("lastIndexValue")] public long LastIndexValue { get; set; }
        [JsonPropertyName("percentVariation")] public double PercentVariation { get; set; }
        [JsonPropertyName("symbolIsin")] public string? SymbolIsin { get; set; }
        [JsonPropertyName("symbolTitle")] public string? SymbolTitle { get; set; }
    }
    internal class MarketWatchResponse
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("order")] public int Order { get; set; }
        [JsonPropertyName("isDefault")] public bool IsDefault { get; set; }
        [JsonPropertyName("customerIsin")] public string? CustomerIsin { get; set; }
        [JsonPropertyName("createDateTime")] public string? CreateDateTime { get; set; }
        [JsonPropertyName("watchCategorySymbols")] public List<WatchSymbol>? WatchCategorySymbols { get; set; }
    }

    internal class WatchSymbol
    {
        [JsonPropertyName("symbolIsin")] public string? SymbolIsin { get; set; }
    }
    internal class MarketDataGraphQLResponse
    {
        [JsonPropertyName("data")] public MarketDataGraphQLData? Data { get; set; }
    }

    internal class MarketDataGraphQLData
    {
        [JsonPropertyName("marketData")] public List<MarketDataGraphQLItem>? MarketData { get; set; }
    }

    internal class MarketDataGraphQLItem
    {
        [JsonPropertyName("symbolIsin")] public string? SymbolIsin { get; set; }
        [JsonPropertyName("stateCode")] public string? StateCode { get; set; }
        [JsonPropertyName("lastTradedPrice")] public long LastTradedPrice { get; set; }
        [JsonPropertyName("closingPrice")] public long ClosingPrice { get; set; }
        [JsonPropertyName("totalTradeValue")] public long TotalTradeValue { get; set; }
        [JsonPropertyName("totalNumberOfTrades")] public long TotalNumberOfTrades { get; set; }
        [JsonPropertyName("feeOfPreviousDaysClosingPrice")] public long FeeOfPreviousDaysClosingPrice { get; set; }
        [JsonPropertyName("priceVar")] public double PriceVar { get; set; }
        [JsonPropertyName("buyRatio")] public string? BuyRatio { get; set; }
        [JsonPropertyName("sellRatio")] public string? SellRatio { get; set; }
        [JsonPropertyName("highPrice")] public long HighPrice { get; set; }
        [JsonPropertyName("lowPrice")] public long LowPrice { get; set; }
        [JsonPropertyName("bestBuyPrice")] public long BestBuyPrice { get; set; }
        [JsonPropertyName("bestBuyQuantity")] public long BestBuyQuantity { get; set; }
        [JsonPropertyName("bestSellPrice")] public long BestSellPrice { get; set; }
        [JsonPropertyName("bestSellQuantity")] public long BestSellQuantity { get; set; }
        [JsonPropertyName("firstTradedPrice")] public long FirstTradedPrice { get; set; }
        [JsonPropertyName("lowAllowedPrice")] public long LowAllowedPrice { get; set; }
        [JsonPropertyName("highAllowedPrice")] public long HighAllowedPrice { get; set; }
        [JsonPropertyName("totalNumberOfSharesTraded")] public long TotalNumberOfSharesTraded { get; set; }
        [JsonPropertyName("minValidBuyVolume")] public long MinValidBuyVolume { get; set; }
        [JsonPropertyName("maxValidBuyVolume")] public long MaxValidBuyVolume { get; set; }
        [JsonPropertyName("minValidSellVolume")] public long MinValidSellVolume { get; set; }
        [JsonPropertyName("maxValidSellVolume")] public long MaxValidSellVolume { get; set; }
        [JsonPropertyName("priceTickSize")] public long PriceTickSize { get; set; }
    }

    /* ═══ Order Report (Sieve paged) ═══ */
    internal class OrderReportResponse
    {
        [JsonPropertyName("records")] public List<OrderReportRecord>? Records { get; set; }
        [JsonPropertyName("totalRecords")] public int TotalRecords { get; set; }
    }

    internal class OrderReportRecord
    {
        [JsonPropertyName("orderReport")] public OrderReportItem? OrderReport { get; set; }
        [JsonPropertyName("orderSources")] public List<OrderSourceItem>? OrderSources { get; set; }
    }

    internal class OrderReportItem
    {
        [JsonPropertyName("createDateTime")] public string? CreateDateTime { get; set; }
        [JsonPropertyName("orderId")] public string? OrderId { get; set; }
        [JsonPropertyName("referenceId")] public string? ReferenceId { get; set; }
        [JsonPropertyName("parentId")] public string? ParentId { get; set; }
        [JsonPropertyName("customerIsin")] public string? CustomerIsin { get; set; }
        [JsonPropertyName("symbolIsin")] public string? SymbolIsin { get; set; }
        [JsonPropertyName("symbolName")] public string? SymbolName { get; set; }
        [JsonPropertyName("correlationId")] public string? CorrelationId { get; set; }
        [JsonPropertyName("validity")] public int Validity { get; set; }
        [JsonPropertyName("validityDate")] public string? ValidityDate { get; set; }
        [JsonPropertyName("price")] public long Price { get; set; }
        [JsonPropertyName("meanPrice")] public long MeanPrice { get; set; }
        [JsonPropertyName("quantity")] public long Quantity { get; set; }
        [JsonPropertyName("orderValue")] public long OrderValue { get; set; }
        [JsonPropertyName("side")] public int Side { get; set; }
        [JsonPropertyName("orderState")] public int OrderState { get; set; }
        [JsonPropertyName("executedQuantity")] public long ExecutedQuantity { get; set; }
        [JsonPropertyName("error")] public string? Error { get; set; }
        [JsonPropertyName("modifyDate")] public string? ModifyDate { get; set; }
        [JsonPropertyName("partition")] public int Partition { get; set; }
        [JsonPropertyName("consumerIndex")] public long ConsumerIndex { get; set; }
        [JsonPropertyName("ignoreCheckingMoney")] public bool IgnoreCheckingMoney { get; set; }
        [JsonPropertyName("traderCredit")] public bool TraderCredit { get; set; }
        [JsonPropertyName("id")] public string? Id { get; set; }
    }

    internal class OrderSourceItem
    {
        [JsonPropertyName("actionType")] public int ActionType { get; set; }
        [JsonPropertyName("createDateTime")] public string? CreateDateTime { get; set; }
        [JsonPropertyName("ip")] public string? Ip { get; set; }
        [JsonPropertyName("orderFrom")] public int OrderFrom { get; set; }
    }

    /* ═══ Sieve Request ═══ */
    internal class SieveRequest
    {
        [JsonPropertyName("page")] public int Page { get; set; }
        [JsonPropertyName("pageSize")] public int PageSize { get; set; }
        [JsonPropertyName("filters")] public List<SieveFilter> Filters { get; set; } = new();
        [JsonPropertyName("sort")] public SieveSort? Sort { get; set; }
    }

    internal class SieveFilter
    {
        [JsonPropertyName("column")] public string Column { get; set; } = "";
        [JsonPropertyName("operator")] public int Operator { get; set; }
        [JsonPropertyName("value")] public object Value { get; set; } = default!;
    }

    internal class SieveSort
    {
        [JsonPropertyName("column")] public string Column { get; set; } = "";
        [JsonPropertyName("dir")] public int Dir { get; set; }
    }

    internal class OrderReportRequest
    {
        [JsonPropertyName("sieveModel")] public SieveRequest SieveModel { get; set; } = new();
        [JsonPropertyName("searchTadbir")] public bool SearchTadbir { get; set; } = true;
    }

    /* ═══ Order State Interpreter ═══ */

    /// <summary>
    /// نگاشت state سفارش کارگزاری EasyTrader.
    ///
    /// ⚠️ این نگاشت بر اساس داده‌های تجربی (فیلترهای UI + نمونه‌های response)
    /// استخراج شده، نه مستندات رسمی. مقادیر ناشناخته عمداً Unknown می‌مونن
    /// تا با مشاهده‌ی بیشتر تکمیل بشن.
    ///
    /// ── دسته‌های UI کارگزاری (از فیلترها) ──
    ///   • درحال انجام  →  { 6, 7, 8, 9, 11, 19, 28, 30 }
    ///   • انجام شده    →  { 14, 15, 20, 35, 36 }
    ///   • خطا          →  { 2, 10, 11, 25 }
    ///   • ویرایش شده   →  { 1, 16, 19 }
    ///   • منقضی شده    →  { 13, 14 }
    ///   • حذف شده      →  { 3, 17, 36 }
    ///
    /// ── stateهای overlap (توی چند دسته) ──
    ///   • 11 → active + error
    ///   • 14 → done + expired
    ///   • 19 → active + modified
    ///   • 36 → done + canceled
    ///
    /// ── stateهای تأییدشده از response sample ──
    ///   • 14 → partial fill (533/1800)
    ///   • 18 → canceled, no fill (0/30000)  ⚠️ توی هیچ فیلتری نیست
    ///   • 20 → full fill (1000/1000)
    /// </summary>
    internal static class EasyTraderOrderStateMap
    {
        /* ═══════════ SETS (طبق فیلترهای UI) ═══════════ */

        private static readonly HashSet<int> ActiveStates = new() { 6, 7, 8, 9, 11, 19, 28, 30 };
        private static readonly HashSet<int> DoneStates = new() { 14, 15, 20, 35, 36 };
        private static readonly HashSet<int> ErrorStates = new() { 2, 10, 11, 25 };
        private static readonly HashSet<int> ModifiedStates = new() { 1, 16, 19 };
        private static readonly HashSet<int> ExpiredStates = new() { 13, 14 };
        private static readonly HashSet<int> CanceledStates = new() { 3, 17, 36 };

        /* ═══════════ PREDICATES ═══════════ */

        /// <summary>سفارش در دسته‌ی «درحال انجام»ه.</summary>
        public static bool IsActive(int state) => ActiveStates.Contains(state);

        /// <summary>سفارش در دسته‌ی «انجام شده»ئه.</summary>
        public static bool IsDone(int state) => DoneStates.Contains(state);

        /// <summary>سفارش در دسته‌ی «خطا»ئه.</summary>
        public static bool IsError(int state) => ErrorStates.Contains(state);

        /// <summary>سفارش در دسته‌ی «ویرایش شده»ئه.</summary>
        public static bool IsModified(int state) => ModifiedStates.Contains(state);

        /// <summary>سفارش در دسته‌ی «منقضی شده»ئه.</summary>
        public static bool IsExpired(int state) => ExpiredStates.Contains(state);

        /// <summary>سفارش در دسته‌ی «حذف شده»ئه.</summary>
        public static bool IsCanceled(int state) => CanceledStates.Contains(state);

        /// <summary>آیا این state اصلاً شناخته‌شده‌ست؟ (طبق فیلتر «بدون خطا»)</summary>
        public static bool IsKnown(int state)
            => IsActive(state) || IsDone(state) || IsError(state)
            || IsModified(state) || IsExpired(state) || IsCanceled(state);

        /* ═══════════ PRIMARY KIND (محافظه‌کارانه) ═══════════ */

        /// <summary>
        /// تفسیر state به یک kind معنادار.
        /// فقط برای stateهایی که با اطمینان می‌شناسیم مقدار برمی‌گردونه؛
        /// بقیه Unknown می‌مونن. برای دسته‌بندی دقیق‌تر از predicateها استفاده کن.
        /// </summary>
        public static OrderStateKind ToKind(int state) => state switch
        {
            // ── خطاها (تأییدشده از فیلتر «خطا») ──
            2 or 10 or 25 => OrderStateKind.Rejected,
            11 => OrderStateKind.Rejected,  // ⚠️ هم توی active هم error

            // ── نیمه‌خورده (response sample: 533/1800) ──
            14 => OrderStateKind.PartiallyExecuted,

            // ── کامل خورده (response sample: 1000/1000) ──
            20 => OrderStateKind.FullyExecuted,

            // ── کنسل (response sample: 0/30000) ──
            // ⚠️ توی فیلتر «حذف شده» نیست — احتمالاً کنسلی از نوع دیگه
            18 => OrderStateKind.Canceled,

            // ── بقیه: بدون مستندات، حدس نمی‌زنیم ──
            _ => OrderStateKind.Unknown,
        };
    }
    internal class OrderTradeItem
    {
        [JsonPropertyName("isr")] public string? Isr { get; set; }
        [JsonPropertyName("tradeNumber")] public long TradeNumber { get; set; }
        [JsonPropertyName("customerIsin")] public string? CustomerIsin { get; set; }
        [JsonPropertyName("quantity")] public long Quantity { get; set; }
        [JsonPropertyName("date")] public string? Date { get; set; }
        [JsonPropertyName("price")] public long Price { get; set; }
        [JsonPropertyName("remain")] public long Remain { get; set; }
        [JsonPropertyName("hasRemain")] public bool HasRemain { get; set; }
        [JsonPropertyName("isin")] public string? Isin { get; set; }
        [JsonPropertyName("side")] public int Side { get; set; }
        [JsonPropertyName("hon")] public long Hon { get; set; }
        [JsonPropertyName("partition")] public int Partition { get; set; }
        [JsonPropertyName("consumerIndex")] public long ConsumerIndex { get; set; }
        [JsonPropertyName("origin")] public int Origin { get; set; }
        [JsonPropertyName("cancelDateTime")] public string? CancelDateTime { get; set; }
        [JsonPropertyName("isCanceled")] public bool? IsCanceled { get; set; }
        [JsonPropertyName("requestId")] public long RequestId { get; set; }
        [JsonPropertyName("createDateTime")] public string? CreateDateTime { get; set; }
        [JsonPropertyName("id")] public string? Id { get; set; }
    }
    /* ═══ Payment Account Balances ═══ */
    internal class PaymentAccountBalancesResponse
    {
        [JsonPropertyName("totalBalance")] public long TotalBalance { get; set; }
        [JsonPropertyName("isCustomerConstraintRestricted")] public bool IsCustomerConstraintRestricted { get; set; }
        [JsonPropertyName("bankAccounts")] public List<BankAccountItem>? BankAccounts { get; set; }
        [JsonPropertyName("accountBalancePerDate")] public List<AccountBalancePerDateItem>? AccountBalancePerDate { get; set; }
    }

    internal class BankAccountItem
    {
        [JsonPropertyName("id")] public long Id { get; set; }
        [JsonPropertyName("accountNumber")] public string? AccountNumber { get; set; }
        [JsonPropertyName("shebaNumber")] public string? ShebaNumber { get; set; }
        [JsonPropertyName("cardNumber")] public string? CardNumber { get; set; }
        [JsonPropertyName("bankName")] public string? BankName { get; set; }
        [JsonPropertyName("bankCode")] public string? BankCode { get; set; }
        [JsonPropertyName("bankTitle")] public string? BankTitle { get; set; }
        [JsonPropertyName("isActive")] public bool IsActive { get; set; }
        [JsonPropertyName("isPending")] public bool? IsPending { get; set; }
        [JsonPropertyName("shebaInquiry")] public bool? ShebaInquiry { get; set; }
        [JsonPropertyName("status")] public string? Status { get; set; }
    }

    internal class AccountBalancePerDateItem
    {
        [JsonPropertyName("effectiveDate")] public int EffectiveDate { get; set; }
        [JsonPropertyName("performDate")] public string? PerformDate { get; set; }
        [JsonPropertyName("availableBalance")] public long AvailableBalance { get; set; }
        [JsonPropertyName("maxSingleRequestAmount")] public long MaxSingleRequestAmount { get; set; }
        [JsonPropertyName("maxTotalRequestAmount")] public long MaxTotalRequestAmount { get; set; }
        [JsonPropertyName("maxTotalRequestCount")] public int? MaxTotalRequestCount { get; set; }
        [JsonPropertyName("hasImeWallet")] public bool HasImeWallet { get; set; }
        [JsonPropertyName("accountBalancePerBank")] public List<AccountBalancePerBankItem>? AccountBalancePerBank { get; set; }
    }

    internal class AccountBalancePerBankItem
    {
        [JsonPropertyName("bankAccountId")] public long BankAccountId { get; set; }
        [JsonPropertyName("isBankAvailable")] public bool IsBankAvailable { get; set; }
        [JsonPropertyName("isRequestConstraintRestricted")] public bool IsRequestConstraintRestricted { get; set; }
        [JsonPropertyName("requestRestrictionDetail")] public string? RequestRestrictionDetail { get; set; }
        [JsonPropertyName("singleRequestAmount")] public long SingleRequestAmount { get; set; }
        [JsonPropertyName("totalRequestAmount")] public long TotalRequestAmount { get; set; }
        [JsonPropertyName("totalRequestCount")] public int? TotalRequestCount { get; set; }
    }

    /* ═══ Payments (Withdrawal Request) ═══ */
    internal class WithdrawalRequestPayload
    {
        [JsonPropertyName("bankAccountId")] public long BankAccountId { get; set; }
        [JsonPropertyName("iban")] public string Iban { get; set; } = "";
        [JsonPropertyName("amount")] public long Amount { get; set; }
        [JsonPropertyName("performDate")] public string PerformDate { get; set; } = "";
        [JsonPropertyName("isImeRequest")] public bool IsImeRequest { get; set; }
    }
}