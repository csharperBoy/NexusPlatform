using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Trader.Application.Abstractions;
using Trader.Application.Brokers;
using Trader.Application.Dtos;
using Trader.Application.Dtos.Account;
using Trader.Application.Dtos.MarketData;
using Trader.Application.Dtos.Order;
using Trader.Domain.Enums;
using Trader.Infrastructure.Brokers.EasyTrader.Internal;

namespace Trader.Infrastructure.Brokers.EasyTrader
{
    public class EasyTraderBrokerClient : IBrokerClient
    {
        private readonly EasyTraderOptions _options;

        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<EasyTraderBrokerClient> _logger;

        //private static readonly JsonSerializerOptions JsonOpts = new()
        //{
        //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        //};

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };


        public EasyTraderBrokerClient(
            IOptions<EasyTraderOptions> options,
            IHttpClientFactory httpFactory,
            ILogger<EasyTraderBrokerClient> logger)
        {
            _options = options.Value;
            _httpFactory = httpFactory;
            _logger = logger;
        }

        public BrokerType BrokerType => BrokerType.EasyTrader;

        /* ══════════════════════════════════════════════════
           SESSION
           ══════════════════════════════════════════════════ */
        public string SerializeSession(BrokerSession session)
        {
            if (session is not EasyTraderSession et)
                throw new ArgumentException(
                    $"Expected {nameof(EasyTraderSession)}", nameof(session));
            return JsonSerializer.Serialize(et, JsonOpts);
        }

        public BrokerSession DeserializeSession(string json)
        {
            var session = JsonSerializer.Deserialize<EasyTraderSession>(json, JsonOpts)
                ?? throw new InvalidOperationException("Session deserialization returned null");
            return session;
        }

        private static EasyTraderSession AsSession(BrokerSession s)
            => s as EasyTraderSession
               ?? throw new ArgumentException("Session is not EasyTraderSession");

        /* ══════════════════════════════════════════════════
           LOGIN (OIDC + PKCE + Activation)
           ══════════════════════════════════════════════════ */
        public async Task<BrokerSession> LoginAsync(
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
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);

            /* ─── ۱. GET /connect/authorize ─── */
            var authorizeUrl = BuildAuthorizeUrl(challenge, state);
            _logger.LogDebug("EasyTrader OIDC step 1: authorize");

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

