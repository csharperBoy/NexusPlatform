using Trader.Application.Dtos.Order;
using Trader.Domain.Enums;
using Trader.ExternalApiClient.EasyTrader;

namespace Trader.Infrastructure.Brokers.EasyTrader.Internal
{
    internal static class EasyTraderOrderHistoryMapper
    {
        public static OrderHistoryItemDto Map(OrderReportRecord r)
        {
            var o = r.OrderReport!;
            return new OrderHistoryItemDto
            {
                OrderId = o.OrderId ?? "",
                InternalId = o.Id ?? "",
                ReferenceId = o.ReferenceId ?? "",
                ParentId = o.ParentId ?? "",
                CorrelationId = o.CorrelationId ?? "",
                CustomerIsin = o.CustomerIsin ?? "",
                SymbolIsin = o.SymbolIsin ?? "",
                SymbolName = o.SymbolName ?? "",
                Price = o.Price,
                MeanPrice = o.MeanPrice,
                Quantity = o.Quantity,
                ExecutedQuantity = o.ExecutedQuantity,
                OrderValue = o.OrderValue,
                Side = o.Side,
                OrderStateRaw = o.OrderState,
                State = OrderStateKindExtensions.FromEasyTrader(o.OrderState),
                CreateDateTime = EasyTraderDateParsers.ParseOrDefault(o.CreateDateTime),
                ModifyDateTime = EasyTraderDateParsers.ParseOrNull(o.ModifyDate),
                ValidityDate = EasyTraderDateParsers.ParseOrNull(o.ValidityDate),
                Validity = o.Validity,
                IgnoreCheckingMoney = o.IgnoreCheckingMoney,
                TraderCredit = o.TraderCredit,
                Error = o.Error,
                Sources = (r.OrderSources ?? new()).Select(src => new OrderSourceDto
                {
                    ActionType = src.ActionType,
                    CreateDateTime = EasyTraderDateParsers.ParseOrDefault(src.CreateDateTime),
                    Ip = src.Ip ?? "",
                    OrderFrom = src.OrderFrom,
                }).ToList(),
            };
        }
    }
}