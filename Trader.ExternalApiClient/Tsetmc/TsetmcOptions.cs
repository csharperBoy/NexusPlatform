namespace Trader.ExternalApiClient.Tsetmc;

public sealed class TsetmcOptions
{
    public const string SectionName = "ExternalApis:Tsetmc";

    public string BaseUrl { get; set; } = "https://cdn.tsetmc.com";
    public int TimeoutSeconds { get; set; } = 15;

    public string Referer { get; set; } = "https://www.tsetmc.com/";
    public string AcceptLanguage { get; set; } = "en-US,en;q=0.9,fa;q=0.8,ar;q=0.7";

    public string UserAgent { get; set; } =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
        "AppleWebKit/537.36 (KHTML, like Gecko) " +
        "Chrome/152.0.0.0 Safari/537.36";
}