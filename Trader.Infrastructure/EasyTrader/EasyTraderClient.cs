using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using Trader.Application.Abstractions;
using Trader.Application.EasyTrader.Exceptions;
using Trader.Application.EasyTrader.Models;

namespace Trader.Infrastructure.EasyTrader
{
    public class EasyTraderClient : IEasyTraderClient
    {
        private readonly EasyTraderOptions _options;
        private readonly ILogger<EasyTraderClient> _logger;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public EasyTraderClient(
            IOptions<EasyTraderOptions> options,
            ILogger<EasyTraderClient> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /* ══════════════════════════════════════════════════
           LOGIN — OIDC + PKCE + Activation
           ══════════════════════════════════════════════════ */
        public async Task<LoginResult> LoginAsync(
            string username,
            string password,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
                throw new EasyTraderException("Username/Password cannot be empty");

            var verifier = PkceGenerator.GenerateVerifier();
            var challenge = PkceGenerator.GenerateChallenge(verifier);
            var state = PkceGenerator.GenerateState();

            // CookieContainer محلی برای این login
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
            _logger.LogDebug("OIDC step 1: GET authorize");

            using var authorizeRes = await client.GetAsync(authorizeUrl, ct);
            if ((int)authorizeRes.StatusCode is not (302 or 303))
                throw new EasyTraderException(
                    $"Authorize expected 302/303, got {(int)authorizeRes.StatusCode}",
                    (int)authorizeRes.StatusCode);

            /* ─── ۲. GET /Login (صفحه‌ی لاگین) ─── */
            var loginPath = authorizeRes.Headers.Location?.ToString()
                ?? throw new EasyTraderException("Authorize response has no Location header");
            var loginUrl = MakeAbsolute(_options.OidcBaseUrl, loginPath);

            _logger.LogDebug("OIDC step 2: GET login page");

            using var loginPageRes = await client.GetAsync(loginUrl, ct);
            if (!loginPageRes.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Login page returned {(int)loginPageRes.StatusCode}",
                    (int)loginPageRes.StatusCode);

            var loginHtml = await loginPageRes.Content.ReadAsStringAsync(ct);
            var verificationToken = ExtractAntiForgeryToken(loginHtml)
                ?? throw new EasyTraderException(
                    "__RequestVerificationToken not found in login page");

            /* ─── ۳. POST /Login با credentials ─── */
            _logger.LogDebug("OIDC step 3: POST login credentials");

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
                    $"Login POST expected 302/303, got {(int)loginRes.StatusCode}. " +
                    $"Body: {Truncate(body, 200)}",
                    (int)loginRes.StatusCode,
                    body);
            }

            /* ─── ۴. GET /connect/authorize/callback ─── */
            var callbackPath = loginRes.Headers.Location?.ToString()
                ?? throw new EasyTraderException("Login response has no Location header");
            var callbackUrl = MakeAbsolute(_options.OidcBaseUrl, callbackPath);

            _logger.LogDebug("OIDC step 4: GET authorize callback");

            using var callbackRes = await client.GetAsync(callbackUrl, ct);
            if ((int)callbackRes.StatusCode is not (302 or 303))
                throw new EasyTraderException(
                    $"Authorize callback expected 302/303, got {(int)callbackRes.StatusCode}",
                    (int)callbackRes.StatusCode);

            /* ─── ۵. استخراج code از redirect نهایی ─── */
            var finalLocation = callbackRes.Headers.Location?.ToString()
                ?? throw new EasyTraderException("Callback response has no Location header");

            var codeParams = ParseQueryString(new Uri(finalLocation).Query);
            codeParams.TryGetValue("code", out var code);
            codeParams.TryGetValue("state", out var returnedState);

            if (string.IsNullOrEmpty(code))
                throw new EasyTraderException("No 'code' in final callback URL");

            if (returnedState != state)
                throw new EasyTraderException("State mismatch — possible CSRF");

            _logger.LogDebug("OIDC step 5: got authorization code");

