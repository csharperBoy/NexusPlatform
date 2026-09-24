using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions;
using Trader.Application.Brokers;
using Trader.Application.Dtos;
using Trader.Domain.Enums;
using Trader.Infrastructure.Brokers.EasyTrader.Internal;

namespace Trader.Infrastructure.Brokers.EasyTrader
{
    public class EasyTraderBrokerClient : IBrokerClient
    {
        private readonly EasyTraderOptions _options;
        private readonly ILogger<EasyTraderBrokerClient> _logger;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public EasyTraderBrokerClient(
            IOptions<EasyTraderOptions> options,
            ILogger<EasyTraderBrokerClient> logger)
        {
            _options = options.Value;
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
        public async Task<long> MeasureLatencyAsync(
            BrokerSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var clientTs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var path = string.Format(EasyTraderEndpoints.ServerTime, clientTs);
            var url = _options.BaseUrl + path;

            using var client = CreateAuthorizedClient(s.AccessToken);
            var res = await client.GetAsync(url, ct);
            var body = await res.Content.ReadAsStringAsync(ct);
            sw.Stop();

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Server-time failed {(int)res.StatusCode}. " +
                    $"Body: {Truncate(body, 200)}",
                    (int)res.StatusCode, body);

            var response = JsonSerializer.Deserialize<ServerTimeResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Server-time deserialization failed");

            // diff = serverTs - clientTs. RTT/2 تخمین یک‌طرفه.
            return sw.ElapsedMilliseconds / 2;
        }

        /* ══════════════════════════════════════════════════
           SYMBOL INFO
           ══════════════════════════════════════════════════ */
        public async Task<SymbolMarketDataDto> GetSymbolInfoAsync(
            BrokerSession session,
            string symbolName,
            CancellationToken ct = default)
        {
            // توجه: در EasyTrader، GetSymbolInfo نیاز به ISIN داره نه name.
            // چون قرارداد عمومی با name کار می‌کنه، اینجا از name به isin map می‌کنیم.
            // این mapping باید از DB یا یه سرویس بیرونی بیاد. فعلاً فرض: name == isin.
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.SymbolInfo;

            using var client = CreateAuthorizedClient(s.AccessToken);
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new { isin = symbolName }, JsonOpts),
                Encoding.UTF8, "application/json");

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Symbol info failed {(int)res.StatusCode}. " +
                    $"Body: {Truncate(body, 200)}",
                    (int)res.StatusCode, body);

            var wire = JsonSerializer.Deserialize<SymbolInfoResponse>(body, JsonOpts)
                ?? throw new EasyTraderException("Symbol info deserialization failed");

            return new SymbolMarketDataDto
            {
                SymbolName = symbolName,
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
            BrokerSession session, string symbolName, long price, long quantity,
            CancellationToken ct = default)
            => SendOrderInternalAsync(session, symbolName, price, quantity, side: 0, ct);

        public Task<BrokerOrderResultDto> SendSellOrderAsync(
            BrokerSession session, string symbolName, long price, long quantity,
            CancellationToken ct = default)
            => SendOrderInternalAsync(session, symbolName, price, quantity, side: 1, ct);

        private async Task<BrokerOrderResultDto> SendOrderInternalAsync(
            BrokerSession session,
            string symbolName,
            long price,
            long quantity,
            int side,
            CancellationToken ct)
        {
            var s = AsSession(session);
            var url = _options.BaseUrl + EasyTraderEndpoints.Order;

            // این‌ها از تنظیمات پیش‌فرض EasyTrader میان.
            // در آینده می‌تونن per-symbol از DB بیان.
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
                    SymbolIsin = symbolName,   // فرض: name == isin
                    SymbolName = symbolName,
                    OrderModelType = orderModelType,
                    TotalValue = totalValue,
                    OrderFrom = orderFrom,
                }
            };

            using var client = CreateAuthorizedClient(s.AccessToken);
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOpts),
                Encoding.UTF8, "application/json");

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            // بعضی وقتا خطاهای منطقی هم با 200 میان
            if (TryParseOrderResponse(body, out var parsed))
                return parsed;

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Order failed {(int)res.StatusCode}. " +
                    $"Body: {Truncate(body, 400)}",
                    (int)res.StatusCode, body);

            throw new EasyTraderException(
                $"Unexpected order response: {Truncate(body, 400)}",
                (int)res.StatusCode, body);
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

        /* ══════════════════════════════════════════════════
           HELPERS
           ══════════════════════════════════════════════════ */
        private HttpClient CreateAuthorizedClient(string accessToken)
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds),
            };
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/plain, */*");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "fa");
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
            return client;
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
    }
}