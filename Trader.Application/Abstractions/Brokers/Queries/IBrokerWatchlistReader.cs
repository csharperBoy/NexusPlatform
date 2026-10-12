using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Application.Dtos.MarketData;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Queries
{
        public interface IBrokerWatchlistReader : IBroker
        {
            Task<List<MarketWatchCategoryDto>> GetMarketWatchAsync(
                ExternalSystemSession session, CancellationToken ct = default);

            Task<List<IndustryPositiveNegativeDto>> GetIndustryPositiveNegativeAsync(
                ExternalSystemSession session, CancellationToken ct = default);
        }
    
}
