namespace Trader.Infrastructure.Brokers.EasyTrader.Internal
{
    /* ═══ OIDC Token Response ═══ */
    internal class OidcTokenResponse
    {
        public string AccessToken { get; set; } = default!;
        public string? IdToken { get; set; }
        public int ExpiresIn { get; set; }
        public string? Scope { get; set; }
    }

    /* ═══ Server Time Response ═══ */
    internal class ServerTimeResponse
    {
        public long Diff { get; set; }
        public long ServerTimestamp { get; set; }
    }

    /* ═══ Symbol Info Response ═══ */
    internal class SymbolInfoResponse
    {
        public string? SymbolISIN { get; set; }
        public long? HighAllowedPrice { get; set; }
        public long? LowAllowedPrice { get; set; }
        public long? LastTradedPrice { get; set; }
        public long? ClosingPrice { get; set; }
        public long? FirstTradedPrice { get; set; }
        public string? TradeDate { get; set; }
    }

    /* ═══ Order Request ═══ */
    internal class EasyTraderOrderRequest
    {
        public EasyTraderOrder Order { get; set; } = new();
    }

    internal class EasyTraderOrder
    {
        public long Price { get; set; }
        public long Quantity { get; set; }
        public int Side { get; set; }
        public int ValidityType { get; set; }
        public string CreateDateTime { get; set; } = default!;
        public decimal Commission { get; set; }
        public string SymbolIsin { get; set; } = default!;
        public string SymbolName { get; set; } = default!;
        public int OrderModelType { get; set; }
        public long TotalValue { get; set; }
        public int OrderFrom { get; set; }
    }

    /* ═══ Order Response ═══ */
    internal class OrderResponse
    {
        public bool IsSuccessful { get; set; }
        public string? Id { get; set; }
        public string? Message { get; set; }
        public List<OmsErrorItem>? OmsError { get; set; }
    }

    internal class OmsErrorItem
    {
        public int Code { get; set; }
        public string? Name { get; set; }
        public string? Error { get; set; }
    }
}