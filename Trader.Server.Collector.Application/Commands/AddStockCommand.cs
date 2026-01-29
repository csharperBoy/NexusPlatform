using Core.Domain.Enums;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Server.Collector.Domain.Enums;

namespace Trader.Server.Collector.Application.Commands
{
    
    public record AddStockCommand(
     string Isin ,
     string? Title ,
     decimal? BuyCommissionRate ,
     decimal? SellCommissionRate ,
     StockTypeOfMarketEnum? TypeOfMarket = null,
     TimeOnly? PreOpeningTimeStart = null,
     TimeOnly? PreOpeningTimeEnd = null,
     TimeOnly? OpenTime  =null,
     TimeOnly? CloseTime = null,
     int? TPlus  = null,
     int? MinValueBuyOrder = null,
     int? MinValueSellOrder = null,
     int? StepPrice = null,
     bool? IsActive  = null,
     string? CodeOfTsetmc = null,
     DateOnly? ReleaseDate  = null,
     double? PercentOfDailyTolerance = null
) : IRequest<Result<Guid>>;
}
