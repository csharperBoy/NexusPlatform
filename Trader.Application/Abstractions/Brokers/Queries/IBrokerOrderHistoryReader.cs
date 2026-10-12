using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Application.Dtos;
using Trader.Application.Dtos.Order;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Queries
{
        public interface IBrokerOrderHistoryReader : IBroker
        {
            Task<PagedResult<OrderHistoryItemDto>> GetOrderHistoryAsync(
                ExternalSystemSession session, OrderHistoryQuery query,
                CancellationToken ct = default);

            Task<List<OrderTradeDto>> GetOrderTradesAsync(
                ExternalSystemSession session, string orderId,
                CancellationToken ct = default);
        }
    
}
