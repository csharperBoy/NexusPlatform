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
}