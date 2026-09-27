namespace Trader.Infrastructure.Brokers.EasyTrader
{
    public class EasyTraderOptions
    {
        public const string SectionName = "Brokers:EasyTrader";

        public string BaseUrl { get; set; } = "https://api-mts.orbis.easytrader.ir";
        public string OidcBaseUrl { get; set; } = "https://login.emofid.com";
        public string ClientId { get; set; } = "easy_pkce";
        public string RedirectUri { get; set; } = "https://d.easytrader.ir/auth-callback";
        public string Scope { get; set; } = "easy2_api mts_api openid profile login_delegation-api";
        public int TimeoutSeconds { get; set; } = 30;
        public string UserAgent { get; set; } =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
            "AppleWebKit/537.36 (KHTML, like Gecko) " +
            "Chrome/152.0.0.0 Safari/537.36";
        public string AppBuildNo { get; set; } = "53559";
    }
}