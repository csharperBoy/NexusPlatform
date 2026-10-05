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
    
    public class EasyTraderApiClient
    {/*

        private readonly EasyTraderOptions _options;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<EasyTraderBrokerClient> _logger;
        public EasyTraderApiClient(IOptions<EasyTraderOptions> options,
            IHttpClientFactory httpFactory,
            ILogger<EasyTraderOptions> logger)
        {

            _options = options.Value;
        }
        public async Task GET_connect_authorize()
        {

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

            var authorizeUrl = BuildAuthorizeUrl(challenge, state);
            _logger.LogDebug("EasyTrader OIDC step 1: authorize");

            using var authorizeRes = await client.GetAsync(authorizeUrl, ct);
            if ((int)authorizeRes.StatusCode is not (302 or 303))
                throw new EasyTraderException(
                    $"Authorize expected 302/303, got {(int)authorizeRes.StatusCode}",
                    (int)authorizeRes.StatusCode);

            var loginPath = authorizeRes.Headers.Location?.ToString()
                ?? throw new EasyTraderException("Authorize response has no Location");
        }*/
    }

}
