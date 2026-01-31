using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderServer.Domain.Entities;
using TraderServer.Application.Commands;

namespace TraderServer.Application.Interface
{
    public interface IStockService
    {
        Task<Stock> GetStockByISIN(string ISIN);
        Task<Guid> AddStock(AddStockCommand command);
    }
}
