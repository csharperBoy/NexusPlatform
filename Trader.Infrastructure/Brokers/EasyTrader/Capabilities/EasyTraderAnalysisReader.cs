using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Application.Dtos.MarketData;
using Trader.ExternalApiClient;
using Trader.ExternalApiClient.EasyTrader;
using Trader.Infrastructure.Brokers.EasyTrader.Internal;

namespace Trader.Infrastructure.Brokers.EasyTrader.Capabilities
{
    internal class EasyTraderAnalysisReader
        : EasyTraderCapabilityBase, IBrokerAnalysisReader
    {
        public EasyTraderAnalysisReader(
            EasyTraderApiClient api,
            ILogger<EasyTraderAnalysisReader> logger)
            : base(api, logger) { }

        public async Task<TechnicalAnalysisDto> GetTechnicalAnalysisAsync(
            ExternalSystemSession session, string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetTechnicalAnalysisAsync(s.AccessToken, symbolIsin, ct);

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

        public async Task<IndInstTradeDto> GetIndInstTradeAsync(
            ExternalSystemSession session, string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetIndInstTradeAsync(s.AccessToken, symbolIsin, ct);

            return new IndInstTradeDto
            {
                SymbolIsin = wire.SymbolISIN,
                IndBuyVolume = EasyTraderNumericParsers.ParseLong(wire.IndBuyVolume),
                IndBuyNumber = EasyTraderNumericParsers.ParseLong(wire.IndBuyNumber),
                IndSellVolume = EasyTraderNumericParsers.ParseLong(wire.IndSellVolume),
                IndSellNumber = EasyTraderNumericParsers.ParseLong(wire.IndSellNumber),
                InsBuyVolume = EasyTraderNumericParsers.ParseLong(wire.InsBuyVolume),
                InsBuyNumber = EasyTraderNumericParsers.ParseLong(wire.InsBuyNumber),
                InsSellVolume = EasyTraderNumericParsers.ParseLong(wire.InsSellVolume),
                InsSellNumber = EasyTraderNumericParsers.ParseLong(wire.InsSellNumber),
                Date = wire.Date,
            };
        }

        public async Task<IndInstAnalysisDto> GetIndInstAnalysisAsync(
            ExternalSystemSession session, string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetIndInstAnalysisAsync(s.AccessToken, symbolIsin, ct);

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

        public async Task<List<IndTradingTrendDto>> GetIndTradingTrendAsync(
            ExternalSystemSession session, string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var list = await Api.GetIndTradingTrendAsync(s.AccessToken, symbolIsin, ct);

            return list.Select(x => new IndTradingTrendDto
            {
                Type = x.Type ?? "",
                Date = x.Date ?? "",
                Val = x.Val,
            }).ToList();
        }

        public async Task<FundamentalAnalysisDto> GetFundamentalAnalysisAsync(
            ExternalSystemSession session, string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetFundamentalAnalysisAsync(s.AccessToken, symbolIsin, ct);

            return new FundamentalAnalysisDto
            {
                TotalScore = wire.Total?.Score ?? 0,
                Categories = (wire.Item ?? new()).Select(x => new FundamentalCategoryScoreDto
                {
                    Title = x.Title ?? "",
                    Score = x.Score,
                }).ToList(),
            };
        }
    }
}