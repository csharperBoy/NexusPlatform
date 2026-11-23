using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.ExternalApiClient;
using Trader.ExternalApiClient.EasyTrader;
using Trader.Infrastructure.Brokers.EasyTrader.Internal;

namespace Trader.Infrastructure.Brokers.EasyTrader.Capabilities;

internal class EasyTraderCorporateActionReader
        : EasyTraderCapabilityBase, IBrokerCorporateActionReader
{
    public EasyTraderCorporateActionReader(
        EasyTraderApiClient api,
        ILogger<EasyTraderMarketDataReader> logger)
        : base(api, logger) { }

   
    public async Task<IReadOnlyList<CorporateActionMarkDto>> GetCorporateActionsAsync(
        ExternalSystemSession session,
        string isin,
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
    {
        var fromUnix = new DateTimeOffset(DateTime.SpecifyKind(from, DateTimeKind.Utc)).ToUnixTimeSeconds();
        var toUnix = new DateTimeOffset(DateTime.SpecifyKind(to, DateTimeKind.Utc)).ToUnixTimeSeconds();
        var s = AsSession(session);
        var response = await Api.GetChartMarksAsync(
            accessToken: s.AccessToken,   // ← اگر property اسمش فرق داره، اینجا تنظیم کن
            symbolIsin: isin,
            resolution: "1M",
            fromUnix: fromUnix,
            toUnix: toUnix,
            ct: ct);

        if (response.Time is null || response.Time.Length == 0)
            return Array.Empty<CorporateActionMarkDto>();

        var result = new List<CorporateActionMarkDto>(response.Time.Length);
        for (int i = 0; i < response.Time.Length; i++)
        {
            result.Add(new CorporateActionMarkDto(
                ExDateUtc: DateTimeOffset.FromUnixTimeSeconds(response.Time[i]).UtcDateTime,
                Label: response.Label?[i] ?? "",
                SourceId: response.Id?[i] ?? "",
                RawText: response.Text?[i] ?? "",
                UnixSeconds: response.Time[i]));
        }
        return result;
    }
}