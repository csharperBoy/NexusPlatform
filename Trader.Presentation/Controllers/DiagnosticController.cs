using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using Trader.Application.Abstractions.Brokers;
using Trader.ExternalApiClient;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class DiagnosticController : ControllerBase
{
    private readonly IBrokerSessionProvider _sessionProvider;
    private readonly IConfiguration _config;

    public DiagnosticController(
        IBrokerSessionProvider sessionProvider,
        IConfiguration config)
    {
        _sessionProvider = sessionProvider;
        _config = config;
    }

    [HttpGet("TestChart/{accountId:guid}")]
    public async Task<IActionResult> TestChart(
        Guid accountId,
        [FromQuery] string isin = "IRT1AHRM0001",
        [FromQuery] int intervalMinutes = 1,
        [FromQuery] int days = 2,
        [FromQuery] bool forceHttp11 = false,
        [FromQuery] bool browserUa = false,
        [FromQuery] bool addCacheHeaders = false,
        [FromQuery] bool noRedirect = true)
    {
        var session = await _sessionProvider.GetSessionAsync(accountId);
        var token = ExtractToken(session);
        if (string.IsNullOrEmpty(token))
            return BadRequest("Cannot extract AccessToken from session.");

        var baseUrl = _config["Brokers:EasyTrader:BaseUrl"]
            ?? "https://api-mts.orbis.easytrader.ir";

        // ساخت URL دقیقاً مثل کد اصلی
        var url = baseUrl.TrimEnd('/')
            + "/chart/api/v2/datafeed/miniChart/history"
            + $"?symbol={Uri.EscapeDataString(isin)}:{intervalMinutes}&days={days}";

        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromSeconds(20),
            AllowAutoRedirect = !noRedirect,
            AutomaticDecompression = DecompressionMethods.All,
        };

        using var http = new HttpClient(handler);
        http.Timeout = TimeSpan.FromSeconds(15);

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Headers.Accept.ParseAdd("application/json, text/plain, */*");
        req.Headers.TryAddWithoutValidation("Accept-Language", "fa");
        req.Headers.TryAddWithoutValidation("Referer", "https://d.easytrader.ir/");
        req.Headers.TryAddWithoutValidation("Origin", "https://d.easytrader.ir");

        if (addCacheHeaders)
        {
            req.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
            req.Headers.TryAddWithoutValidation("Pragma", "no-cache");
        }

        if (browserUa)
        {
            req.Headers.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }
        else
        {
            var ua = _config["Brokers:EasyTrader:UserAgent"]
                ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
            req.Headers.TryAddWithoutValidation("User-Agent", ua);
        }

        if (forceHttp11)
        {
            req.Version = HttpVersion.Version11;
            req.VersionPolicy = HttpVersionPolicy.RequestVersionExact;
        }
        else
        {
            req.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        }

        var sw = Stopwatch.StartNew();
        try
        {
            var res = await http.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();
            sw.Stop();

            var trimmed = body.TrimStart();
            var isHtml = trimmed.StartsWith("<");
            var isJson = trimmed.StartsWith("{") || trimmed.StartsWith("[");

            return Ok(new
            {
                input = new
                {
                    accountId,
                    isin,
                    intervalMinutes,
                    days,
                    forceHttp11,
                    browserUa,
                    addCacheHeaders,
                    noRedirect
                },
                url,
                elapsedMs = sw.ElapsedMilliseconds,

                statusCode = (int)res.StatusCode,
                reasonPhrase = res.ReasonPhrase,
                httpVersion = res.Version.ToString(),

                isRedirect = (int)res.StatusCode is >= 300 and < 400,
                location = res.Headers.Location?.ToString(),

                contentType = res.Content.Headers.ContentType?.MediaType,
                contentLength = body.Length,

                isHtml,
                isJson,

                bodyPreview = body.Length > 500 ? body.Substring(0, 500) : body,
            });
        }
        catch (Exception ex)
        {
            sw.Stop();
            return Ok(new
            {
                url,
                elapsedMs = sw.ElapsedMilliseconds,
                exception = ex.GetType().FullName + ": " + ex.Message,
                innerException = ex.InnerException?.Message,
            });
        }
    }

    private static string? ExtractToken(ExternalSystemSession session)
    {
        var prop = session.GetType().GetProperty("AccessToken")
                   ?? session.GetType().GetProperty("Token");
        return prop?.GetValue(session) as string;
    }
}