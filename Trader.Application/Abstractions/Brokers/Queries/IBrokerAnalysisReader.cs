using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Application.Dtos.MarketData;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Queries
{
        public interface IBrokerAnalysisReader : IBroker
        {
            Task<TechnicalAnalysisDto> GetTechnicalAnalysisAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

            Task<FundamentalAnalysisDto> GetFundamentalAnalysisAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

            Task<IndInstTradeDto> GetIndInstTradeAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

            Task<IndInstAnalysisDto> GetIndInstAnalysisAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

            Task<List<IndTradingTrendDto>> GetIndTradingTrendAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);
        }
    
}
