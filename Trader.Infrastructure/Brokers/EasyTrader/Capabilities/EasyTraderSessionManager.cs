using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Brokers;
using Trader.Application.Dtos;
using Trader.ExternalApiClient;
using Trader.ExternalApiClient.EasyTrader;
using Trader.Infrastructure.Brokers.EasyTrader.Internal;

namespace Trader.Infrastructure.Brokers.EasyTrader.Capabilities
{
    internal class EasyTraderSessionManager
        : EasyTraderCapabilityBase, IBrokerSessionManager
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public EasyTraderSessionManager(
            EasyTraderApiClient api,
            ILogger<EasyTraderSessionManager> logger)
            : base(api, logger) { }

        public string SerializeSession(ExternalSystemSession session)
        {
            if (session is not EasyTraderSession et)
                throw new ArgumentException(
                    $"Expected {nameof(EasyTraderSession)}", nameof(session));
            return JsonSerializer.Serialize(et, JsonOpts);
        }

        public ExternalSystemSession DeserializeSession(string json)
        {
            var session = JsonSerializer.Deserialize<EasyTraderSession>(json, JsonOpts)
                ?? throw new InvalidOperationException("Session deserialization returned null");
            return session;
        }

        public async Task<ExternalSystemSession> LoginAsync(
            string username,
            string password,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
                throw new EasyTraderException("Username/Password cannot be empty");

            // ─── PKCE setup ───
            var verifier = EasyTraderPkce.GenerateVerifier();
            var challenge = EasyTraderPkce.GenerateChallenge(verifier);
            var state = EasyTraderPkce.GenerateState();

            using var loginSession = Api.CreateLoginSession();

            try
            {
                /* ۱. GET /connect/authorize */
                var authorizeRes = await Api.AuthorizeAsync(loginSession, challenge, state, ct);
                if (authorizeRes.StatusCode is not (302 or 303))
                    throw new EasyTraderException(
                        $"Authorize expected 302/303, got {authorizeRes.StatusCode}",
                        authorizeRes.StatusCode, authorizeRes.Body);

                var loginPath = authorizeRes.Location
                    ?? throw new EasyTraderException("Authorize response has no Location");

                /* ۲. GET /Login */
                var loginPage = await Api.GetLoginPageAsync(loginSession, loginPath, ct);
                var antiForgeryToken = EasyTraderApiClient.ExtractAntiForgeryToken(loginPage.Html)
                    ?? throw new EasyTraderException("__RequestVerificationToken not found");

                /* ۳. POST /Login */
                var postLoginRes = await Api.PostLoginAsync(
                    loginSession, loginPage.LoginUrl, username, password, antiForgeryToken, ct);

                if (postLoginRes.StatusCode is not (302 or 303))
                    throw new EasyTraderException(
                        $"Login expected 302/303, got {postLoginRes.StatusCode}. " +
                        $"Body: {EasyTraderTextHelpers.Truncate(postLoginRes.Body, 200)}",
                        postLoginRes.StatusCode, postLoginRes.Body);

                var callbackPath = postLoginRes.Location
                    ?? throw new EasyTraderException("Login response has no Location");

                /* ۴. GET /connect/authorize/callback */
                var callbackRes = await Api.CallbackAsync(loginSession, callbackPath, ct);

                if (callbackRes.StatusCode is not (302 or 303))
                    throw new EasyTraderException(
                        $"Callback expected 302/303, got {callbackRes.StatusCode}",
                        callbackRes.StatusCode);

                if (string.IsNullOrEmpty(callbackRes.Code))
                    throw new EasyTraderException("No 'code' in callback URL");

                if (callbackRes.State != state)
                    throw new EasyTraderException("State mismatch — possible CSRF");

                /* ۵. POST /connect/token */
                var tokenResponse = await Api.ExchangeTokenAsync(
                    loginSession, callbackRes.Code, verifier, ct);

                if (string.IsNullOrEmpty(tokenResponse.AccessToken))
                    throw new EasyTraderException("access_token missing in response");

                Logger.LogInformation(
                    "EasyTrader login successful, expiresIn={Expires}s",
                    tokenResponse.ExpiresIn);

                /* ۶. POST /same-login (activation) */
                await Api.SameLoginAsync(tokenResponse.AccessToken, ct);

                var exp = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                return new EasyTraderSession(tokenResponse.AccessToken, exp);
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, "EasyTrader login failed: network/SSL error");
                throw;
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                Logger.LogError(ex, "EasyTrader login timed out");
                throw new EasyTraderException("EasyTrader login timed out", ex);
            }
        }

        public async Task<BrokerTimeMeasurement> MeasureLatencyAsync(
            ExternalSystemSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var clientTs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var response = await Api.GetServerTimeAsync(s.AccessToken, clientTs, ct);
            sw.Stop();

            var rtt = sw.ElapsedMilliseconds;

            return new BrokerTimeMeasurement(
                Diff: response.Diff,
                OneWayLatencyMs: rtt / 2,
                RttMs: rtt);
        }
    }
}