            var exp = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            return new EasyTraderSession(tokenResponse.AccessToken, exp);
        }

        /* ══════════════════════════════════════════════════
           ACTIVATION (internal — part of login flow)
           ══════════════════════════════════════════════════ */
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
           MEASURE LATENCY
           ══════════════════════════════════════════════════ */
        public async Task<BrokerTimeMeasurement> MeasureLatencyAsync(
                                                        BrokerSession session,
                                                        CancellationToken ct = default)
        {
            var s = AsSession(session);
            var clientTs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var path = string.Format(EasyTraderEndpoints.ServerTime, clientTs);
            var url = _options.BaseUrl + path;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, s.AccessToken);
            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);
            sw.Stop();

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Server-time failed {(int)res.StatusCode}. Body: {Truncate(body, 200)}",
                    (int)res.StatusCode, body);

            var response = JsonSerializer.Deserialize<ServerTimeResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Server-time deserialization failed");

            var rtt = sw.ElapsedMilliseconds;

            return new BrokerTimeMeasurement(
                Diff: response.Diff,
                OneWayLatencyMs: rtt / 2,
                RttMs: rtt);
        }

        /* ══════════════════════════════════════════════════
           SYMBOL INFO
           ══════════════════════════════════════════════════ */
        public async Task<SymbolMarketDataDto> GetSymbolInfoAsync(
    BrokerSession session,
    string symbolIsin,
    CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.SymbolInfo;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new { isin = symbolIsin }, JsonOpts),
                Encoding.UTF8, "application/json");
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Symbol info failed {(int)res.StatusCode} for isin={symbolIsin}. " +
                    $"Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            /* ✅ چک body خالی — مهم برای تشخیص زودتر */
            if (string.IsNullOrWhiteSpace(body))
                throw new EasyTraderException(
                    $"Symbol info returned empty body for isin={symbolIsin}. " +
                    $"این معمولاً یعنی ISIN ناشناخته است.",
                    (int)res.StatusCode);

            var wire = JsonSerializer.Deserialize<SymbolInfoResponse>(body, JsonOpts)
                ?? throw new EasyTraderException(
                    $"Symbol info deserialization failed for isin={symbolIsin}");

            return new SymbolMarketDataDto
            {
                SymbolName = symbolIsin,
                HighAllowedPrice = wire.HighAllowedPrice,
                LowAllowedPrice = wire.LowAllowedPrice,
                LastTradedPrice = wire.LastTradedPrice,
                ClosingPrice = wire.ClosingPrice,
                FirstTradedPrice = wire.FirstTradedPrice,
                TradeDate = wire.TradeDate,
                FetchedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };
        }

        /* ══════════════════════════════════════════════════
           SEND BUY / SELL ORDER
           ══════════════════════════════════════════════════ */

        public Task<BrokerOrderResultDto> SendBuyOrderAsync(
            BrokerSession session, string symbolIsin, long price, long quantity,
            CancellationToken ct = default)
            => SendOrderLegacyAsync(session, symbolIsin, price, quantity, side: 0, ct);

        public Task<BrokerOrderResultDto> SendSellOrderAsync(
            BrokerSession session, string symbolIsin, long price, long quantity,
            CancellationToken ct = default)
            => SendOrderLegacyAsync(session, symbolIsin, price, quantity, side: 1, ct);


        /* برای سازگاری با API قدیمی — PlanExecutor از این استفاده نمی‌کنه */
        private async Task<BrokerOrderResultDto> SendOrderLegacyAsync(
            BrokerSession session,
            string symbolIsin,
            long price,
            long quantity,
            int side,
            CancellationToken ct)
        {
            var request = BuildOrderRequest(session, symbolIsin, price, quantity, side);
            return await SendOrderWithRequestAsync(session, request, symbolIsin, ct);
        }
        public async Task<BrokerOrderResultDto> SendOrderWithRequestAsync(
     BrokerSession session,
     HttpRequestMessage request,
     string symbolIsin,
     CancellationToken ct = default)
        {
            using (request)
            {
                /* ═══ بدون هیچ لاگی — حداکثر سرعت ═══ */
                var fireAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var client = CreateSharedClient();
                using var res = await client.SendAsync(request, ct);
                var body = await res.Content.ReadAsStringAsync(ct);

                var receivedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                /* ─── empty body ─── */
                if (string.IsNullOrWhiteSpace(body))
                    throw new EasyTraderOrderException(
                        $"Order returned empty body for isin={symbolIsin}, status={(int)res.StatusCode}",
                        fireAtUnixMs,
                        receivedAtUnixMs,
                        (int)res.StatusCode);

                /* ─── پارس موفق ─── */
                if (TryParseOrderResponse(body, out var parsed))
                {
                    parsed.FireAtUnixMs = fireAtUnixMs;
                    parsed.ReceivedAtUnixMs = receivedAtUnixMs;
                    return parsed;
                }

                /* ─── خطای HTTP ─── */
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
        private static bool TryParseOrderResponse(string body, out BrokerOrderResultDto result)
        {
            result = default!;
            if (string.IsNullOrWhiteSpace(body)) return false;

            try
            {
                var resp = JsonSerializer.Deserialize<OrderResponse>(body, JsonOpts);
                if (resp is null) return false;

                var err = resp.OmsError?.FirstOrDefault();

                result = new BrokerOrderResultDto
                {
                    IsSuccessful = resp.IsSuccessful,
                    OrderId = resp.Id,
                    Message = resp.Message,
                    ErrorCode = err?.Code,
                    ErrorName = err?.Name,
                };
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// آماده‌سازی HttpRequestMessage و HttpClient برای fire سریع.
        /// این متد sync هست — فقط ساختار داده می‌سازه.
        /// </summary>
        public HttpRequestMessage BuildOrderRequest(
    BrokerSession session,
    string symbolIsin,
    long price,
    long quantity,
    int side)
        {
            var s = AsSession(session);
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
            AttachAuth(request, s.AccessToken);

            return request;
        }
        /* ══════════════════════════════════════════════════
   CANDLES
   ══════════════════════════════════════════════════ */
        public async Task<List<CandleDto>> GetCandlesAsync(
            BrokerSession session,
            string symbolIsin,
            int days = 1,
            int intervalMinutes = 1,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.ChartHistory
                + $"?symbol={Uri.EscapeDataString(symbolIsin)}:{intervalMinutes}&days={days}";

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Chart history failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            if (string.IsNullOrWhiteSpace(body))
                throw new EasyTraderException(
                    $"Chart history returned empty body for isin={symbolIsin}");

            var wire = JsonSerializer.Deserialize<CandleHistoryResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Chart history deserialization failed");

            return wire.Data.Select(d => new CandleDto
            {
                T = d.T,
                O = d.O,
                H = d.H,
                L = d.L,
                C = d.C,
                V = d.V,
            }).ToList();
        }

        /* ══════════════════════════════════════════════════
           RETURN CHART
           ══════════════════════════════════════════════════ */
        public async Task<ReturnChartDto> GetReturnChartAsync(
            BrokerSession session,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.ReturnChartData;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new { isin = symbolIsin }, JsonOpts),
                Encoding.UTF8, "application/json");
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Return chart failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<ReturnChartResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Return chart deserialization failed");

            return new ReturnChartDto
            {
                LastTradedPrice = wire.LastTradedPrice,
                E30 = wire.E30,
                E90 = wire.E90,
                E360 = wire.E360,
                MaturityDay = wire.MaturityDay,
                DaysToMaturity = wire.DaysToMaturity,
                ReturnToMaturity = wire.ReturnToMaturity,
            };
        }

        /* ══════════════════════════════════════════════════
           TECHNICAL ANALYSIS
           ══════════════════════════════════════════════════ */
        public async Task<TechnicalAnalysisDto> GetTechnicalAnalysisAsync(
            BrokerSession session,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.TechnicalAnalysis
                + $"?isin={Uri.EscapeDataString(symbolIsin)}";

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Technical analysis failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<TechnicalAnalysisResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Technical analysis deserialization failed");

            return new TechnicalAnalysisDto
            {
                TotalScore = new TechnicalScoreDto
                {
                    Cat = wire.TotalScore?.Cat ?? "totalscore",
                    Value = wire.TotalScore?.Value ?? 0,
                    State = wire.TotalScore?.State ?? "Neutral",
                },
                CategoryScore = (wire.CategoryScore ?? new()).Select(c => new TechnicalCategoryScoreDto
                {
                    Cat = c.Cat ?? "",
                    CatFa = c.CatFa ?? "",
                    State = c.State ?? "Neutral",
                }).ToList(),
            };
        }

        /* ══════════════════════════════════════════════════
           IND/INST TRADE
           ══════════════════════════════════════════════════ */
        public async Task<IndInstTradeDto> GetIndInstTradeAsync(
            BrokerSession session,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.IndInstTrade;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new { isin = symbolIsin }, JsonOpts),
                Encoding.UTF8, "application/json");
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"IndInst trade failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<IndInstTradeResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("IndInst trade deserialization failed");

            return new IndInstTradeDto
            {
                SymbolIsin = wire.SymbolISIN,
                IndBuyVolume = ParseLong(wire.IndBuyVolume),
                IndBuyNumber = ParseLong(wire.IndBuyNumber),
                IndSellVolume = ParseLong(wire.IndSellVolume),
                IndSellNumber = ParseLong(wire.IndSellNumber),
                InsBuyVolume = ParseLong(wire.InsBuyVolume),
                InsBuyNumber = ParseLong(wire.InsBuyNumber),
                InsSellVolume = ParseLong(wire.InsSellVolume),
                InsSellNumber = ParseLong(wire.InsSellNumber),
                Date = wire.Date,
            };
        }

        /* ══════════════════════════════════════════════════
           IND/INST ANALYSIS
           ══════════════════════════════════════════════════ */
        public async Task<IndInstAnalysisDto> GetIndInstAnalysisAsync(
            BrokerSession session,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.IndInstAnalysis
                + $"?isin={Uri.EscapeDataString(symbolIsin)}";

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"IndInst analysis failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            var list = JsonSerializer.Deserialize<List<IndInstAnalysisResponse>>(body, JsonOpts);
            var wire = list?.FirstOrDefault();

            if (wire is null)
                throw new EasyTraderException(
                    $"IndInst analysis returned empty for isin={symbolIsin}");

            return new IndInstAnalysisDto
            {
                IndBuyVol = wire.IndBuyVol,
                IndBuyPow = wire.IndBuyPow,
                IndSellVol = wire.IndSellVol,
                InsSellVol = wire.InsSellVol,
                InsBuyVol = wire.InsBuyVol,
                BidPres = wire.BidPres,
                NetInd = wire.NetInd,
                BuyPerInd = wire.BuyPerInd,
                SellPerInd = wire.SellPerInd,
                DiffValInd = wire.DiffValInd,
            };
        }

        /* ══════════════════════════════════════════════════
           IND TRADING TREND
           ══════════════════════════════════════════════════ */
        public async Task<List<IndTradingTrendDto>> GetIndTradingTrendAsync(
            BrokerSession session,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.IndTradingTrend
                + $"?isin={Uri.EscapeDataString(symbolIsin)}";

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Ind trading trend failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            var list = JsonSerializer.Deserialize<List<IndTradingTrendItem>>(body, JsonOpts)
                ?? new();

            return list.Select(x => new IndTradingTrendDto
            {
                Type = x.Type ?? "",
                Date = x.Date ?? "",
                Val = x.Val,
            }).ToList();
        }

        /* ══════════════════════════════════════════════════
           MARKET SHEET SUM
           ══════════════════════════════════════════════════ */
        public async Task<MarketSheetSumDto> GetMarketSheetSumAsync(
            BrokerSession session,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl
                + string.Format(EasyTraderEndpoints.MarketSheetSum, symbolIsin);

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Market sheet failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<MarketSheetSumResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Market sheet deserialization failed");

            return new MarketSheetSumDto
            {
                BuyVolume = wire.BuyVolume,
                BuyCount = wire.BuyCount,
                SellVolume = wire.SellVolume,
                SellCount = wire.SellCount,
            };
        }
        /* ══════════════════════════════════════════════════
   CASH BALANCE
   ══════════════════════════════════════════════════ */
        public async Task<MoneyDto> GetCashBalanceAsync(
            BrokerSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.Money;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Money failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<MoneyResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Money deserialization failed");

            return new MoneyDto
            {
                T0 = wire.T0,
                T1 = wire.T1,
                T2 = wire.T2,
                BuyPowerT0 = wire.BuyPowerT0,
                BuyPowerT1 = wire.BuyPowerT1,
                BuyPowerT2 = wire.BuyPowerT2,
                BlockT2 = wire.BlockT2,
                WithdrawBlockT2 = wire.WithdrawBlockT2,
                MarginBlock = wire.MarginBlock,
                Credit = wire.Credit,
                AvandCredit = wire.AvandCredit,
                WalletWithdrawBalanceT0 = wire.WalletWithdrawBalanceT0,
                WarrantValueCredit = wire.WarrantValueCredit,
                HamiBalance = wire.HamiBalance,
                Block = wire.Block,
            };
        }

        /* ══════════════════════════════════════════════════
           PORTFOLIO PERFORMANCE
           ══════════════════════════════════════════════════ */
        public async Task<List<PortfolioPerformanceDto>> GetPortfolioPerformanceAsync(
            BrokerSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.Performance;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Performance failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<PerformanceResponse>(body, JsonOpts);
            if (wire?.Items is null) return new();

            return wire.Items.Select(p => new PortfolioPerformanceDto
            {
                SymbolIsin = p.SymbolIsin ?? "",
                SymbolName = p.SymbolName ?? "",
                Date = p.Date ?? "",
                PersianDate = p.PersianDate,
                Asset = p.Asset,
                BonusShareVolume = p.BonusShareVolume,
                StockRightVolume = p.StockRightVolume,
                PeriodicDPSAmount = p.PeriodicDPSAmount,
                LastDPSAmount = p.LastDPSAmount,
                TotalDPSAmount = p.TotalDPSAmount,
                BreakevenPoint = p.BreakevenPoint,
                BreakevenWithoutDps = p.BreakevenWithoutDps,
                PeriodicBuyAveragePrice = p.PeriodicBuyAveragePrice,
                PeriodicBuyAveragePriceWithoutDps = p.PeriodicBuyAveragePriceWithoutDps,
                PeriodicBuyAmount = p.PeriodicBuyAmount,
                PeriodicSellAveragePrice = p.PeriodicSellAveragePrice,
                PeriodicSellAmount = p.PeriodicSellAmount,
                PeriodicBoughtVolume = p.PeriodicBoughtVolume,
                PeriodicSoldVolume = p.PeriodicSoldVolume,
                PeriodicRealizedProfitAndLoss = p.PeriodicRealizedProfitAndLoss,
                PeriodicRealizedProfitAndLossWithoutDps = p.PeriodicRealizedProfitAndLossWithoutDps,
                TotalBuyAveragePrice = p.TotalBuyAveragePrice,
                TotalBuyAveragePriceWithoutDps = p.TotalBuyAveragePriceWithoutDps,
                TotalBuyAmount = p.TotalBuyAmount,
                TotalSellAveragePrice = p.TotalSellAveragePrice,
                TotalSellAmount = p.TotalSellAmount,
                TotalBoughtVolume = p.TotalBoughtVolume,
                TotalSoldVolume = p.TotalSoldVolume,
                TotalRealizedProfitAndLoss = p.TotalRealizedProfitAndLoss,
                TotalRealizedProfitAndLossWithoutDps = p.TotalRealizedProfitAndLossWithoutDps,
                DailyBuyAveragePrice = p.DailyBuyAveragePrice,
                DailySellAveragePrice = p.DailySellAveragePrice,
                DailyBoughtVolume = p.DailyBoughtVolume,
                DailySoldVolume = p.DailySoldVolume,
                PeriodicBuyNetPrice = p.PeriodicBuyNetPrice,
                PeriodicSellNetPrice = p.PeriodicSellNetPrice,
                TotalBuyNetPrice = p.TotalBuyNetPrice,
                TotalSellNetPrice = p.TotalSellNetPrice,
            }).ToList();
        }

        /* ══════════════════════════════════════════════════
           TRADING TIMES
           ══════════════════════════════════════════════════ */
        public async Task<List<TradingTimeDto>> GetTradingTimesAsync(
            BrokerSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.TradingTime;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Trading time failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<TradingTimeResponse>(body, JsonOpts);
            if (wire?.Items is null) return new();

            return wire.Items.Select(t => new TradingTimeDto
            {
                Title = t.Title ?? new(),
                Days = t.Days ?? new(),
                TradingTimeType = t.TradingTimeType,
                StartPreTradingTime = t.StartPreTradingTime,
                EndPreTradingTime = t.EndPreTradingTime,
                StartTradingTime = t.StartTradingTime,
                EndTradingTime = t.EndTradingTime,
                StartTalTime = t.StartTalTime,
                EndTalTime = t.EndTalTime,
            }).ToList();
        }

        /* ══════════════════════════════════════════════════
           CLIENT APP SETTING
           ══════════════════════════════════════════════════ */
        public async Task<ClientAppSettingDto> GetClientAppSettingAsync(
            BrokerSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.ClientAppSetting;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"ClientAppSetting failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<ClientAppSettingResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("ClientAppSetting deserialization failed");

            return new ClientAppSettingDto
            {
                LightTheme = wire.LightTheme,
                BuyQuantity = wire.BuyQuantity,
                SellQuantity = wire.SellQuantity,
                Tick = wire.Tick,
                TickType = wire.TickType ?? "",
                PriceFromHeadline = wire.PriceFromHeadline,
                OrderConfirmation = wire.OrderConfirmation,
                DivideOrderToMultiple = wire.DivideOrderToMultiple,
                NotchUp = wire.NotchUp,
                NotchDown = wire.NotchDown,
                PageSize = wire.PageSize,
                ApplyCommissionInPortfolio = wire.ApplyCommissionInPortfolio,
                UseClosingPriceInPortfolioTotalValue = wire.UseClosingPriceInPortfolioTotalValue,
                ShowNotifications = wire.ShowNotifications,
                DataTracker = wire.DataTracker,
                UsePersianNumber = wire.UsePersianNumber,
                NoSleep = wire.NoSleep,
                NoBalance = wire.NoBalance,
                UserStatusBarToUp = wire.UserStatusBarToUp,
                PortfolioBasedOnLastPositivePeriod = wire.PortfolioBasedOnLastPositivePeriod,
                PortfolioTotalValueCalculateType = wire.PortfolioTotalValueCalculateType,
                KeepOrderFormAfterSubmit = wire.KeepOrderFormAfterSubmit,
                PriceFromHeadlineSide = wire.PriceFromHeadlineSide,
                BlinkOnDataChange = wire.BlinkOnDataChange,
                PortfolioBuyAveragePriceCalculationType = wire.PortfolioBuyAveragePriceCalculationType,
                PortfolioCalculationType = wire.PortfolioCalculationType,
                SoldPortfolioBuyAveragePriceCalculationType = wire.SoldPortfolioBuyAveragePriceCalculationType,
                SoldPortfolioCalculationType = wire.SoldPortfolioCalculationType,
                Volatility = wire.Volatility,
                InterestRate = wire.InterestRate,
                ShowAsLastPeriodAsset = wire.ShowAsLastPeriodAsset,
            };
        }

        /* ══════════════════════════════════════════════════
           IS CREDIT CUSTOMER
           ══════════════════════════════════════════════════ */
        public async Task<bool> IsCreditCustomerAsync(
            BrokerSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.IsCreditCustomer;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"IsCreditCustomer failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            // response: true / false
            return body.Trim().ToLowerInvariant() == "true";
        }
        /* ══════════════════════════════════════════════════
   INDUSTRY POSITIVE / NEGATIVE SYMBOLS
   ══════════════════════════════════════════════════ */
        public async Task<List<IndustryPositiveNegativeDto>> GetIndustryPositiveNegativeAsync(
            BrokerSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.PositiveNegativeSymbols;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Industry positive-negative failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<List<IndustryPositiveNegativeResponse>>(body, JsonOpts)
                ?? new();

            return wire.Select(x => new IndustryPositiveNegativeDto
            {
                IndustryCode = x.IndustryCode ?? "",
                IndustryName = x.IndustryName ?? "",
                NegativeCount = x.NegativeCount,
                ZeroCount = x.ZeroCount,
                PositiveCount = x.PositiveCount,
            }).ToList();
        }

        /* ══════════════════════════════════════════════════
           TSE INDEX
           ══════════════════════════════════════════════════ */
        public async Task<List<TseIndexDto>> GetTseIndexAsync(
            BrokerSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.TseIndex;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"TSE index failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<List<TseIndexResponse>>(body, JsonOpts) ?? new();

            return wire.Select(x => new TseIndexDto
            {
                SymbolIsin = x.SymbolIsin ?? "",
                SymbolTitle = x.SymbolTitle ?? "",
                DayOfEvent = DateTimeOffset.TryParse(x.DayOfEvent, out var dt) ? dt : default,
                IndexChanges = x.IndexChanges,
                LastIndexValue = x.LastIndexValue,
                PercentVariation = x.PercentVariation,
            }).ToList();
        }

        /* ══════════════════════════════════════════════════
           MARKET WATCH
           ══════════════════════════════════════════════════ */
        public async Task<List<MarketWatchCategoryDto>> GetMarketWatchAsync(
            BrokerSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.MarketWatch;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"MarketWatch failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<List<MarketWatchResponse>>(body, JsonOpts) ?? new();

            return wire.Select(x => new MarketWatchCategoryDto
            {
                Id = (x.Id ?? "").Trim(),               // ← کارگزاری id رو با فاصله‌ی انتهایی برمی‌گردونه
                Name = x.Name ?? "",
                Order = x.Order,
                IsDefault = x.IsDefault,
                CustomerIsin = x.CustomerIsin,
                CreateDateTime = DateTimeOffset.TryParse(x.CreateDateTime, out var dt) ? dt : null,
                SymbolIsins = (x.WatchCategorySymbols ?? new())
                    .Where(w => !string.IsNullOrEmpty(w.SymbolIsin))
                    .Select(w => w.SymbolIsin!)
                    .ToList(),
            }).ToList();
        }

        /* ══════════════════════════════════════════════════
           START SESSION
           ══════════════════════════════════════════════════ */
        public async Task<StartSessionDto> GetStartSessionAsync(
            BrokerSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.StartSession;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"StartSession failed {(int)res.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<StartSessionResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("StartSession deserialization failed");

            return new StartSessionDto
            {
                ServerTime = DateTimeOffset.TryParse(wire.ServerTime, out var st) ? st : default,
                StartSessionTimeStamp = DateTimeOffset.TryParse(wire.StartSessionTimeStamp, out var ss) ? ss : default,
            };
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

        public async Task<List<BatchMarketDataItemDto>> GetBatchMarketDataAsync(
            BrokerSession session,
            IReadOnlyList<string> symbolIsins,
            CancellationToken ct = default)
        {
            if (symbolIsins is null || symbolIsins.Count == 0)
                return new();

            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.MarketDataGraphQL;

            // ساخت query داینامیک — لیست isinها به‌صورت ["a","b",...]
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
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Batch marketData failed {(int)res.StatusCode}. Body: {Truncate(body, 400)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<MarketDataGraphQLResponse>(body, JsonOpts);
            var items = wire?.Data?.MarketData;
            if (items is null) return new();

            return items.Select(x => new BatchMarketDataItemDto
            {
                SymbolIsin = x.SymbolIsin ?? "",
                StateCode = x.StateCode ?? "",
                LastTradedPrice = x.LastTradedPrice,
                ClosingPrice = x.ClosingPrice,
                FeeOfPreviousDaysClosingPrice = x.FeeOfPreviousDaysClosingPrice,
                PriceVar = x.PriceVar,
                BuyRatio = ParseDecimal(x.BuyRatio),
                SellRatio = ParseDecimal(x.SellRatio),
                HighPrice = x.HighPrice,
                LowPrice = x.LowPrice,
                FirstTradedPrice = x.FirstTradedPrice,
                LowAllowedPrice = x.LowAllowedPrice,
                HighAllowedPrice = x.HighAllowedPrice,
                BestBuyPrice = x.BestBuyPrice,
                BestBuyQuantity = x.BestBuyQuantity,
                BestSellPrice = x.BestSellPrice,
                BestSellQuantity = x.BestSellQuantity,
                TotalTradeValue = x.TotalTradeValue,
                TotalNumberOfTrades = x.TotalNumberOfTrades,
                TotalNumberOfSharesTraded = x.TotalNumberOfSharesTraded,
                MinValidBuyVolume = x.MinValidBuyVolume,
                MaxValidBuyVolume = x.MaxValidBuyVolume,
                MinValidSellVolume = x.MinValidSellVolume,
                MaxValidSellVolume = x.MaxValidSellVolume,
                PriceTickSize = x.PriceTickSize,
            }).ToList();
        }

        public async Task<PagedResult<OrderHistoryItemDto>> GetOrderHistoryAsync(
    BrokerSession session,
    OrderHistoryQuery query,
    CancellationToken ct = default)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.OrderReport;

            // ساخت Sieve
            var sieve = new SieveRequest
            {
                Page = query.Page,
                PageSize = query.PageSize,
                Sort = new SieveSort
                {
                    Column = "createDateTime",
                    Dir = 1, // نزولی
                },
            };

            // فیلتر orderState
            if (query.OrderStateRawFilter is { Count: > 0 })
            {
                sieve.Filters.Add(new SieveFilter
                {
                    Column = "orderState",
                    Operator = 9, // In
                    Value = query.OrderStateRawFilter,
                });
            }

            // فیلتر symbolIsin
            if (query.SymbolIsins is { Count: > 0 })
            {
                sieve.Filters.Add(new SieveFilter
                {
                    Column = "symbolIsin",
                    Operator = 9,
                    Value = query.SymbolIsins,
                });
            }

            var payload = new OrderReportRequest
            {
                SieveModel = sieve,
                SearchTadbir = true,
            };

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOpts),
                Encoding.UTF8, "application/json");
            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Order report failed {(int)res.StatusCode}. Body: {Truncate(body, 400)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<OrderReportResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Order report deserialization failed");

            var items = (wire.Records ?? new())
                .Where(r => r.OrderReport is not null)
                .Select(r => MapOrderHistory(r))
                .ToList();

            return new PagedResult<OrderHistoryItemDto>
            {
                Items = items,
                TotalRecords = wire.TotalRecords,
                Page = query.Page,
                PageSize = query.PageSize,
            };
        }
        public async Task<List<OrderTradeDto>> GetOrderTradesAsync(
    BrokerSession session,
    string orderId,
    CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new EasyTraderException("orderId cannot be empty");

            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.OrderTrades;

            var client = CreateSharedClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);

            // ⚠️ order-id توی Header میاد، نه query string.
            // orderId ممکنه کاراکترهای خاص داشته باشه (%, @, [, ,) پس
            // از TryAddWithoutValidation استفاده می‌کنیم.
            req.Headers.TryAddWithoutValidation("order-id", orderId);

            AttachAuth(req, s.AccessToken);

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Order trades failed {(int)res.StatusCode} for orderId={orderId}. " +
                    $"Body: {Truncate(body, 300)}",
                    (int)res.StatusCode, body);

            // کارگزاری برای سفارش بدون fill، [] برمی‌گردونه — نرمال هست
            if (string.IsNullOrWhiteSpace(body))
                return new();

            var wire = JsonSerializer.Deserialize<List<OrderTradeItem>>(body, JsonOpts);
            if (wire is null) return new();

            return wire.Select(x => new OrderTradeDto
            {
                OrderId = x.Isr ?? "",
                TradeNumber = x.TradeNumber,
                InternalId = x.Id ?? "",
                RequestId = x.RequestId,
                CustomerIsin = x.CustomerIsin ?? "",
                SymbolIsin = x.Isin ?? "",
                Side = x.Side,
                Price = x.Price,
                Quantity = x.Quantity,
                Remain = x.Remain,
                HasRemain = x.HasRemain,
                TradeDate = TryParseDate(x.Date),
                CreateDateTime = TryParseDate(x.CreateDateTime),
                CancelDateTime = TryParseDateOrNull(x.CancelDateTime),
                IsCanceled = x.IsCanceled,
                Hon = x.Hon,
                Partition = x.Partition,
                ConsumerIndex = x.ConsumerIndex,
                Origin = x.Origin,
            }).ToList();
        }
        /* ══════════════════════════════════════════════════
           HELPERS
           ══════════════════════════════════════════════════ */
        /// <summary>
        /// یه HttpClient مشترک از factory می‌گیره.
        /// Authorization header رو روی خود request می‌ذاریم — نه روی client.
        /// </summary>
        private HttpClient CreateSharedClient()
        {
            var client = _httpFactory.CreateClient("EasyTrader");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/plain, */*");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "fa");
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
            return client;
        }

        /// <summary>اضافه کردن Authorization به HttpRequestMessage.</summary>
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

        private static string MakeAbsolute(string baseUrl, string pathOrUrl)
        {
            if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var abs))
                return abs.ToString();

            return baseUrl.TrimEnd('/') + "/" + pathOrUrl.TrimStart('/');
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

        private static string Truncate(string s, int max)
            => s.Length <= max ? s : s[..max] + "...";

        private static string FormatEasyTraderDateTime(DateTime d)
        {
            var h24 = d.Hour;
            var h12 = h24 % 12 == 0 ? 12 : h24 % 12;
            var ampm = h24 < 12 ? "AM" : "PM";
            return $"{d.Month}/{d.Day}/{d.Year}, {h12}:{d.Minute:D2}:{d.Second:D2} {ampm}";
        }
        /* helper برای پارس اعداد علمی مثل "1.05358e+008" */
        private static long ParseLong(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;

            if (long.TryParse(value, out var l)) return l;

            if (double.TryParse(value,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var d))
            {
                return (long)Math.Round(d);
            }

            return 0;
        }

        private static decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0m;
            if (decimal.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var d))
                return d;
            return 0m;
        }

        private static OrderHistoryItemDto MapOrderHistory(OrderReportRecord r)
        {
            var o = r.OrderReport!;
            return new OrderHistoryItemDto
            {
                OrderId = o.OrderId ?? "",
                InternalId = o.Id ?? "",
                ReferenceId = o.ReferenceId ?? "",
                ParentId = o.ParentId ?? "",
                CorrelationId = o.CorrelationId ?? "",
                CustomerIsin = o.CustomerIsin ?? "",
                SymbolIsin = o.SymbolIsin ?? "",
                SymbolName = o.SymbolName ?? "",
                Price = o.Price,
                MeanPrice = o.MeanPrice,
                Quantity = o.Quantity,
                ExecutedQuantity = o.ExecutedQuantity,
                OrderValue = o.OrderValue,
                Side = o.Side,
                OrderStateRaw = o.OrderState,
                State = EasyTraderOrderStateMap.ToKind(o.OrderState),
                CreateDateTime = TryParseDate(o.CreateDateTime),
                ModifyDateTime = TryParseDateOrNull(o.ModifyDate),
                ValidityDate = TryParseDateOrNull(o.ValidityDate),
                Validity = o.Validity,
                IgnoreCheckingMoney = o.IgnoreCheckingMoney,
                TraderCredit = o.TraderCredit,
                Error = o.Error,
                Sources = (r.OrderSources ?? new()).Select(src => new OrderSourceDto
                {
                    ActionType = src.ActionType,
                    CreateDateTime = TryParseDate(src.CreateDateTime),
                    Ip = src.Ip ?? "",
                    OrderFrom = src.OrderFrom,
                }).ToList(),
            };
        }

        /* helperهای کوچیک که احتمالاً بعداً زیاد به کار میان */
        private static DateTimeOffset TryParseDate(string? s)
            => DateTimeOffset.TryParse(s, out var d) ? d : default;

        private static DateTimeOffset? TryParseDateOrNull(string? s)
            => DateTimeOffset.TryParse(s, out var d) ? d : null;
    }
}