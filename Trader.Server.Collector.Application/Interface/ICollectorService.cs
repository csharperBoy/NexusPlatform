using BrokerageOperations.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Server.Collector.Application.DTO;

namespace Trader.Server.Collector.Application.Interface
{
    public interface ICollectorService
    {
        Task SaveSnapShotStockTrading(SaveSnapShotRequest req);
    }
}
