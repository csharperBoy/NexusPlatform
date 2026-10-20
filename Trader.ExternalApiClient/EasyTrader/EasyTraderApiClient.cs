using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Trader.ExternalApiClient.EasyTrader
{
    /* ══════════════════════════════════════════════════
       API CLIENT — یک متد، یک API
       ══════════════════════════════════════════════════ */

    public class EasyTraderApiClient
    {
        private const string HttpClientName = "EasyTrader";

        private readonly EasyTraderOptions _options;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<EasyTraderApiClient> _logger;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public EasyTraderApiClient(
            IOptions<EasyTraderOptions> options,
            IHttpClientFactory httpFactory,
            ILogger<EasyTraderApiClient> logger)
        {
            _options = options.Value;
            _httpFactory = httpFactory;
            _logger = logger;
        }

        /* ══════════════════════════════════════════════════
           LOGIN FLOW — هر متد معادل یک API
           ══════════════════════════════════════════════════ */

        /// <summary>
        /// ساخت یک Login Session مشترک.
        /// Broker این رو یه بار می‌گیره و به همه‌ی متدهاش پاس می‌ده.
        /// </summary>
        public LoginHttpSession CreateLoginSession()
        {
            var cookieJar = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = cookieJar,
                AllowAutoRedirect = false,
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.All,
            };
            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds),
            };
            if (!string.IsNullOrWhiteSpace(_options.UserAgent))
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);

            return new LoginHttpSession(client);
        }

        /// <summary>GET /connect/authorize</summary>
        public async Task<RedirectResult> AuthorizeAsync(
            LoginHttpSession session,
            string codeChallenge,
            string state,
            CancellationToken ct = default)
        {
            var url = BuildAuthorizeUrl(codeChallenge, state);
            using var res = await session.Client.GetAsync(url, ct);
            return await ToRedirectResultAsync(res, ct);
        }

        /// <summary>GET /Login</summary>
        public async Task<LoginPageResult> GetLoginPageAsync(
            LoginHttpSession session,
            string locationPath,
            CancellationToken ct = default)
        {
            var loginUrl = MakeAbsolute(_options.OidcBaseUrl, locationPath);

            using var res = await session.Client.GetAsync(loginUrl, ct);
            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Login page returned {(int)res.StatusCode}",
                    (int)res.StatusCode);

            var html = await res.Content.ReadAsStringAsync(ct);
            return new LoginPageResult(loginUrl, html);
        }

        /// <summary>POST /Login</summary>
        public async Task<RedirectResult> PostLoginAsync(
            LoginHttpSession session,
            string loginUrl,
            string username,
            string password,
            string antiForgeryToken,
            CancellationToken ct = default)
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Username"] = username,
                ["Password"] = password,
                ["__RequestVerificationToken"] = antiForgeryToken,
            });

            using var res = await session.Client.PostAsync(loginUrl, content, ct);
            return await ToRedirectResultAsync(res, ct);
        }

        /// <summary>GET /connect/authorize/callback</summary>
        public async Task<OidcCallbackResult> CallbackAsync(
            LoginHttpSession session,
            string locationPath,
            CancellationToken ct = default)
        {
            var callbackUrl = MakeAbsolute(_options.OidcBaseUrl, locationPath);

            using var res = await session.Client.GetAsync(callbackUrl, ct);

            var statusCode = (int)res.StatusCode;
            var location = res.Headers.Location?.ToString();

            string? code = null;
            string? state = null;

            if (!string.IsNullOrEmpty(location))
            {
                var queryIdx = location.IndexOf('?');
                var query = queryIdx >= 0 ? location[(queryIdx + 1)..] : string.Empty;
                var q = ParseQueryString(query);
                q.TryGetValue("code", out code);
                q.TryGetValue("state", out state);
            }

            return new OidcCallbackResult(statusCode, location, code, state);
        }

        /// <summary>POST /connect/token</summary>
        public async Task<OidcTokenResponse> ExchangeTokenAsync(
            LoginHttpSession session,
            string code,
            string codeVerifier,
            CancellationToken ct = default)
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = _options.RedirectUri,
                ["code"] = code,
                ["code_verifier"] = codeVerifier,
                ["client_id"] = _options.ClientId,
            });

            var tokenUrl = _options.OidcBaseUrl + EasyTraderEndpoints.OidcToken;
            using var res = await session.Client.PostAsync(tokenUrl, content, ct);

            if (!res.IsSuccessStatusCode)
            {
                var errBody = await SafeReadBody(res, ct);
                throw new EasyTraderException(
                    $"Token exchange failed {(int)res.StatusCode}. " +
                    $"Body: {Truncate(errBody, 300)}",
                    (int)res.StatusCode, errBody);
            }

            var json = await res.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<OidcTokenResponse>(json, JsonOpts)
                ?? throw new EasyTraderException("Token response deserialization failed");
        }

        /// <summary>POST /easy/api/account/same-login (activation)</summary>
        public async Task SameLoginAsync(
            string accessToken,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.SameLogin;

            var body = new
            {
                uuid = Guid.NewGuid().ToString(),
                appBuildNo = _options.AppBuildNo,
                width = 452,
                height = 599,
                devicePlatform = "Desktop",
                platformInfo = _options.UserAgent,
            };

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            req.Content = new StringContent(
                JsonSerializer.Serialize(body, JsonOpts),
                Encoding.UTF8, "application/json");

            using var res = await client.SendAsync(req, ct);
            var resBody = await res.Content.ReadAsStringAsync(ct);

            if (res.IsSuccessStatusCode) return;

            if ((int)res.StatusCode == 400 &&
                resBody.Contains("already logged in", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("EasyTrader token already active");
                return;
            }

            throw new EasyTraderException(
                $"Token activation failed {(int)res.StatusCode}. " +
                $"Body: {Truncate(resBody, 300)}",
                (int)res.StatusCode, resBody);
        }

        /// <summary>Helper: استخراج anti-forgery token از HTML صفحه‌ی لاگین.</summary>
        public static string? ExtractAntiForgeryToken(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return doc.DocumentNode
                .SelectSingleNode("//input[@name='__RequestVerificationToken']")
                ?.GetAttributeValue("value", string.Empty);
        }

        /* ══════════════════════════════════════════════════
           ACCOUNT
           ══════════════════════════════════════════════════ */

        /// <summary>GET /easy/api/account/server-time/{ts}</summary>
        public async Task<ServerTimeResponse> GetServerTimeAsync(
            string accessToken,
            long clientTimestampMs,
            CancellationToken ct = default)
        {
            var path = string.Format(EasyTraderEndpoints.ServerTime, clientTimestampMs);
            var url = _options.BaseUrl + path;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Server-time failed {(int)res.StatusCode}. Body: {Truncate(body, 200)}",
                    (int)res.StatusCode, body);

            return JsonSerializer.Deserialize<ServerTimeResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Server-time deserialization failed");
        }

        /// <summary>GET /easy/api/money</summary>
        public async Task<MoneyResponse> GetMoneyAsync(
            string accessToken,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.Money;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Money failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            return JsonSerializer.Deserialize<MoneyResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Money deserialization failed");
        }

        /// <summary>GET /easy/api/clientAppSetting</summary>
        public async Task<ClientAppSettingResponse> GetClientAppSettingAsync(
            string accessToken,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.ClientAppSetting;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"ClientAppSetting failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            return JsonSerializer.Deserialize<ClientAppSettingResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("ClientAppSetting deserialization failed");
        }

        /* ══════════════════════════════════════════════════
           PORTFOLIO
           ══════════════════════════════════════════════════ */

        /// <summary>GET /assetmodule/api/performance</summary>
        public async Task<PerformanceResponse> GetPerformanceAsync(
            string accessToken,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.Performance;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Performance failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            return JsonSerializer.Deserialize<PerformanceResponse>(body, JsonOpts)
                ?? new PerformanceResponse();
        }

        /* ══════════════════════════════════════════════════
           ORDER
           ══════════════════════════════════════════════════ */

        

        /// <summary>ساخت درخواست سفارش — sync، بدون I/O.</summary>
        public HttpRequestMessage BuildOrderRequest(
            string accessToken,
            string symbolIsin,
            long price,
            long quantity,
            int side)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.Order;

            const decimal commission = 0.003712m;
            const int validityType = 0;
            const int orderModelType = 1;
            const int orderFrom = 34;

            var totalValue = (long)Math.Round(price * quantity * (1 + (double)commission));

            var payload = new EasyTraderOrderRequest
            {
                Order = new EasyTraderOrder
                {
                    Price = price,
                    Quantity = quantity,
                    Side = side,
                    ValidityType = validityType,
                    CreateDateTime = FormatEasyTraderDateTime(DateTime.Now),
                    Commission = commission,
                    SymbolIsin = symbolIsin,
                    SymbolName = symbolIsin,
                    OrderModelType = orderModelType,
                    TotalValue = totalValue,
                    OrderFrom = orderFrom,
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload, JsonOpts),
                    Encoding.UTF8,
                    "application/json"),
            };
            AttachAuth(request, accessToken);

            return request;
        }

        /// <summary>POST /core/api/v2/order — ارسال درخواست آماده‌شده.</summary>
        public async Task<OrderSendResult> SendOrderAsync(
            HttpRequestMessage request,
            string symbolIsin,
            CancellationToken ct = default)
        {
            using (request)
            {
                var fireAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var client = CreateSharedClient();
                using var res = await client.SendAsync(request, ct);
                var body = await res.Content.ReadAsStringAsync(ct);

                var receivedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (string.IsNullOrWhiteSpace(body))
                    throw new EasyTraderOrderException(
                        $"Order returned empty body for isin={symbolIsin}, status={(int)res.StatusCode}",
                        fireAtUnixMs,
                        receivedAtUnixMs,
                        (int)res.StatusCode);

                if (TryParseOrderResponse(body, out var parsed))
                    return new OrderSendResult(parsed, fireAtUnixMs, receivedAtUnixMs);

                if (!res.IsSuccessStatusCode)
                    throw new EasyTraderOrderException(
                        $"Order failed {(int)res.StatusCode}. Body: {Truncate(body, 400)}",
                        fireAtUnixMs,
                        receivedAtUnixMs,
                        (int)res.StatusCode,
                        body);

                throw new EasyTraderOrderException(
                    $"Unexpected order response: {Truncate(body, 400)}",
                    fireAtUnixMs,
                    receivedAtUnixMs,
                    (int)res.StatusCode,
                    body);
            }
        }

        /* ══════════════════════════════════════════════════
           SYMBOL
           ══════════════════════════════════════════════════ */

        /// <summary>POST /symbols/api/MarketData/symbol-info-data</summary>
        public async Task<SymbolInfoResponse> GetSymbolInfoAsync(
            string accessToken,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.SymbolInfo;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new { isin = symbolIsin }, JsonOpts),
                Encoding.UTF8, "application/json");
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Symbol info failed {(int)res.StatusCode} for isin={symbolIsin}. " +
                    $"Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            if (string.IsNullOrWhiteSpace(body))
                throw new EasyTraderException(
                    $"Symbol info returned empty body for isin={symbolIsin}. " +
                    $"این معمولاً یعنی ISIN ناشناخته است.",
                    (int)res.StatusCode);

            return JsonSerializer.Deserialize<SymbolInfoResponse>(body, JsonOpts)
                ?? throw new EasyTraderException(
                    $"Symbol info deserialization failed for isin={symbolIsin}");
        }

        /// <summary>POST /symbols/api/MarketData/return-chart-data</summary>
        public async Task<ReturnChartResponse> GetReturnChartDataAsync(
            string accessToken,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.ReturnChartData;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new { isin = symbolIsin }, JsonOpts),
                Encoding.UTF8, "application/json");
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Return chart failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            return JsonSerializer.Deserialize<ReturnChartResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Return chart deserialization failed");
        }

        /// <summary>POST /symbols/api/MarketData/indInstTrade</summary>
        public async Task<IndInstTradeResponse> GetIndInstTradeAsync(
            string accessToken,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.IndInstTrade;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new { isin = symbolIsin }, JsonOpts),
                Encoding.UTF8, "application/json");
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"IndInst trade failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            return JsonSerializer.Deserialize<IndInstTradeResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("IndInst trade deserialization failed");
        }

        /// <summary>GET /symbols/api/TradingTime</summary>
        public async Task<TradingTimeResponse> GetTradingTimeAsync(
            string accessToken,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.TradingTime;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Trading time failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            return JsonSerializer.Deserialize<TradingTimeResponse>(body, JsonOpts)
                ?? new TradingTimeResponse();
        }

        /* ══════════════════════════════════════════════════
           CHART
           ══════════════════════════════════════════════════ */

        /// <summary>GET /chart/api/v2/datafeed/miniChart/history</summary>
        public async Task<CandleHistoryResponse> GetChartHistoryAsync(
            string accessToken,
            string symbolIsin,
            int days,
            int intervalMinutes,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.ChartHistory
                + $"?symbol={Uri.EscapeDataString(symbolIsin)}:{intervalMinutes}&days={days}";

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Chart history failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            if (string.IsNullOrWhiteSpace(body))
                throw new EasyTraderException(
                    $"Chart history returned empty body for isin={symbolIsin}");

            return JsonSerializer.Deserialize<CandleHistoryResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Chart history deserialization failed");
        }

        /* ══════════════════════════════════════════════════
           ANALYSIS
           ══════════════════════════════════════════════════ */

        /// <summary>GET /easy/api/symbol-analysis/technical-analysis</summary>
        public async Task<TechnicalAnalysisResponse> GetTechnicalAnalysisAsync(
            string accessToken,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.TechnicalAnalysis
                + $"?isin={Uri.EscapeDataString(symbolIsin)}";

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Technical analysis failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            return JsonSerializer.Deserialize<TechnicalAnalysisResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Technical analysis deserialization failed");
        }

        /// <summary>GET /easy/api/symbol-analysis/ind-inst</summary>
        public async Task<IndInstAnalysisResponse?> GetIndInstAnalysisAsync(
            string accessToken,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.IndInstAnalysis
                + $"?isin={Uri.EscapeDataString(symbolIsin)}";

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"IndInst analysis failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            var list = JsonSerializer.Deserialize<List<IndInstAnalysisResponse>>(body, JsonOpts);
            return list?.FirstOrDefault();
        }

        /// <summary>GET /easy/api/symbol-analysis/ind-trading-trend</summary>
        public async Task<List<IndTradingTrendItem>> GetIndTradingTrendAsync(
            string accessToken,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.IndTradingTrend
                + $"?isin={Uri.EscapeDataString(symbolIsin)}";

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Ind trading trend failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            return JsonSerializer.Deserialize<List<IndTradingTrendItem>>(body, JsonOpts) ?? new();
        }

        /// <summary>GET /easy/api/symbol-analysis/fundamental-analysis</summary>
        public async Task<FundamentalAnalysisResponse> GetFundamentalAnalysisAsync(
            string accessToken,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.FundamentalAnalysis
                + $"?isin={Uri.EscapeDataString(symbolIsin)}";

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Fundamental analysis failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            if (string.IsNullOrWhiteSpace(body))
                throw new EasyTraderException(
                    $"Fundamental analysis returned empty body for isin={symbolIsin}");

            return JsonSerializer.Deserialize<FundamentalAnalysisResponse>(body, JsonOpts)
                ?? throw new EasyTraderException(
                    $"Fundamental analysis deserialization failed for isin={symbolIsin}");
        }

        /* ══════════════════════════════════════════════════
           MARKET SHEET
           ══════════════════════════════════════════════════ */

        /// <summary>GET /ms/api/MarketSheet/sum/{isin}</summary>
        public async Task<MarketSheetSumResponse> GetMarketSheetSumAsync(
            string accessToken,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl
                + string.Format(EasyTraderEndpoints.MarketSheetSum, symbolIsin);

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Market sheet failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            return JsonSerializer.Deserialize<MarketSheetSumResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Market sheet deserialization failed");
        }

        /* ══════════════════════════════════════════════════
           CREDIT
           ══════════════════════════════════════════════════ */

        /// <summary>GET /credit/api/isCreditCustomer</summary>
        public async Task<bool> IsCreditCustomerAsync(
            string accessToken,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.IsCreditCustomer;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"IsCreditCustomer failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            return body.Trim().ToLowerInvariant() == "true";
        }

        /* ══════════════════════════════════════════════════
           MARKET LIVE
           ══════════════════════════════════════════════════ */

        /// <summary>GET /easy/api/market-live/industries/positive-negative-symbols</summary>
        public async Task<List<IndustryPositiveNegativeResponse>> GetPositiveNegativeSymbolsAsync(
            string accessToken,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.PositiveNegativeSymbols;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Industry positive-negative failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            return JsonSerializer.Deserialize<List<IndustryPositiveNegativeResponse>>(body, JsonOpts)
                ?? new();
        }

        /// <summary>GET /symbols/api/MarketData/tse-index</summary>
        public async Task<List<TseIndexResponse>> GetTseIndexAsync(
            string accessToken,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.TseIndex;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"TSE index failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            return JsonSerializer.Deserialize<List<TseIndexResponse>>(body, JsonOpts) ?? new();
        }

        /// <summary>GET /easy/api/MarketWatch</summary>
        public async Task<List<MarketWatchResponse>> GetMarketWatchAsync(
            string accessToken,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.MarketWatch;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"MarketWatch failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            return JsonSerializer.Deserialize<List<MarketWatchResponse>>(body, JsonOpts) ?? new();
        }

        /* ══════════════════════════════════════════════════
           MARKET DATA (GraphQL)
           ══════════════════════════════════════════════════ */

        /// <summary>POST /symbols/api/marketdata (GraphQL batch)</summary>
        public async Task<List<MarketDataGraphQLItem>> GetMarketDataGraphQLAsync(
    string accessToken,
    IReadOnlyList<string> symbolIsins,
    CancellationToken ct = default)
        {
            if (symbolIsins is null || symbolIsins.Count == 0)
                return new();

            const int maxAttempts = 2;
            Exception? lastEx = null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await GetMarketDataGraphQLInternalAsync(
                        accessToken, symbolIsins, ct);
                }
                catch (HttpRequestException ex) when (attempt < maxAttempts)
                {
                    lastEx = ex;
                    // یک Delay کوتاه — Connection مرده از Pool حذف می‌شود
                    // و Retry با Connection تازه انجام می‌شود
                    await Task.Delay(300, ct);
                }
                catch (System.IO.IOException ex) when (attempt < maxAttempts)
                {
                    lastEx = ex;
                    await Task.Delay(300, ct);
                }
                catch (TaskCanceledException ex) when (attempt < maxAttempts && !ct.IsCancellationRequested)
                {
                    // Timeout — نه Cancel توسط کاربر
                    lastEx = ex;
                    await Task.Delay(300, ct);
                }
            }

            throw lastEx ?? new InvalidOperationException("Retry exhausted");
        }

        private async Task<List<MarketDataGraphQLItem>> GetMarketDataGraphQLInternalAsync(
            string accessToken,
            IReadOnlyList<string> symbolIsins,
            CancellationToken ct)
        {
            const string fields =
                "symbolIsin stateCode lastTradedPrice closingPrice totalTradeValue " +
                "totalNumberOfTrades feeOfPreviousDaysClosingPrice priceVar buyRatio sellRatio " +
                "highPrice lowPrice bestBuyPrice bestBuyQuantity bestSellPrice bestSellQuantity " +
                "firstTradedPrice lowAllowedPrice highAllowedPrice totalNumberOfSharesTraded " +
                "minValidBuyVolume maxValidBuyVolume minValidSellVolume maxValidSellVolume priceTickSize";

            var url = _options.BaseUrl + EasyTraderEndpoints.MarketDataGraphQL;

            var isinsPart = string.Join(",", symbolIsins.Select(i => $"\"{i}\""));
            var query = $"query {{marketData(request: {{ isins: [{isinsPart}] }}) {{ {fields} }} }}";

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new { query }, JsonOpts),
                Encoding.UTF8, "application/json");
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Batch marketData failed {(int)res.StatusCode}. Body: {Truncate(body, 400)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<MarketDataGraphQLResponse>(body, JsonOpts);
            return wire?.Data?.MarketData ?? new();
        }

        /* ══════════════════════════════════════════════════
           ORDER HISTORY
           ══════════════════════════════════════════════════ */

        /// <summary>POST /easy/api/orderHistory/orderReport</summary>
        public async Task<OrderReportResponse> GetOrderReportAsync(
            string accessToken,
            OrderReportRequest payload,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.OrderReport;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOpts),
                Encoding.UTF8, "application/json");
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Order report failed {(int)res.StatusCode}. Body: {Truncate(body, 400)}",
                    (int)res.StatusCode, body);

            return JsonSerializer.Deserialize<OrderReportResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Order report deserialization failed");
        }

        /// <summary>GET /easy/api/orderHistory/trades/ (order-id در header)</summary>
        public async Task<List<OrderTradeItem>> GetOrderTradesAsync(
            string accessToken,
            string orderId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new EasyTraderException("orderId cannot be empty");

            var url = _options.BaseUrl + EasyTraderEndpoints.OrderTrades;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);

            // orderId ممکنه کاراکترهای خاص داشته باشه → TryAddWithoutValidation
            req.Headers.TryAddWithoutValidation("order-id", orderId);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Order trades failed {(int)res.StatusCode} for orderId={orderId}. " +
                    $"Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            if (string.IsNullOrWhiteSpace(body))
                return new();

            return JsonSerializer.Deserialize<List<OrderTradeItem>>(body, JsonOpts) ?? new();
        }

        /* ══════════════════════════════════════════════════
           FINANCE
           ══════════════════════════════════════════════════ */

        /// <summary>GET /finance/api/payment-account-balances</summary>
        public async Task<PaymentAccountBalancesResponse> GetPaymentAccountBalancesAsync(
            string accessToken,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.PaymentAccountBalances;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Payment balances failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            return JsonSerializer.Deserialize<PaymentAccountBalancesResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Payment balances deserialization failed");
        }

        /// <summary>POST /finance/api/payments (withdrawal)</summary>
        public async Task<RawHttpResult> PostPaymentsAsync(
            string accessToken,
            WithdrawalRequestPayload payload,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.Payments;

            var fireAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOpts),
                Encoding.UTF8, "application/json");
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            var receivedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            return new RawHttpResult(
                res.IsSuccessStatusCode,
                (int)res.StatusCode,
                body,
                fireAt,
                receivedAt);
        }

        /// <summary>POST /finance/api/payments/{id}/cancel</summary>
        public async Task<RawHttpResult> PostPaymentCancelAsync(
            string accessToken,
            long paymentId,
            CancellationToken ct = default)
        {
            if (paymentId <= 0)
                throw new EasyTraderException("paymentId must be positive");

            var url = _options.BaseUrl
                + string.Format(EasyTraderEndpoints.PaymentCancel, paymentId);

            var fireAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            AttachAuth(req, accessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            var receivedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            return new RawHttpResult(
                res.IsSuccessStatusCode,
                (int)res.StatusCode,
                body,
                fireAt,
                receivedAt);
        }

        /* ══════════════════════════════════════════════════
           PRIVATE HELPERS
           ══════════════════════════════════════════════════ */

        private HttpClient CreateSharedClient()
        {
            var client = _httpFactory.CreateClient(HttpClientName);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/plain, */*");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "fa");
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
            return client;
        }

        private static void AttachAuth(HttpRequestMessage request, string accessToken)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
        }

        private string BuildAuthorizeUrl(string challenge, string state)
        {
            var qs = new Dictionary<string, string>
            {
                ["client_id"] = _options.ClientId,
                ["redirect_uri"] = _options.RedirectUri,
                ["response_type"] = "code",
                ["scope"] = _options.Scope,
                ["state"] = state,
                ["code_challenge"] = challenge,
                ["code_challenge_method"] = "S256",
            };

            var query = string.Join("&", qs.Select(kv =>
                $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

            return $"{_options.OidcBaseUrl}{EasyTraderEndpoints.OidcAuthorize}?{query}";
        }

        private static async Task<RedirectResult> ToRedirectResultAsync(
            HttpResponseMessage res, CancellationToken ct)
        {
            var statusCode = (int)res.StatusCode;

            if (statusCode is >= 300 and < 400)
            {
                return new RedirectResult(
                    statusCode,
                    res.Headers.Location?.ToString(),
                    null);
            }

            var body = await SafeReadBody(res, ct);
            return new RedirectResult(statusCode, null, body);
        }

        private static bool TryParseOrderResponse(string body, out OrderResponse result)
        {
            result = default!;
            if (string.IsNullOrWhiteSpace(body)) return false;
            try
            {
                var resp = JsonSerializer.Deserialize<OrderResponse>(body, JsonOpts);
                if (resp is null) return false;
                result = resp;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string MakeAbsolute(string baseUrl, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new EasyTraderException("URL path is empty");

            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            if (!baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                throw new EasyTraderException(
                    $"Invalid OidcBaseUrl: '{baseUrl}'. Expected http/https.");
            }

            var baseClean = baseUrl.TrimEnd('/');
            var pathClean = path.StartsWith('/') ? path : "/" + path;
            return baseClean + pathClean;
        }

        private static Dictionary<string, string> ParseQueryString(string query)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(query)) return result;

            query = query.TrimStart('?');
            foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var idx = pair.IndexOf('=');
                if (idx < 0) continue;
                var key = Uri.UnescapeDataString(pair[..idx]);
                var value = Uri.UnescapeDataString(pair[(idx + 1)..]);
                result[key] = value;
            }
            return result;
        }

        private static async Task<string> SafeReadBody(HttpResponseMessage res, CancellationToken ct)
        {
            try { return await res.Content.ReadAsStringAsync(ct); }
            catch { return "<unreadable>"; }
        }

        private static string Truncate(string? s, int max)
        {
            if (string.IsNullOrEmpty(s)) return s ?? string.Empty;
            return s.Length <= max ? s : s[..max] + "...";
        }

        private static string FormatEasyTraderDateTime(DateTime d)
        {
            var h24 = d.Hour;
            var h12 = h24 % 12 == 0 ? 12 : h24 % 12;
            var ampm = h24 < 12 ? "AM" : "PM";
            return $"{d.Month}/{d.Day}/{d.Year}, {h12}:{d.Minute:D2}:{d.Second:D2} {ampm}";
        }
    }
}