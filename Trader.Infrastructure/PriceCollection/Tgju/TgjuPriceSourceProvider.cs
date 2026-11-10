using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions.PriceCollection;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.ExternalApiClient.Tgju;

namespace Trader.Infrastructure.PriceCollection.Tgju;

/// <summary>
/// Provider برای TGJU — منطق pagination + boundary + parse.
/// فقط TgjuApiClient (ExternalApiClient) را صدا می‌زند برای HTTP.
/// </summary>
public sealed class TgjuPriceSourceProvider : IPriceSourceProvider
{
    private const int PageSize = 500;

    private readonly TgjuApiClient _client;
    private readonly PriceCollectionOptions _opts;
    private readonly ILogger<TgjuPriceSourceProvider> _logger;

    private static readonly TimeZoneInfo TehranTz =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

    public TgjuPriceSourceProvider(
        TgjuApiClient client,
        IOptions<PriceCollectionOptions> opts,
        ILogger<TgjuPriceSourceProvider> logger)
    {
        _client = client;
        _opts = opts.Value;
        _logger = logger;
    }

    public PriceSourceType SourceType => PriceSourceType.Tgju;

    public async Task<PriceFetchResult> FetchAsync(
        PriceSource source,
        DateTime? sinceEffectiveAt,
        int maxPages,
        CancellationToken ct = default)
    {
        var config = ParseConfig(source.ConfigJson);
        var allPoints = new List<PricePoint>();
        var pagesFetched = 0;
        var hitBoundary = false;
        var start = 0;

        while (pagesFetched < maxPages)
        {
            ct.ThrowIfCancellationRequested();

            var wire = await _client.FetchPageAsync(
                config.Indicator, start, PageSize, ct);
            pagesFetched++;

            if (wire.Data.Count == 0) break;

            foreach (var cells in wire.Data)
            {
                var parsed = ParseRow(cells);
                if (parsed is null) continue; // malformed — skip silently

                if (sinceEffectiveAt.HasValue
                    && parsed.Value.EffectiveAt <= sinceEffectiveAt.Value)
                {
                    hitBoundary = true;
                    break;
                }

                allPoints.Add(new PricePoint(
                    Rate: parsed.Value.Close,
                    EffectiveAt: parsed.Value.EffectiveAt,
                    Source: source.Code));
            }

            if (hitBoundary) break;
            if (wire.Data.Count < PageSize) break;

            start += PageSize;

            if (_opts.RateLimitMsBetweenPages > 0)
                await Task.Delay(_opts.RateLimitMsBetweenPages, ct);
        }

        _logger.LogDebug(
            "Tgju fetch complete: source={Code} pages={Pages} points={Count} hitBoundary={Hit}",
            source.Code, pagesFetched, allPoints.Count, hitBoundary);

        return new PriceFetchResult(allPoints, pagesFetched, hitBoundary);
    }

    /* ═══════════════════ Parse ═══════════════════ */

    private static (decimal Close, DateTime EffectiveAt)? ParseRow(string[] cells)
    {
        if (cells.Length < 7) return null;

        // [0] = Close
        if (!decimal.TryParse(
                cells[0].Replace(",", "").Trim(),
                NumberStyles.Number, CultureInfo.InvariantCulture, out var close))
            return null;

        // [6] = تاریخ میلادی yyyy/MM/dd
        if (!DateOnly.TryParseExact(
                cells[6].Trim(), "yyyy/MM/dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return null;

        // EffectiveAt = 00:00 Asia/Tehran → UTC
        var tehranMidnight = DateTime.SpecifyKind(
            date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(tehranMidnight, TehranTz);

        return (close, utc);
    }

    private static TgjuConfigModel ParseConfig(string configJson)
    {
        try
        {
            var cfg = JsonSerializer.Deserialize<TgjuConfigModel>(
                configJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (cfg is null || string.IsNullOrWhiteSpace(cfg.Indicator))
                throw new InvalidOperationException(
                    "Tgju config: 'indicator' is required.");

            return cfg;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Tgju config JSON invalid: {ex.Message}", ex);
        }
    }
}