            /* ─── ۶. POST /connect/token ─── */
            _logger.LogDebug("OIDC step 6: POST token exchange");

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
                    $"Token exchange failed {(int)tokenRes.StatusCode}. Body: {Truncate(body, 300)}",
                    (int)tokenRes.StatusCode,
                    body);
            }

            var tokenJson = await tokenRes.Content.ReadAsStringAsync(ct);
            using var tokenDoc = JsonDocument.Parse(tokenJson);
            var root = tokenDoc.RootElement;

            var accessToken = root.GetProperty("access_token").GetString()
                ?? throw new EasyTraderException("access_token missing in token response");
            var idToken = root.TryGetProperty("id_token", out var idEl)
                ? idEl.GetString() : null;
            var expiresIn = root.TryGetProperty("expires_in", out var expEl)
                ? expEl.GetInt32() : 43200;
            var scope = root.TryGetProperty("scope", out var scopeEl)
                ? scopeEl.GetString() ?? "" : "";

            _logger.LogInformation(
                "OIDC login successful, expiresIn={Expires}s", expiresIn);

            /* ─── ۷. Activate token (same-login) ─── */
            await ActivateTokenInternalAsync(client, accessToken, ct);

            return new LoginResult(accessToken, idToken, expiresIn, scope);
        }

        /* ══════════════════════════════════════════════════
           ACTIVATE TOKEN — same-login
           ══════════════════════════════════════════════════ */
        public async Task ActivateTokenAsync(
            string accessToken,
            CancellationToken ct = default)
        {
            using var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true,
            };
            using var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds),
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);

            await ActivateTokenInternalAsync(client, accessToken, ct);
        }

        private async Task ActivateTokenInternalAsync(
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
                _logger.LogDebug("Token activated successfully");
                return;
            }

            // 400 با پیام "already logged in" → قبلاً فعال شده
            if ((int)res.StatusCode == 400 &&
                resBody.Contains("already logged in", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Token already active, skipping activation");
                return;
            }

            throw new EasyTraderException(
                $"Token activation failed {(int)res.StatusCode}. Body: {Truncate(resBody, 300)}",
                (int)res.StatusCode,
                resBody);
        }

        /* ══════════════════════════════════════════════════
           SERVER TIME
           ══════════════════════════════════════════════════ */
        public async Task<ServerTimeResult> GetServerTimeAsync(
            string accessToken,
            CancellationToken ct = default)
        {
            var clientTs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var path = string.Format(EasyTraderEndpoints.ServerTime, clientTs);
            var url = _options.BaseUrl + path;

            using var client = CreateAuthorizedClient(accessToken);
            var res = await client.GetAsync(url, ct);
            var body = await res.Content.ReadAsStringAsync(ct);
            sw.Stop();

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Server-time failed {(int)res.StatusCode}. Body: {Truncate(body, 200)}",
                    (int)res.StatusCode, body);

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            var diff = root.GetProperty("diff").GetInt64();
            var serverTs = root.GetProperty("serverTimestamp").GetInt64();

            return new ServerTimeResult(
                ClientTimestamp: clientTs,
                ServerTimestamp: serverTs,
                Diff: diff,
                Rtt: sw.ElapsedMilliseconds);
        }

        /* ══════════════════════════════════════════════════
           SYMBOL INFO
           ══════════════════════════════════════════════════ */
        public async Task<MarketSymbolInfoResult> GetSymbolInfoAsync(
            string accessToken,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.SymbolInfo;

            using var client = CreateAuthorizedClient(accessToken);
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new { isin = symbolIsin }),
                Encoding.UTF8, "application/json");

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new EasyTraderException(
                    $"Symbol info failed {(int)res.StatusCode}. Body: {Truncate(body, 200)}",
                    (int)res.StatusCode, body);

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            return new MarketSymbolInfoResult(
                SymbolIsin: GetStringOrNull(root, "symbolISIN") ?? symbolIsin,
                HighAllowedPrice: GetLongOrNull(root, "highAllowedPrice"),
                LowAllowedPrice: GetLongOrNull(root, "lowAllowedPrice"),
                LastTradedPrice: GetLongOrNull(root, "lastTradedPrice"),
                ClosingPrice: GetLongOrNull(root, "closingPrice"),
                FirstTradedPrice: GetLongOrNull(root, "firstTradedPrice"),
                TradeDate: GetStringOrNull(root, "tradeDate"),
                FetchedAt: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        /* ══════════════════════════════════════════════════
           SEND ORDER
           ══════════════════════════════════════════════════ */
        public async Task<OrderResult> SendOrderAsync(
            string accessToken,
            OrderPayload payload,
            CancellationToken ct = default)
        {
            var url = _options.BaseUrl + EasyTraderEndpoints.Order;

            using var client = CreateAuthorizedClient(accessToken);
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOpts),
                Encoding.UTF8, "application/json");

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
            {
                // بعضی وقتا خطاها هم به شکل OrderResult میان
                if (TryParseOrderResult(body, out var parsedError))
                    return parsedError;

                throw new EasyTraderException(
                    $"Order failed {(int)res.StatusCode}. Body: {Truncate(body, 400)}",
                    (int)res.StatusCode, body);
            }

            if (!TryParseOrderResult(body, out var parsed))
                throw new EasyTraderException(
                    $"Unexpected order response: {Truncate(body, 400)}",
                    (int)res.StatusCode, body);

            return parsed;
        }

        private static bool TryParseOrderResult(string body, out OrderResult result)
        {
            result = default!;
            if (string.IsNullOrWhiteSpace(body)) return false;

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                var isSuccessful = root.TryGetProperty("isSuccessful", out var okEl)
                    && okEl.ValueKind == JsonValueKind.True;

                var orderId = GetStringOrNull(root, "id");
                var message = GetStringOrNull(root, "message");

                OmsError? omsError = null;
                if (root.TryGetProperty("omsError", out var errEl) &&
                    errEl.ValueKind == JsonValueKind.Array &&
                    errEl.GetArrayLength() > 0)
                {
                    var first = errEl[0];
                    omsError = new OmsError(
                        Code: GetIntOrNull(first, "code") ?? 0,
                        Name: GetStringOrNull(first, "name") ?? "",
                        Message: GetStringOrNull(first, "error") ?? "");
                }

                result = new OrderResult(isSuccessful, orderId, message, omsError);
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

        private static string ExtractAntiForgeryToken(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var node = doc.DocumentNode
                .SelectSingleNode("//input[@name='__RequestVerificationToken']");

            return node?.GetAttributeValue("value", string.Empty) ?? string.Empty;
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

        private static string? GetStringOrNull(JsonElement el, string name)
            => el.TryGetProperty(name, out var v) && v.ValueKind != JsonValueKind.Null
                ? v.GetString() : null;

        private static long? GetLongOrNull(JsonElement el, string name)
        {
            if (!el.TryGetProperty(name, out var v)) return null;
            if (v.ValueKind == JsonValueKind.Null) return null;
            if (v.ValueKind == JsonValueKind.Number) return v.GetInt64();
            if (v.ValueKind == JsonValueKind.String &&
                long.TryParse(v.GetString(), out var parsed)) return parsed;
            return null;
        }

        private static int? GetIntOrNull(JsonElement el, string name)
        {
            if (!el.TryGetProperty(name, out var v)) return null;
            if (v.ValueKind == JsonValueKind.Null) return null;
            if (v.ValueKind == JsonValueKind.Number) return v.GetInt32();
            return null;
        }
    }
}