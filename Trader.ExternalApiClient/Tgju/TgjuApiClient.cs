using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Trader.ExternalApiClient.Tgju;

/// <summary>
/// کلاینت HTTP خام TGJU. هیچ منطق دامنه‌ای اینجا نیست —
/// فقط ساخت URL + GET + Deserialize.
///
/// منطق pagination/boundary/sanity در Trader.Infrastructure (TgjuPriceSourceProvider).
/// </summary>
public sealed class TgjuApiClient
{
    private const string HttpClientName = "Tgju";

    private readonly IHttpClientFactory _httpFactory;
    private readonly TgjuOptions _options;
    private readonly ILogger<TgjuApiClient> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public TgjuApiClient(
        IHttpClientFactory httpFactory,
        IOptions<TgjuOptions> options,
        ILogger<TgjuApiClient> logger)
    {
        _httpFactory = httpFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// یک صفحه از Summary Table را برمی‌گرداند — خام.
    /// </summary>
    public async Task<TgjuDataTablesResponse> FetchPageAsync(
        string indicator,
        int start,
        int length,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(indicator))
            throw new ArgumentException("indicator is required.", nameof(indicator));
        if (start < 0)
            throw new ArgumentOutOfRangeException(nameof(start));
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        var path = string.Format(TgjuEndpoints.SummaryTableData, indicator);
        var query = BuildDataTablesQuery(start, length);
        var url = path + "?" + query;

        var client = _httpFactory.CreateClient(HttpClientName);
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Accept.ParseAdd("application/json, text/javascript, */*; q=0.01");
        req.Headers.TryAddWithoutValidation("Accept-Language", "fa");
        req.Headers.TryAddWithoutValidation("Referer", "https://www.tgju.org/");
        req.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");

        using var res = await client.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            throw new TgjuException(
                $"TGJU fetch failed {(int)res.StatusCode} for indicator={indicator}. " +
                $"Body: {Truncate(body, 300)}",
                (int)res.StatusCode, body);

        if (string.IsNullOrWhiteSpace(body))
            throw new TgjuException(
                $"TGJU returned empty body for indicator={indicator}.",
                (int)res.StatusCode);

        try
        {
            var wire = JsonSerializer.Deserialize<TgjuDataTablesResponse>(body, JsonOpts);
            if (wire is null)
                throw new TgjuException(
                    $"TGJU deserialization returned null for indicator={indicator}.");
            return wire;
        }
        catch (JsonException jex)
        {
            throw new TgjuException(
                $"TGJU JSON parse failed for indicator={indicator}. " +
                $"Preview: {Truncate(body, 300)}. Inner: {jex.Message}",
                (int)res.StatusCode, body);
        }
    }

    /* ═══════════════════ Helpers ═══════════════════ */

    /// <summary>DataTables boilerplate — همان که مرورگر می‌فرستد.</summary>
    private static string BuildDataTablesQuery(int start, int length)
    {
        var qs = new Dictionary<string, string>
        {
            ["lang"] = "fa",
            ["order_dir"] = "asc",
            ["draw"] = "1",
            ["start"] = start.ToString(CultureInfo.InvariantCulture),
            ["length"] = length.ToString(CultureInfo.InvariantCulture),
            ["search"] = "",
            ["order_col"] = "",
            ["convert_to_ad"] = "1",
            ["from"] = "",
            ["to"] = "",
            ["_"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                .ToString(CultureInfo.InvariantCulture),
        };

        for (int i = 0; i <= 7; i++)
        {
            qs[$"columns[{i}][data]"] = i.ToString(CultureInfo.InvariantCulture);
            qs[$"columns[{i}][name]"] = "";
            qs[$"columns[{i}][searchable]"] = "true";
            qs[$"columns[{i}][orderable]"] = "true";
            qs[$"columns[{i}][search][value]"] = "";
            qs[$"columns[{i}][search][regex]"] = "false";
        }

        return string.Join("&", qs.Select(kv =>
            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
    }

    private static string Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return s ?? string.Empty;
        return s.Length <= max ? s : s[..max] + "...";
    }
}