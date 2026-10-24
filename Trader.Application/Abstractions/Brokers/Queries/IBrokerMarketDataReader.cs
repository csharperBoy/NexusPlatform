using Trader.Application.Dtos;
using Trader.Application.Dtos.MarketData;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Queries
{
        public interface IBrokerMarketDataReader : IBroker
        {
            Task<SymbolMarketDataDto> GetSymbolInfoAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);


        /// <summary>
        /// دریافت Candle از Broker.
        /// resolution: "1", "5", "15", "30", "60", "1D", "1W", "1M"
        /// countback: تعداد کندل درخواستی.
        /// </summary>
        Task<List<CandleDto>> GetCandlesAsync(
            ExternalSystemSession session,
            string symbolIsin,
            string resolution,
            int countback,
            CancellationToken ct = default);

            Task<List<BatchMarketDataItemDto>> GetBatchMarketDataAsync(
                ExternalSystemSession session, IReadOnlyList<string> symbolIsins,
                CancellationToken ct = default);

            Task<List<TseIndexDto>> GetTseIndexAsync(
                ExternalSystemSession session, CancellationToken ct = default);

            Task<MarketSheetSumDto> GetMarketSheetSumAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

            Task<ReturnChartDto> GetReturnChartAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

            Task<List<TradingTimeDto>> GetTradingTimesAsync(
                ExternalSystemSession session, CancellationToken ct = default);
        }
    
}
