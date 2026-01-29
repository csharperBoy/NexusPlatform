using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Server.Collector.Application.Commands;
using Trader.Server.Collector.Domain.Entities;

namespace Trader.Server.Collector.Application.Interface
{
    public interface IStockService
    {
        Task<Stock> GetStockByISIN(string ISIN);
        Task<Guid> AddStock(AddStockCommand command);
    }
}
