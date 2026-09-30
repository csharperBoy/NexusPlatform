using System.Text.Json.Serialization;

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
}