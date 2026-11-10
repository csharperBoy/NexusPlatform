namespace Trader.ExternalApiClient.Tgju;

public sealed class TgjuOptions
{
    public const string SectionName = "ExternalApis:Tgju";

    public string BaseUrl { get; set; } = "https://api.tgju.org";
    public int TimeoutSeconds { get; set; } = 10;

    public string UserAgent { get; set; } =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
        "AppleWebKit/537.36 (KHTML, like Gecko) " +
        "Chrome/152.0.0.0 Safari/537.36";
}