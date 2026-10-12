using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Application.Dtos;
using Trader.Application.Dtos.MarketData;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Queries
{
        public interface IBrokerMarketDataReader : IBroker
        {
            Task<SymbolMarketDataDto> GetSymbolInfoAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

            Task<List<CandleDto>> GetCandlesAsync(
                ExternalSystemSession session, string symbolIsin,
                int days = 1, int intervalMinutes = 1, CancellationToken ct = default);

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
