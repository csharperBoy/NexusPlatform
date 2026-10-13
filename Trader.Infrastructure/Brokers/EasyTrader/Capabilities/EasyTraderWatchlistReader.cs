using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Application.Dtos.MarketData;
using Trader.ExternalApiClient;
using Trader.ExternalApiClient.EasyTrader;
using Trader.Infrastructure.Brokers.EasyTrader.Internal;

namespace Trader.Infrastructure.Brokers.EasyTrader.Capabilities
{
    internal class EasyTraderWatchlistReader
        : EasyTraderCapabilityBase, IBrokerWatchlistReader
    {
        public EasyTraderWatchlistReader(
            EasyTraderApiClient api,
            ILogger<EasyTraderWatchlistReader> logger)
            : base(api, logger) { }

        public async Task<List<MarketWatchCategoryDto>> GetMarketWatchAsync(
            ExternalSystemSession session, CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetMarketWatchAsync(s.AccessToken, ct);

            return wire.Select(x => new MarketWatchCategoryDto
            {
                Id = (x.Id ?? "").Trim(),
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

        public async Task<List<IndustryPositiveNegativeDto>> GetIndustryPositiveNegativeAsync(
            ExternalSystemSession session, CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetPositiveNegativeSymbolsAsync(s.AccessToken, ct);

            return wire.Select(x => new IndustryPositiveNegativeDto
            {
                IndustryCode = x.IndustryCode ?? "",
                IndustryName = x.IndustryName ?? "",
                NegativeCount = x.NegativeCount,
                ZeroCount = x.ZeroCount,
                PositiveCount = x.PositiveCount,
            }).ToList();
        }
    }
}