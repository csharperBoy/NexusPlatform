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
        var rawPoints = new List<PricePoint>();
        var pagesFetched = 0;
        var start = 0;

        _logger.LogInformation(
            "Tgju fetch start: source={Code} sinceEffectiveAt={Since} maxPages={MaxPages}",
            source.Code, sinceEffectiveAt, maxPages);

        while (pagesFetched < maxPages)
        {
            ct.ThrowIfCancellationRequested();

            var wire = await _client.FetchPageAsync(
                config.Indicator, start, PageSize, ct);
            pagesFetched++;

            _logger.LogInformation(
                "Tgju page {Page}: fetched={Count} recordsTotal={Total} start={Start}",
                pagesFetched, wire.Data.Count, wire.RecordsTotal, start);

            if (wire.Data.Count == 0)
            {
                _logger.LogWarning(
                    "Tgju page {Page} returned empty data array.", pagesFetched);
                break;
            }

            var parsedOk = 0;
            var parsedFail = 0;

            foreach (var cells in wire.Data)
            {
                var parsed = ParseRow(cells);
                if (parsed is null)
                {
                    parsedFail++;
                    // لاگ ۳ نمونه اول برای دیباگ
                    if (parsedFail <= 3)
                    {
                        _logger.LogWarning(
                            "ParseRow failed: length={Len} c0='{C0}' c6='{C6}'",
                            cells.Length,
                            cells.Length > 0 ? cells[0] : "<n/a>",
                            cells.Length > 6 ? cells[6] : "<n/a>");
                    }
                    continue;
                }

                parsedOk++;
                rawPoints.Add(new PricePoint(
                    Rate: parsed.Value.Close,
                    EffectiveAt: parsed.Value.EffectiveAt,
                    Source: source.Code));
            }

            _logger.LogInformation(
                "Tgju page {Page} parsed: ok={Ok} fail={Fail} rawSoFar={Total}",
                pagesFetched, parsedOk, parsedFail, rawPoints.Count);

            if (wire.Data.Count < PageSize) break;

            start += PageSize;

            if (_opts.RateLimitMsBetweenPages > 0)
                await Task.Delay(_opts.RateLimitMsBetweenPages, ct);
        }

        // ─── Sort by EffectiveAt descending (newest first), regardless of API order ───
        rawPoints.Sort((a, b) => b.EffectiveAt.CompareTo(a.EffectiveAt));

        // ─── Filter: keep only STRICTLY NEWER than sinceEffectiveAt ───
        IReadOnlyList<PricePoint> finalPoints;
        bool hitBoundary;

        if (sinceEffectiveAt.HasValue)
        {
            finalPoints = rawPoints
                .Where(p => p.EffectiveAt > sinceEffectiveAt.Value)
                .ToList();
            hitBoundary = finalPoints.Count < rawPoints.Count;
        }
        else
        {
            finalPoints = rawPoints;
            hitBoundary = false;
        }

        _logger.LogInformation(
            "Tgju fetch done: source={Code} pages={Pages} raw={Raw} final={Final} " +
            "hitBoundary={Hit} sinceEffectiveAt={Since}",
            source.Code, pagesFetched, rawPoints.Count, finalPoints.Count,
            hitBoundary, sinceEffectiveAt);

        return new PriceFetchResult(finalPoints, pagesFetched, hitBoundary);
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