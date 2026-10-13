using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Application.Dtos;
using Trader.Application.Dtos.MarketData;
using Trader.ExternalApiClient;
using Trader.ExternalApiClient.EasyTrader;
using Trader.Infrastructure.Brokers.EasyTrader.Internal;

namespace Trader.Infrastructure.Brokers.EasyTrader.Capabilities
{
    internal class EasyTraderMarketDataReader
        : EasyTraderCapabilityBase, IBrokerMarketDataReader
    {
        public EasyTraderMarketDataReader(
            EasyTraderApiClient api,
            ILogger<EasyTraderMarketDataReader> logger)
            : base(api, logger) { }

        public async Task<SymbolMarketDataDto> GetSymbolInfoAsync(
            ExternalSystemSession session, string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetSymbolInfoAsync(s.AccessToken, symbolIsin, ct);

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

        public async Task<List<CandleDto>> GetCandlesAsync(
            ExternalSystemSession session, string symbolIsin,
            int days = 1, int intervalMinutes = 1, CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetChartHistoryAsync(
                s.AccessToken, symbolIsin, days, intervalMinutes, ct);

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

        public async Task<ReturnChartDto> GetReturnChartAsync(
            ExternalSystemSession session, string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetReturnChartDataAsync(s.AccessToken, symbolIsin, ct);

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

        public async Task<MarketSheetSumDto> GetMarketSheetSumAsync(
            ExternalSystemSession session, string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetMarketSheetSumAsync(s.AccessToken, symbolIsin, ct);

            return new MarketSheetSumDto
            {
                BuyVolume = wire.BuyVolume,
                BuyCount = wire.BuyCount,
                SellVolume = wire.SellVolume,
                SellCount = wire.SellCount,
            };
        }

        public async Task<List<BatchMarketDataItemDto>> GetBatchMarketDataAsync(
            ExternalSystemSession session,
            IReadOnlyList<string> symbolIsins,
            CancellationToken ct = default)
        {
            if (symbolIsins is null || symbolIsins.Count == 0)
                return new();

            var s = AsSession(session);
            var wire = await Api.GetMarketDataGraphQLAsync(s.AccessToken, symbolIsins, ct);

            return wire.Select(x => new BatchMarketDataItemDto
            {
                SymbolIsin = x.SymbolIsin ?? "",
                StateCode = x.StateCode ?? "",
                LastTradedPrice = x.LastTradedPrice,
                ClosingPrice = x.ClosingPrice,
                FeeOfPreviousDaysClosingPrice = x.FeeOfPreviousDaysClosingPrice,
                PriceVar = x.PriceVar,
                BuyRatio = EasyTraderNumericParsers.ParseDecimal(x.BuyRatio),
                SellRatio = EasyTraderNumericParsers.ParseDecimal(x.SellRatio),
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

        public async Task<List<TradingTimeDto>> GetTradingTimesAsync(
            ExternalSystemSession session, CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetTradingTimeAsync(s.AccessToken, ct);

            if (wire.Items is null) return new();

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

        public async Task<List<TseIndexDto>> GetTseIndexAsync(
            ExternalSystemSession session, CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetTseIndexAsync(s.AccessToken, ct);

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
    }
}