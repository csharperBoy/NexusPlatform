using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Trader.Infrastructure.Brokers.EasyTrader;
using Trader.Infrastructure.Brokers.EasyTrader.Internal;

namespace Trader.ExternalApiClient.EasyTrader
{
    /* ═══ نتیجه‌ی ارسال سفارش همراه با timings ═══ */
    public sealed record OrderSendResult(
        OrderResponse Response,
        long FireAtUnixMs,
        long ReceivedAtUnixMs);

    /* ═══ نتیجه‌ی خام HTTP (برای withdrawal/cancel) ═══ */
    public readonly record struct RawHttpResult(
        bool IsSuccessful,
        int StatusCode,
        string? Body,
        long FireAtUnixMs,
        long ReceivedAtUnixMs);

    /// <summary>
    /// کلاینت خام EasyTrader.
    /// تمام فراخوانی‌های HTTP، ساخت request و deserialize اینجا انجام می‌شه.
    /// فقط در سطح wire DTO کار می‌کنه و هیچ اطلاعی از Application DTOها نداره.
    /// </summary>
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
           AUTH (OIDC + PKCE + Activation)
           ══════════════════════════════════════════════════ */
        public async Task<OidcTokenResponse> LoginAsync(
            string username,
            string password,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
                throw new EasyTraderException("Username/Password cannot be empty");

            var verifier = EasyTraderPkce.GenerateVerifier();
            var challenge = EasyTraderPkce.GenerateChallenge(verifier);
            var state = EasyTraderPkce.GenerateState();

            var cookieJar = new CookieContainer();
            using var handler = new HttpClientHandler
            {
                CookieContainer = cookieJar,
                AllowAutoRedirect = false,
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.All,
            };
            using var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds),
            };

            if (!string.IsNullOrWhiteSpace(_options.UserAgent))
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);

            try
            {
                /* ─── ۱. GET /connect/authorize ─── */
                var authorizeUrl = BuildAuthorizeUrl(challenge, state);

                using var authorizeRes = await client.GetAsync(authorizeUrl, ct);
                if ((int)authorizeRes.StatusCode is not (302 or 303))
                    throw new EasyTraderException(
                        $"Authorize expected 302/303, got {(int)authorizeRes.StatusCode}",
                        (int)authorizeRes.StatusCode);

                /* ─── ۲. GET /Login ─── */
                var loginPath = authorizeRes.Headers.Location?.ToString()
                    ?? throw new EasyTraderException("Authorize response has no Location");
                var loginUrl = MakeAbsolute(_options.OidcBaseUrl, loginPath);

                using var loginPageRes = await client.GetAsync(loginUrl, ct);
                if (!loginPageRes.IsSuccessStatusCode)
                    throw new EasyTraderException(
                        $"Login page returned {(int)loginPageRes.StatusCode}",
                        (int)loginPageRes.StatusCode);

                var loginHtml = await loginPageRes.Content.ReadAsStringAsync(ct);
                var verificationToken = ExtractAntiForgeryToken(loginHtml)
                    ?? throw new EasyTraderException("__RequestVerificationToken not found");

                /* ─── ۳. POST /Login ─── */
                using var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["Username"] = username,
                    ["Password"] = password,
                    ["__RequestVerificationToken"] = verificationToken,
                });

                using var loginRes = await client.PostAsync(loginUrl, formContent, ct);
                if ((int)loginRes.StatusCode is not (302 or 303))
                {
                    var body = await SafeReadBody(loginRes, ct);
                    throw new EasyTraderException(
                        $"Login expected 302/303, got {(int)loginRes.StatusCode}. " +
                        $"Body: {Truncate(body, 200)}",
                        (int)loginRes.StatusCode, body);
                }

                /* ─── ۴. GET /connect/authorize/callback ─── */
                var callbackPath = loginRes.Headers.Location?.ToString()
                    ?? throw new EasyTraderException("Login response has no Location");
                var callbackUrl = MakeAbsolute(_options.OidcBaseUrl, callbackPath);

                using var callbackRes = await client.GetAsync(callbackUrl, ct);
                if ((int)callbackRes.StatusCode is not (302 or 303))
                    throw new EasyTraderException(
                        $"Callback expected 302/303, got {(int)callbackRes.StatusCode}",
                        (int)callbackRes.StatusCode);

                /* ─── ۵. Extract code ─── */
                var finalLocation = callbackRes.Headers.Location?.ToString()
                    ?? throw new EasyTraderException("Callback response has no Location");

                var codeParams = ParseQueryString(new Uri(finalLocation).Query);
                codeParams.TryGetValue("code", out var code);
                codeParams.TryGetValue("state", out var returnedState);

                if (string.IsNullOrEmpty(code))
                    throw new EasyTraderException("No 'code' in callback URL");

                if (returnedState != state)
                    throw new EasyTraderException("State mismatch — possible CSRF");

                /* ─── ۶. POST /connect/token ─── */
                using var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "authorization_code",
                    ["redirect_uri"] = _options.RedirectUri,
                    ["code"] = code,
                    ["code_verifier"] = verifier,
                    ["client_id"] = _options.ClientId,
                });

                var tokenUrl = _options.OidcBaseUrl + EasyTraderEndpoints.OidcToken;
                using var tokenRes = await client.PostAsync(tokenUrl, tokenContent, ct);

                if (!tokenRes.IsSuccessStatusCode)
                {
                    var body = await SafeReadBody(tokenRes, ct);
                    throw new EasyTraderException(
                        $"Token exchange failed {(int)tokenRes.StatusCode}. " +
                        $"Body: {Truncate(body, 300)}",
                        (int)tokenRes.StatusCode, body);
                }

                var tokenJson = await tokenRes.Content.ReadAsStringAsync(ct);
                var tokenResponse = JsonSerializer.Deserialize<OidcTokenResponse>(tokenJson, JsonOpts)
                    ?? throw new EasyTraderException("Token response deserialization failed");

                if (string.IsNullOrEmpty(tokenResponse.AccessToken))
                    throw new EasyTraderException("access_token missing in response");

                _logger.LogInformation(
                    "EasyTrader login successful, expiresIn={Expires}s",
                    tokenResponse.ExpiresIn);

                /* ─── ۷. Activation (same-login) ─── */
                await ActivateTokenAsync(client, tokenResponse.AccessToken, ct);

                return tokenResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "EasyTrader login failed: network/SSL error");
                throw;
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(ex,
                    "EasyTrader login timed out after {Timeout}s",
                    _options.TimeoutSeconds);
                throw new EasyTraderException(
                    $"EasyTrader login timed out after {_options.TimeoutSeconds}s", ex);
            }
        }

        private async Task ActivateTokenAsync(
            HttpClient client,
            string accessToken,
            CancellationToken ct)
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

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            req.Headers.Accept.ParseAdd("application/json, text/plain, */*");
            req.Headers.TryAddWithoutValidation("Accept-Language", "fa");
            req.Content = new StringContent(
                JsonSerializer.Serialize(body, JsonOpts),
                Encoding.UTF8, "application/json");

            using var res = await client.SendAsync(req, ct);
            var resBody = await res.Content.ReadAsStringAsync(ct);

            if (res.IsSuccessStatusCode)
            {
                _logger.LogDebug("EasyTrader token activated");
                return;
            }

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

        /* ══════════════════════════════════════════════════
           ACCOUNT
           ══════════════════════════════════════════════════ */
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
        /// <summary>
        /// ساخت HttpRequestMessage آماده برای fire سریع. sync است.
        /// </summary>
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

        /// <summary>
        /// ارسال request آماده‌شده. بدون لاگ — حداکثر سرعت.
        /// در صورت خطا EasyTraderOrderException با timings پرتاب می‌شه.
        /// </summary>
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

        /* ══════════════════════════════════════════════════
           SYMBOL
           ══════════════════════════════════════════════════ */
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

        public async Task<ReturnChartResponse> GetReturnChartAsync(
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

        public async Task<TradingTimeResponse> GetTradingTimesAsync(
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
        public async Task<List<IndustryPositiveNegativeResponse>> GetIndustryPositiveNegativeAsync(
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
           BATCH MARKET DATA (GraphQL)
           ══════════════════════════════════════════════════ */
        private const string MarketDataGraphQLFields =
            "symbolIsin stateCode lastTradedPrice closingPrice totalTradeValue " +
            "totalNumberOfTrades feeOfPreviousDaysClosingPrice priceVar buyRatio sellRatio " +
            "highPrice lowPrice bestBuyPrice bestBuyQuantity bestSellPrice bestSellQuantity " +
            "firstTradedPrice lowAllowedPrice highAllowedPrice totalNumberOfSharesTraded " +
            "minValidBuyVolume maxValidBuyVolume minValidSellVolume maxValidSellVolume priceTickSize";

        public async Task<List<MarketDataGraphQLItem>> GetBatchMarketDataAsync(
            string accessToken,
            IReadOnlyList<string> symbolIsins,
            CancellationToken ct = default)
        {
            if (symbolIsins is null || symbolIsins.Count == 0)
                return new();

            var url = _options.BaseUrl + EasyTraderEndpoints.MarketDataGraphQL;

            var isinsPart = string.Join(",", symbolIsins.Select(i => $"\"{i}\""));
            var query =
                $"query {{marketData(request: {{ isins: [{isinsPart}] }}) " +
                $"{{ {MarketDataGraphQLFields} }} }}";

            var payload = new { query };

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
                    $"Batch marketData failed {(int)res.StatusCode}. Body: {Truncate(body, 400)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<MarketDataGraphQLResponse>(body, JsonOpts);
            return wire?.Data?.MarketData ?? new();
        }

        /* ══════════════════════════════════════════════════
           ORDER HISTORY
           ══════════════════════════════════════════════════ */
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

        public async Task<RawHttpResult> RequestWithdrawalAsync(
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

        public async Task<RawHttpResult> CancelPaymentAsync(
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
           HELPERS
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

        private static string? ExtractAntiForgeryToken(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var node = doc.DocumentNode
                .SelectSingleNode("//input[@name='__RequestVerificationToken']");

            return node?.GetAttributeValue("value", string.Empty);
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