using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Commands;
using Trader.Application.Dtos;
using Trader.Application.Dtos.Order;
using Trader.ExternalApiClient;
using Trader.ExternalApiClient.EasyTrader;
using Trader.Infrastructure.Brokers.EasyTrader.Internal;

namespace Trader.Infrastructure.Brokers.EasyTrader.Capabilities
{
    internal class EasyTraderTradingCommands
        : EasyTraderCapabilityBase, IBrokerTradingCommands
    {
        public EasyTraderTradingCommands(
            EasyTraderApiClient api,
            ILogger<EasyTraderTradingCommands> logger)
            : base(api, logger) { }

        public Task<BrokerOrderResultDto> SendBuyOrderAsync(
            ExternalSystemSession session, string symbolIsin, long price, long quantity,
            CancellationToken ct = default)
            => SendOrderLegacyAsync(session, symbolIsin, price, quantity, side: 0, ct);

        public Task<BrokerOrderResultDto> SendSellOrderAsync(
            ExternalSystemSession session, string symbolIsin, long price, long quantity,
            CancellationToken ct = default)
            => SendOrderLegacyAsync(session, symbolIsin, price, quantity, side: 1, ct);

        private async Task<BrokerOrderResultDto> SendOrderLegacyAsync(
            ExternalSystemSession session, string symbolIsin, long price, long quantity,
            int side, CancellationToken ct)
        {
            var request = BuildOrderRequest(session, symbolIsin, price, quantity, side);
            return await SendOrderWithRequestAsync(session, request, symbolIsin, ct);
        }

        public HttpRequestMessage BuildOrderRequest(
            ExternalSystemSession session,
            string symbolIsin,
            long price,
            long quantity,
            int side)
        {
            var s = AsSession(session);
            return Api.BuildOrderRequest(s.AccessToken, symbolIsin, price, quantity, side);
        }

        public async Task<BrokerOrderResultDto> SendOrderWithRequestAsync(
            ExternalSystemSession session,
            HttpRequestMessage request,
            string symbolIsin,
            CancellationToken ct = default)
        {
            // session فقط برای یکنواختی امضا؛ auth داخل خود request هست
            _ = AsSession(session);

            var result = await Api.SendOrderAsync(request, symbolIsin, ct);
            var err = result.Response.OmsError?.FirstOrDefault();

            return new BrokerOrderResultDto
            {
                IsSuccessful = result.Response.IsSuccessful,
                OrderId = result.Response.Id,
                Message = result.Response.Message,
                ErrorCode = err?.Code,
                ErrorName = err?.Name,
                FireAtUnixMs = result.FireAtUnixMs,
                ReceivedAtUnixMs = result.ReceivedAtUnixMs,
            };
        }
    }
}