using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Application.Dtos;
using Trader.Application.Dtos.Order;
using Trader.ExternalApiClient;
using Trader.ExternalApiClient.EasyTrader;
using Trader.Infrastructure.Brokers.EasyTrader.Internal;

namespace Trader.Infrastructure.Brokers.EasyTrader.Capabilities
{
    internal class EasyTraderOrderHistoryReader
        : EasyTraderCapabilityBase, IBrokerOrderHistoryReader
    {
        public EasyTraderOrderHistoryReader(
            EasyTraderApiClient api,
            ILogger<EasyTraderOrderHistoryReader> logger)
            : base(api, logger) { }

        public async Task<PagedResult<OrderHistoryItemDto>> GetOrderHistoryAsync(
            ExternalSystemSession session,
            OrderHistoryQuery query,
            CancellationToken ct = default)
        {
            var s = AsSession(session);

            var sieve = new SieveRequest
            {
                Page = query.Page,
                PageSize = query.PageSize,
                Sort = new SieveSort { Column = "createDateTime", Dir = 1 },
            };

            if (query.OrderStateRawFilter is { Count: > 0 })
            {
                sieve.Filters.Add(new SieveFilter
                {
                    Column = "orderState",
                    Operator = 9,
                    Value = query.OrderStateRawFilter,
                });
            }

            if (query.SymbolIsins is { Count: > 0 })
            {
                sieve.Filters.Add(new SieveFilter
                {
                    Column = "symbolIsin",
                    Operator = 9,
                    Value = query.SymbolIsins,
                });
            }

            var payload = new OrderReportRequest
            {
                SieveModel = sieve,
                SearchTadbir = true,
            };

            var wire = await Api.GetOrderReportAsync(s.AccessToken, payload, ct);

            var items = (wire.Records ?? new())
                .Where(r => r.OrderReport is not null)
                .Select(EasyTraderOrderHistoryMapper.Map)
                .ToList();

            return new PagedResult<OrderHistoryItemDto>
            {
                Items = items,
                TotalRecords = wire.TotalRecords,
                Page = query.Page,
                PageSize = query.PageSize,
            };
        }

        public async Task<List<OrderTradeDto>> GetOrderTradesAsync(
            ExternalSystemSession session,
            string orderId,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetOrderTradesAsync(s.AccessToken, orderId, ct);

            return wire.Select(x => new OrderTradeDto
            {
                OrderId = x.Isr ?? "",
                TradeNumber = x.TradeNumber,
                InternalId = x.Id ?? "",
                RequestId = x.RequestId,
                CustomerIsin = x.CustomerIsin ?? "",
                SymbolIsin = x.Isin ?? "",
                Side = x.Side,
                Price = x.Price,
                Quantity = x.Quantity,
                Remain = x.Remain,
                HasRemain = x.HasRemain,
                TradeDate = EasyTraderDateParsers.ParseOrDefault(x.Date),
                CreateDateTime = EasyTraderDateParsers.ParseOrDefault(x.CreateDateTime),
                CancelDateTime = EasyTraderDateParsers.ParseOrNull(x.CancelDateTime),
                IsCanceled = x.IsCanceled,
                Hon = x.Hon,
                Partition = x.Partition,
                ConsumerIndex = x.ConsumerIndex,
                Origin = x.Origin,
            }).ToList();
        }
    }
}